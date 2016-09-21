#!/bin/sh

# Initialize variables
dbhost="10.194.15.241"
outpath=tests
ret=1
run_test=1

# defaults to 3MB
size_limit=3041112
config="test"
type="*"
dir="/pfv/.firmwaretestcebu/pdf/v1_2"
testdir="./tests"
regex="([0-9a-z]+)*"
hits=0
outlog=""
emul="-"
user="chritan"
sniffer=/users/chritan/bin/pdls_sniff
if [ -f "./pdls_sniff" ]; then
   sniffer=./pdls_sniff
fi

declare -a invalid_ext=('zip' 'bat' 'svn' 'ftp' 'exe' 'dl_' 'pcap' 'tmp' 'css' 'chm' 'mst' 'pdb'
  'pjl' 'log' 'fls' 'ucf' 'img' 'lnk' 'cfg' 'lst' 'vsd' 'dmg' 'db' 'dll' 'bin' 'js' 'pl' 'dat'
  'xls' 'doc' 'ppt' 'xlsx' 'docx' 'pptx' 'pptx#' 'rtf' 'xml' 'sh' );
  
if [ -z "$1" ]; then
  module=v1_2
else
  module=$1
fi


# const char * program_name
# int exit_status
help ()
{
   echo "Tanch Unified Resource Tool (tur-tool). a.k.a turtle"
   echo "Test case document profiler/classifier and unit test automation tool.";
   echo "Input directory will be scanned and filtered based on the given key filter string.";

   echo " Usage: ";
   echo "   turtle -t pdf -x [xpsapp|pdls] -s invalid_test -d /pfv/.firmwaretestcebu/pdf/v1_6/annotations";
   echo "";
   echo " Options:";
   echo "    -d, --dir";
   echo "        intput testcase directory.";
   echo "    -x, --exec";
   echo "        executable (absolute path)";
   echo "    -e, --emul=emulator";
   echo "        which Emulator to run.";
   echo "    -f, --force rerun";
   echo "        will delete previous run log file";
   echo "    -t, --type";
   echo "        document type to filter.";
   echo "    -s, --search";
   echo "        input search keyword";
   echo "    -l, --size";
   echo "        file size limit";
   echo "    -db ";
   echo "        target ftp host|ip address";
   echo "    -passwd ";
   echo "        ftp user password";
   echo "    -O, --output-format, defaults to checksum";
   echo "        [pam|none|checksum] Ouput format.";
   echo "";
   exit 0
}

filter_file()
{
  file=$1
  search=$2
  contains=0
  data=`grep -rl  $search $file`
  if [ ${#data} -gt 0 ]; then
    contains=1
  fi
          
  #if grep -q $search "$file"; then
  #  contains=1
  #fi
  
  echo "filtering $file for $search, result is: $contains"
  if [ $contains -eq 1 ]; then
    hits=$((hits+1))
  fi
  
  return $contains
}

classifyPJL()
{
    sed -n /@PJL/p $f >> $outlog
     
    #line=0
    #context=100
    #perl -ne "if ((\$. >= ($line - $context)) && \
    #    (\$. <= ($line + 2 * $context))) { \
    #      echo "[tag with: $file_type]" >> $outlog
    #      printf ('%5d %s ', \$., \$. == $line ? '=>' : '  '); \
    #      print; \
    #}"
   
   return 0
}

# Parameters:
# $1 - input file
runtest()
{
    ret=0
    f="$1"
    filter=$2
    fname=`basename $f | cut -d'.' -f1`
    
    fullname=`basename $f`
    rm -f $fname.cksum
    mkdir -p $outpath/$conf
    
    if [ -f "$testdir/$fullname" ]; then
      if [ "${verbose+defined}" ]; then
        echo " -- $f found skipping tests!"
      fi
      return
    fi
    
    if [ $fullname == "README" ]; then
      if [ "${verbose+defined}" ]; then
       echo "INVALID -- $f: skipping tests!"
      fi
      return
    fi

if [ ! -x "pdls_sniff" ]; then
    pdl_type=`$sniffer $f`
    if [ -z "$pdl_type" ]; then
       return
    fi
    file_type=`basename $pdl_type`
    emul=`dirname $pdl_type`
    
    #$exec -e `$sniffer $f` -O checksum -o $testdir/$conf/$fname- $f &> $outlog
    if [ $file_type == doc* -o $file_type == "MDOC" -o $file_type == "Office2007" -o $file_type == "ppds" ]; then
       echo "$file_type -- skipping, not a valid testfile: $f"
       return
    fi
    
    if [ "${scan_check_only+defined}" ]; then
       echo "calling --pdl $emul --type $file_type --file $f"
       return
    fi

    echo "emul = $emul"    
    if [ $emul == 'XPS' ]; then
       etrace.pl --pdl $emul --exec /users/chritan/bin/xpsapp --file $f | tee $outlog
    else
       etrace.pl --pdl $emul --exec "/users/chritan/bin/pdlsapp" --file $f | tee $outlog
    fi
else
    echo "etrace.pl --exec ./runpage4 --file $f | tee $outlog"
    etrace.pl --exec ./runpage4 --file $f | tee $outlog
fi

    # special case for PJL as its not part of emulator
    # we classify it manually here. 
    sed -n /@PJL/p $f >> $outlog
    
    echo "[tag with: $file_type]" >> $outlog

    # parse output log for keyword/tag      
    if [ -f "$outlog" ]; then
      if [ ${filter+defined} ]; then
        filter_file $outlog $filter
        if [ $? -eq 1 ]; then
          echo "[tag with: $filter]" >> $outlog
          ret=1
        fi
      fi
    fi
    return $ret
}

getparam() 
{
  ret=1
  if [ "${1+defined}" ]; then
    ret="$1"
    return 1
  else
    echo "Error: missing argument!"
    return 0
  fi 
}

if [ -z "$1" ]; then
   help $0 0
   return 1
fi

while [ "${1+defined}" ]; do
    case "$1" in
      -h | --help)
        help $0 0
        ;;
      --) # End of all options
        break
       ;;
      -v | --verbose)
        verbose=1 
        ;;
      --check)
        scan_check_only=1
        ;;
      -f | --force)
        force_run=1
        ;;
      -d | --dir)
        if [ "${2+defined}" ]; then
          dir="$2"
          shift
        else
          echo "Error: source testcase directory!"
          echo "e.g $0 -d /pfv/.firmwaretestcebu/pdf/v1_2"
          exit 0
        fi 
        ;;
      -sniffer)
        if [ "${2+defined}" ]; then
          sniffer="$2"
          shift
        fi 
        ;;
      -x | --exec)
        if [ "${2+defined}" ]; then
          exec="$2"
          shift
        else
          echo "Error: testdir test directory!"
          echo "e.g $0 -p /bonus/scratch/tanch/pdls/tests"
          exit 0
        fi 
        ;;
      -e | --emul)
        if [ "${2+defined}" ]; then
          emul="$2"
          shift
        else
          echo "Error: emulator type required!"
          exit 0
        fi 
        ;;
      -t | --type)
        if [ "${2+defined}" ]; then
          type="$2"
          shift
        else
          echo "Error: missing document type filter argument!"
          exit 0
        fi 
        ;;
      -s | --search)
        if [ "${2+defined}" ]; then
          input_filter="$2"
          shift
        else
          echo "Error: file search keyword missing!"
          exit 0
        fi
        ;; 
      -l | --size)
        if [ "${2+defined}" ]; then
          size_limit="$2"
          shift
        else
          echo "Error: missing limit size!"
          exit 0
        fi
        ;;
      -user)
        if [ "${2+defined}" ]; then
          user="$2"
          shift
        else
          echo "Error: missing argument value!"
          exit 0
        fi ;;
      -passwd)
        if [ "${2+defined}" ]; then
          passwd="$2"
          shift
        else
          echo "Error: missing argument value!"
          exit 0
        fi ;;
      -db | --dbhost)
        if [ "${2+defined}" ]; then
          dbhost="$2"
          shift
        else
          echo "Error: missing value for host!"
          exit 0
        fi
        ;;
      -*)
        echo "Error: Unknown option: $1" >&2
        exit 1 ;;
      *)  # No more options
        break ;;
    esac
    shift
done

SAVEIFS=$IFS

#if [ ! "${exec+defined}" ]; then
#   echo "ERROR: exec path (-x option) not specified!"
#   exit 0
#fi

#if [ ! -x "$exec" ]; then
#  echo "ERROR: invalid pdls binary (-p option)!"
#  exit 0
#fi

IFS='|' read -ra filters <<< "$input_filter"

echo "path: $dir"
echo "document type: $type"
echo "config: $testdir/$conf.config"
echo "executable: $exec"

#mysql --host=10.194.15.187 --user=tanch --password=tanch tutorial_db << EOF
#insert into files (fname,floc) values('samplefile', 'http://www.site.com/'); << EOF
#quit << EOF

IFS=$(echo -en "\n\b")
mkdir -p $testdir/$conf

for f in `find $dir -type f -name "*$type"` ; do
  ext="${f##*.}"
  if [[ " ${invalid_ext[*]} " == *$ext* ]]; then
     if [ "${verbose+defined}" ]; then
        echo "$ext: Skipping invalid: $f"
     fi
     continue
  fi 

  if [[ $f == *".svn"* ]]; then
     echo "Skipping invalid: $f"
     continue
  fi
  
  outlog=`echo "$f" | md5sum`
  [[ $outlog =~ $regex ]]
  outlog="${BASH_REMATCH[1]}.log"
  if [ -f $outlog ]; then
     if [ "${force_run+defined}" ]; then
      rm -f $outlog
     else
      echo "Skipping: $f , duplicate file: $outlog" 
      continue
     fi
  fi
  
  actualsize=$(wc -c <"$f")
  if [ $actualsize -ge $size_limit ]; then
    if [ "${verbose+defined}" ]; then
      echo Skipping: $f, size is over $size_limit bytes
    fi
    continue
  fi

  START_TIME=$(date +"%D %T")

  # step 1: run executable on testcase file!
  runtest $f $filters
  if [ $? -eq 0 ]; then
     #rm -f $outlog
     continue;
  fi

  if [ ! -f "$outlog" -o ! -s "$outlog" ]; then
     continue;
  fi
  
  echo "[@size: $actualsize]" >> $outlog
  echo "[@startTime: $START_TIME]" >> $outlog  
  # step 2: lookup tag/keywork inside testcase file
  for i in "${filters[@]}"; do
    filter_file $f $i
    if [ $? -eq 1 ]; then
      echo "[tag with: $i]" >> $outlog
    else
      continue
    fi
  done
  
  END_TIME=$(date +"%D %T") 
  echo "[@endTime: $END_TIME]" >> $outlog
  
  if [ "${passwd+defined}" ]; then
    echo "sendftp -h $dbhost -u $user -p $passwd -f $outlog -d turtle/data"
    sendftp -h $dbhost -u $user -p $passwd -f $outlog -d turtle/data
    if [ $? -ne 0 ]; then
       echo "Error: FTP failed!"
    fi
  else
    echo "Error: sendftp -h $dbhost -u $user -p $passwd -f $outlog -d turtle/data"
  fi
#  break
done

echo "SEARCH HITS: $hits"

IFS=$SAVEIFS 
exit 0



