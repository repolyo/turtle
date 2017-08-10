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

# TODO: move this to config file
libsymlink      = "|awk '{print $3 \" \" $4}'"
libfilter       = "|awk '/printservice|jobsystem|pullprint/'"
#librmdup        = "|awk '!NF || !seen[$0]++'"
#rmheader        = "|awk '!/..h:/'"
unallowedtag    = 'R|r|U|u|W|w'
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
            exe = ['hydra', 'pagemaker/pagemaker']
        elif "./" in executable:
            exe = executable[2:].split()
        else:
            exe =   executable.split()
        return exe

    def getliboffsets(self, executable):
        lddcommand  = "ldd "
        lcommand    = lddcommand + executable + libsymlink + libfilter
        libdict = {}
        offset_address  = subprocess.check_output(lcommand, shell=True)
        for line in offset_address.splitlines():
            if line:
                lib, addr = line.split()
                libdict[lib] = str(addr[1:len(addr) -1])
        return libdict

    def readobjects(self, executable):
        symboltable = {}
        nmcommand   = "nm -l --numeric-sort "
        lddcommand  = "ldd "
        tpattern    = re.compile(unallowedtag)
        ppattern    = re.compile(pathallowed)

        exe = self.getexecutable(executable)
        exe = ' '.join(exe)
        print("Reading objects from [" + exe + "]...")

        lcommand = nmcommand + exe
        address_symbols = subprocess.check_output(lcommand, shell=True)
        for line in address_symbols.splitlines():
            if line:
                words = line.split()
                if len(words) == 4 and words[3]:
                    tmatch = re.search(tpattern, words[1])
                    pmatch = re.search(ppattern, words[3])
                    if tmatch or not pmatch or ".L" in words[2]:
                        continue
                    rop = words[0]
                    value="[" + words[3] + "] => " + words[2]
                    if self.ishex(rop):
                        key=str(int(rop, 16))
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

        print("Reading objects from [" + ' '.join(exe) + "]...")
        tee.write("Reading objects from [" + ' '.join(exe) + "]...\n\n")

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
