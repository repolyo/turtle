#!/usr/bin/env python

#
#  08/04/17 18:27:26
#  chritan@lexmark.com

import sys, os, subprocess, re, atexit, time, ftplib, argparse, traceback, shelve
import argparse
from readobjectsymbol import ReadObjectSymbol, CLIParser
from readlibtrace import LibraryMap
import turtleconfig as tconfig

#Defined variables
#
fhost = "turtle.lrdc.lexmark.com";
fuser = "anonymous";
fpass = "";

hydra="./hydra"
logfile = "hydra.log"
tracefile = "trace.out"
fakesys = "./tests/fakesys"
all_processes = []
func_dict = None

def file_exist (fpath):
    return os.path.isfile(fpath)

def is_exe(fpath):
    return file_exist (fpath) and os.access(fpath, os.X_OK)

def del_file (file):
    try:
        os.remove(file)
    except OSError:
        pass

def to_key (key):
    return str (int(str (key), 16))

def md5sum (file):
    return shell_command ( "md5sum " +file ).split()[0].rstrip()

def shell_command(cmd):
    try:
       #print "tanch@shell_command ("+ cmd + ")"
       p = subprocess.Popen(cmd.split(), stdin=subprocess.PIPE, stdout=subprocess.PIPE,stderr=subprocess.PIPE)
       (out,err) = p.communicate(cmd)

       if p.returncode==0:
          output=str(out).rstrip()
       else:
          print ("failed to encode, exit-code=%d, error=%s" % (p.returncode,str(err)))
    except OSError as e:
       sys.exit("Error: %s" % (str(e)))
    return output

def load_symbols (dict, bin):
    ret = 0
    readsymbol = ReadObjectSymbol()
    symboltable = readsymbol.readobjects(bin)
    for key in symboltable:
        dict[key] = symboltable[key]
        ret += 1

    return ret

def upload_file (host, user, passw, fpath):
    dir="turtle/data"
    try:
        ftp = ftplib.FTP(host, user, passw)
        ftp.cwd(dir)
        fh = open(fpath, 'rb')
        ftp.storbinary('STOR ' + fpath, fh)
        fh.close
        ftp.quit()
    except ftplib.all_errors, e:
        print str(e)         

def cleanup():
    timeout_sec = 5
    # list of your processes
    for p in all_processes:
        p_sec = 0
        print str(p) + ' killed...'
        for second in range(timeout_sec):
            if p.poll() == None:
                time.sleep(1)
                p_sec += 1
        # supported from python 2.6
        if p_sec >= timeout_sec:
            p.kill()

    print 'cleaned up!'

def execute (dict, command):
    process = subprocess.Popen(command, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    output = ''
#trace_dict = { } # keep records to avoid duplicates
#func_addr_pattern = re.compile(r'^E (0?x?[0-9a-fA-F]+)$')
    
    # Poll process for new output until finished
    for line in iter(process.stdout.readline, ""):
#        match = re.search(func_addr_pattern, line)
#        if match:
#            key = to_key (match.group(1))
#            if key in trace_dict:
#                continue
#
#            trace_dict[key] = 1
#            if dict.has_key(key):
#                func = dict[key]
#                line = func[1] + " => " + func[0] + "\n"
#            else:
#                continue
        print line,
        output += line

    process.wait()
    exitCode = process.returncode

    if (exitCode == 0):
        return output
    else:
        raise Exception(command, exitCode, output)

def send_file (fdict, testcase, logfile):
    trace_dict = { } # keep records to avoid duplicates
    func_addr_pattern = re.compile(r'^E (0?x?[0-9a-fA-F]+)$')
    libmap           = LibraryMap()
    
    # simulate network stream 
    f = open('network0', 'w+')
    f.write("T0\n")
    f.write("F"+testcase+"\n")
    f.write("Q\n")
    f.close()
    
    cmd = "./run_testcase " + testcase + " | tee " + logfile
    print "Invoking: " + cmd 
    
    execute (fdict, [ hydra + " | tee " + logfile ])

    offsetaddressdict = libmap.getmapaddresses()
    libminmax         = libmap.sortoffsetaddresses(offsetaddressdict)
    with open(tracefile, 'r') as rfile:
        lines = rfile.readlines()
        with open(logfile, "a") as myfile:
            for line in lines:
                match = re.search(func_addr_pattern, line)
                if match:
                    key = to_key (match.group(1))
                    if key in trace_dict:
                        continue
                    trace_dict[key] = 1

                    if libminmax[0] <= key <= libminmax[-1]:
                        key = libmap.checklibaddress(offsetaddressdict, key)
                    if fdict.has_key(key):
                        line = fdict[key]+"\n";
                    else:
                        continue
                    myfile.write(line)
    del_file (tracefile)

def argsParser():
    parser = argparse.ArgumentParser()
    parser.add_argument("-x", "--bin", default=[], type=str, nargs='+', help="executable/s (absolute path)")
    parser.add_argument("-R", "--resolution", default=None,       help="Set default resolution in dots per inch. This can be overriden by the job.")
    parser.add_argument("-F","--ftp",   default=None,       help="target ftp host|ip address")
    parser.add_argument("-f", "--file", default=None,    help="test file")
    if len(sys.argv) == 1:
        parser.print_help()
        sys.exit()
    return parser

def main():
    pid = None
    parser=argsParser()
    args = parser.parse_args()

    if args is None:
        parser.print_help()

    if not is_exe (fakesys):
        print "ERROR: Can't find fakesys!"
        sys.exit(1)

    print args
    
    func_dict = shelve.open("symbols.dat")
    func_count = len (func_dict)
            
    os.environ["PRINT_DOC_OUTPUT_FORMAT"] = "p"
    os.environ["DRIVER_TO_FAX_DOC_OUTPUT_FORMAT"] = "p"
    
    # This keeps the simengine-backend from popping up its graphical user
    # interface window
    os.environ["DISPLAY"] = ""
    
    # in this case we also assume the user wants to use the local
    # copy of pagemaker.  tell rob how to find that.
    os.environ["ROB_ACTIVATION_CONFIG_DIR"] = "./test-activation"

    # Point to the test version of cfstab which is assumed to
    # be in the test directory under the hydra executable.
    # This sets up the correct font directory et al.
    os.environ["CFS_TAB_FILE"] = shell_command ("dirname "+ shell_command ("readlink -f "+hydra ) ) + "/tests/cfstab"

    # must set up the library search path to find hydra's shared libraries.
    os.environ["LD_LIBRARY_PATH"] = ""

    # Default for this test is to disable scanning applications
    # and to run hydra in SFP-only mode to test printing.
    os.environ["SCAN_DISABLED"] = "1"
    os.environ["FAX_DISABLED"] = "1"
            
    atexit.register(cleanup)
    logfile = md5sum ( args.file ) + ".log"
        
    try:
        for binary in args.bin:
            if not is_exe (binary):
                print "Can't find " + binary + " in current directory"
                sys.exit(1)
            
            if func_dict.has_key(binary):
                print binary + " entry count: " + str (func_dict[binary])
            else:
                func_dict[binary] = load_symbols (func_dict, binary)

        if file_exist (logfile):
            print "Duplicate file: " + logfile
            sys.exit(1)

        del_file (tracefile)

        print "Starting mock system..."
        pid = str(subprocess.Popen([fakesys]).pid)
        print fakesys + " ("+str(pid)+")"

        send_file (func_dict, args.file, logfile)
        
        upload_file('turtle.lrdc.lexmark.com', 'anonymous', '.',  logfile)
        
    except:
        print "Exception in user code:"
        print '-'*60
        traceback.print_exc(file=sys.stdout)
        print '-'*60
    finally:
        func_dict.close()       # close it
    
    if pid is not None and pid:
        os.system("kill -9 " + pid)
    
    sys.exit()
    
if __name__ == "__main__":
    main()
