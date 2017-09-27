#!/usr/bin/env python

#
#  08/24/17 13:25:26
#  vgelig@lexmark.com

import sys, os, subprocess, re, shelve
import turtleconfig as tconfig

#Defined variables
#
def deletefile (ffile):
    try:
        os.remove(ffile)
    except OSError:
        pass

class LibraryMap:
    def __init__(self):
        pass

    def ishex(self, address):
        try:
            int(address, 16)
            return True
        except ValueError:
            return False

    def sortoffsetaddresses(self, fdict):
        addrlist = []
        for sort in fdict:
            if "trace.map" in sort:
                continue
            addrlist = addrlist + fdict[sort]
        return sorted(addrlist)

    def removetracechecksum(self, fdict):
        newdict = {}
        for libname in fdict:
            if "trace.map" in libname:
                continue
            newdict[libname] = fdict[libname]
        return newdict
    
    def checklibaddress(self, fdict, addr):
        return_addr = addr
        for libname, librange in fdict.items():
            start = None
            faddr = int(addr)
            start = int(librange[0])
            end   = int(librange[-1])
            if start > faddr  or faddr > end:
                continue
            # loop through library's range
            count = 0
            while count < len(librange)/2:
                index = count * 2
                start = int(librange[index])
                end   = int(librange[index + 1])
                if start <= faddr < end:
                    libnamenoext = '.'.join(libname.split('.')[0:-1])
                    tmp_addr = (faddr - start) + int(tconfig.library_offset[libnamenoext], 16)
                    return_addr = str(tmp_addr)
                    break
                count += 1
        return return_addr

    def createmapaddress(self, fdict, filename):
        pattern    = re.compile(''.join(tconfig.lib_subsystems))
        extension  = filename.split('.')[-1]
        prevlibname = None
        with open(filename, 'r') as tfile:
            lines = tfile.readlines()
            for line in lines:
                match = re.search(pattern, line)
                if match:
                    words = line.split()
                    libname = words[-1].strip().split('/')[-1]
                    if libname in tconfig.library_offset:
                        if prevlibname != libname:
                            refreshlib = True
                        offsetaddrmin = words[0].split()[0].split('-')[0]
                        offsetaddrmax = words[0].split()[0].split('-')[1]
                        if self.ishex(offsetaddrmin) and self.ishex(offsetaddrmax):
                            libnameextend = libname + "." + extension

                            if libnameextend not in fdict or refreshlib:
                                fdict[libnameextend] = [str(int(offsetaddrmin,16)), str(int(offsetaddrmax,16))]
                                prevlibname = libname
                                refreshlib = False
                            else:
                                temp = fdict[libnameextend]
                                temp.append(str(int(offsetaddrmin,16)))
                                temp.append(str(int(offsetaddrmax,16)))
                                fdict[libnameextend] = temp

    def getmapaddresses(self):
        deletefile("hydra-checksum.dat")
        fdict          = shelve.open("hydra-checksum.dat")
        hydratrace     = "trace.map.hydra"
        pagemakertrace = "trace.map.pagemaker"

        command  = "md5sum trace.map.hydra | awk '{print $1}'"
        result   = subprocess.check_output(command, shell=True)
        checksum = result.strip()
        hydramapnotequal = True
        if hydratrace in fdict:
            hydramapnotequal = False
            if fdict[hydratrace] != checksum:
                hydramapnotequal = True
        if hydramapnotequal:
            self.createmapaddress (fdict, hydratrace)
            fdict[hydratrace] = checksum
        self.createmapaddress (fdict, pagemakertrace)
        temp = self.removetracechecksum(fdict)
        return temp


if __name__ == "__main__":
    librarymap = LibraryMap()
    deletefile("hydra-checksum.dat")

    fdict = librarymap.getmapaddresses()
    for addr, lrange in fdict.items():
        print addr + ": " + ','.join(lrange)
