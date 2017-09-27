#!/usr/bin/env python
################################################################################
#
#   Read Object Symbol
#
#   Copyright 2017 Lexmark International, Inc.
#   All Rights Reserved. Proprietary and Confidential.
#
#   vgelig@lexmark.com
#   2017-08-04
#
################################################################################

import sys, os, subprocess, argparse, hashlib, re
import turtleconfig as tconfig
# TODO: move this to config file
libsymlink      = "|awk '{print $3 \" \" $1}'"
libfilter       = "|awk '/" + ''.join(tconfig.lib_subsystems) + "/'"
librmdup        = "|awk '!NF || !seen[$0]++'"
unallowedtag    = 'R|r'
pathallowed     = ':+[0-9]+$'



class ReadObjectSymbol:
    def __init__(self):
        pass

    def ishex(self, address):
        try:
            int(address, 16)
            return True
        except ValueError:
            return False

    def getexecutable(self, executable):
        if "hydra" in executable:
            exe = "hydra pagemaker/pagemaker"
        elif "./" in executable:
            exe = executable[2:]
        else:
            exe =   executable
        return exe

    # Get the library paths
    # Add manually defined offsets from turtleconfig.py
    def getliboffsets(self, executable):
        lddcommand  = "ldd "
        lcommand    = lddcommand + executable + libsymlink + libfilter + librmdup
        libdict     = {}

        offset_address  = subprocess.check_output(lcommand, shell=True)
        for line in offset_address.splitlines():
            if line:
                lib, addr = line.split()
                libdict[lib] = tconfig.library_offset[addr]
        return libdict

    # Read the demangled symbols from the executable and the libraries
    def readobjects(self, executable):
        symboltable = {}
        nmcommand   = "nm -lC --print-file-name --numeric-sort "
        tpattern    = re.compile(unallowedtag)
        ppattern    = re.compile(pathallowed)
        exe         = self.getexecutable(executable)
        liboffset   = self.getliboffsets(exe)

        rcommand    = ' ' + ' '.join(liboffset.keys())
        lcommand    = nmcommand + exe + rcommand

        print("Reading objects from [" + exe + "]...")
        address_symbols = subprocess.check_output(lcommand, shell=True)
        for line in address_symbols.splitlines():
            if line:
                words = line.split()
                libaddr = words[0].split(':')[0]
                libname = ''.join(libaddr.split('/')[-1:])
                addr = words[0].split(':')[1]
                if len(words) == 4 and words[3]:
                    tmatch = re.search(tpattern, words[1])
                    pmatch = re.search(ppattern, words[3])
                    libaddr = words[0].split(':')[0]
                    libname = ''.join(libaddr.split('/')[-1:])
                    addr = words[0].split(':')[1]
                    if tmatch or not pmatch or ".L" in words[2] or "::" in words[2]:
                        continue
                    if libname in tconfig.library_offset:
                        lop = liboffset[libaddr]
                        rop = addr
                    else:
                        lop = '00'
                        rop = addr

                    value="[" + words[3] + "] => " + words[2]
                    if self.ishex(lop) and self.ishex(rop):
                        key=str(int(lop ,16) + int(rop, 16))
                        symboltable[key] = value

        return symboltable

    def parsesymboltable(self, symboltablefile):
        symboltable = {}

        if symboltablefile:
            print("Reading objects from [" + symboltablefile + "]...")
            with open(symboltablefile, 'r') as rfile:
                rline = rfile.readlines()
                for line in rline:
                    words = line.strip('\n').split(',')
                    if len(words) > 1:
                        if self.ishex(words[0]):
                            key = str(int(words[0] ,16))
                            value = ' '.join(words[1:])
                            symboltable[key] = value
        return symboltable

    def printsymboltable(self, executable, symboltable):
        logfile = self.getlogfile(executable)
        exe     = self.getexecutable(executable)
        tee     = open(logfile, 'a')

        print("Reading objects from [" + exe + "]...")
        tee.write("Reading objects from [" + exe + "]...\n\n")

        for key in sorted(symboltable):
            print "%s: %s" % (key, symboltable[key])
            tee.write(key + ',' + symboltable[key] + '\n')
        tee.write("Dictionary keys count: " + str(len(symboltable)))
        tee.flush()
        tee.close()
        return logfile

    def getlogfile(self, executable):
        logfile = hashlib.md5(executable).hexdigest() + '.log'
        if os.path.exists(logfile):
            print "Delete existing " + logfile
            os.remove(logfile)
        return logfile

class CLIParser:
    def __init__(self):
        pass

    def argsParser(self):
        parser = argparse.ArgumentParser()
        parser.add_argument("-x", "--exe",  default=None,       help="executable (absolute path)")
        parser.add_argument("--etrace",     default=None,       help="symbol table log. ")
        parser.add_argument("-f", "--force",action='store_true',help="rerun - will delete previous run log file")
        if len(sys.argv) == 1:
            parser.print_help()
            sys.exit()
        return parser


if __name__ == "__main__":
    cliparser  = CLIParser()
    readsymbol = ReadObjectSymbol()

    parser = cliparser.argsParser()
    args = parser.parse_args()
    symboltable=dict()

    if args.etrace and os.path.exists(args.etrace) and not args.force:
        # parse etrace only when not forced
        symboltable = readsymbol.parsesymboltable(args.etrace)
    elif args.exe:
        # read objects from binary and filtere libraries
        symboltable = readsymbol.readobjects(args.exe)
    #print symbol table to file
    logfile = readsymbol.printsymboltable(args.exe, symboltable)
    print logfile
