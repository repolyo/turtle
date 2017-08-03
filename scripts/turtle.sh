#!/bin/sh

# Initialize variables
dbhost="turtle.lrdc.lexmark.com"
outpath=tests
ret=1
run_test=1

# defaults to 40MB
size_limit=41943040
config="test"
type="*"
dir="/pfv/.firmwaretestcebu/pdf/v1_2"
testdir="./tests"
regex="([0-9a-z]+)*"
hits=0
outlog=""
emul="-"
user="anonymous"
passwd="."
persona="pdlsapp"
input_filter="pdls"
resolution=600
skip_files="turtle.skip"
sniffer=/users/chritan/bin/pdls_sniff
if [ -f "tests/snifftype" ]; then
   sniffer=tests/snifftype
elif [ -f "./pdls_sniff" ]; then
   sniffer=./pdls_sniff
fi

declare -a invalid_ext=('zip' 'bat' 'svn' 'ftp' 'exe' 'dl_' 'pcap' 'tmp' 'css' 'chm' 'mst' 'pdb'
  'pjl' 'log' 'fls' 'ucf' 'img' 'lnk' 'cfg' 'lst' 'vsd' 'dmg' 'db' 'dll' 'bin' 'js' 'pl' 'dat'
  'xls' 'doc' 'ppt' 'xlsx' 'docx' 'pptx' 'pptx#' 'rtf' 'xml' 'sh' 'jar' 'bak' );
  
declare -a ext_table=(
        'pdf'  # PDF
        'ps'  # Postscript
        'ps2' # XPS
        'jpg'          # jpeg
        'gif' # GIF
        'tiff' # tiff
        'png' # png
        'xl'      # XL
        'txt'      # PPDS
        'html'            # HTML
);

declare -a emul_table=(
        'PDF/pdf' 
        'PS/ps' 
        'PS/ps' 
        'DI/di' 'DI/di' 'DI/di' 'DI/di' 
        'XL/xl'
        'PCLXL/pcl'
        'HTML/html'
);

if [ -z "$1" ]; then
  module=v1_2
else
  module=$1
fi

pdl_type=""

sniff_type()
{
    file=$1
    echo "testing: $file"
    pdl_type="XPS/ps"
    for((i=0;i<${#ext_table[@]};i++))
    do
      echo "$i: ${ext_table[$i]}"
      if [[ "$file" == *${ext_table[$i]} ]] 
      then
          pdl_type=${emul_table[$i]}
          return
      fi
    done
}


# const char * program_name
# int exit_status
help ()
{
   echo "Emula[tors] tool (turtle)"
   echo "Test case document profiler/classifier and unit test automation tool.";
   echo "Input directory will be scanned and filtered based on the given key filter string.";

   echo " Usage: ";
   echo "   turtle -t pdf -x [xpsapp|pdls] -p sim-palazzo -s invalid_test -d /pfv/.firmwaretestcebu/pdf/v1_6/annotations";
   echo "";
   echo " Options:";
   echo "    -d, --dir";
   echo "        intput testcase directory.";
   echo "    -x, --exec";
   echo "        executable (absolute path)";
   echo "    -e, --emul=emulator";
   echo "        which Emulator to run.";
   echo "    -p, --persona";
   echo "        build persona";
   echo "    -f, --force rerun";
   echo "        will delete previous run log file";
   echo "    -t, --type";
   echo "        document type to filter.";
   echo "    -s, --search";
   echo "        input search keyword";
   echo "    -l, --size";
   echo "        file size limit";
   echo "    -R, --resolution";
   echo "        Set default resolution in dots per inch. This can be overriden by the job.";
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

    pdl_type=`$sniffer $f`
    if [ -z "$pdl_type" ]; then
       # determine by file extension
       sniff_type $f
    fi

    if [ $emul == "-" ]; then

       if [ -z "$pdl_type" -o $pdl_type == "x/PJL" ]; then
          echo "ERROR: Unable to detect emulator type($pdl_type): $f"
          return
       fi
       emul=`dirname $pdl_type`
       if [ $emul == "." ]; then
          emul=$pdl_type
       fi
    fi

    file_type=`basename $pdl_type`
    
    #$exec -e `$sniffer $f` -O checksum -o $testdir/$conf/$fname- $f &> $outlog
    if [ $file_type == doc* -o $file_type == "MDOC" -o $file_type == "Office2007" -o $file_type == "ppds" ]; then
       echo "$file_type -- skipping, not a valid testfile: $f"
       return
    fi
    
    if [ "${scan_check_only+defined}" ]; then
       echo "calling --pdl $emul --type $file_type --file $f"
       return
    fi

    echo "emul = $emul $pdl_type"    
    if [ "$emul" == "XPS" -a ! -f xpsapp/xpsapp ] ; then
       echo "ERROR: could not find xpsapp binary"
       return
    fi

if [ -x $sniffer ]; then

    if [ ! "${exec+defined}" ]; then
       if [ $emul == 'XPS' ]; then
          exec="/users/chritan/bin/xpsapp"
       else
          exec="/users/chritan/bin/pdlsapp"
       fi
    fi
    
else
    exec="./runpage4"
fi

    echo "/users/chritan/bin/ftrace.pl --pdl $emul --exec $exec --resolution $resolution --file $f --persona $persona | tee $outlog"
    /users/chritan/bin/ftrace.pl --resolution $resolution --pdl $emul --exec $exec --file "$f" --persona $persona | tee $outlog

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
      -p | --persona)
        if [ "${2+defined}" ]; then
          persona="$2"
          shift
        fi 
        ;;
      -R | --resolution)
        if [ "${2+defined}" ]; then
          resolution="$2"
          shift
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

blacklist_file=blacklist.txt
declare -a blacklist

if [ -f $blacklist_file ]; then
   blacklist=( `cat "$blacklist_file"`)
   printf "Blacklist: $blacklist_file\n"
   for i in "${blacklist[@]}"
   do
       printf "\t $i\n"
   done
fi


for f in `find $dir -size -$size_limit -type f -name "*$type"` ; do
  loc=${f%/*}
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

  if [[ " ${blacklist[*]} " == *$loc* ]]; then
     echo "Blacklisted: Skipping $f ..."
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
    echo Skipping: $f, size is over $size_limit bytes
    echo "$f" >> $skip_files
    continue
  fi

  START_TIME=$(date +"%D %T")

  # step 1: run executable on testcase file!
  runtest $f $filters
  filter_found=$?
  END_TIME=$(date +"%D %T") 

  if [ ! -f "$outlog" -o ! -s "$outlog" ]; then
     continue;
  fi

  echo "[@size: $actualsize]" >> $outlog
  echo "[@startTime: $START_TIME]" >> $outlog  
  echo "[@endTime: $END_TIME]" >> $outlog

  # step 2: lookup tag/keywork inside testcase file
  for i in "${filters[@]}"; do
    filter_file $f $i
    if [ $? -eq 1 ]; then
      echo "[tag with: $i]" >> $outlog
      filter_found=1
    fi
  done
  
  if [ $filter_found -eq 1 ]; then
     echo "sendftp -h $dbhost -f $outlog -d turtle/data"
     sendftp -h $dbhost -u $user -p $passwd -f $outlog -d turtle/data
     if [ $? -ne 0 ]; then
        echo "Error: FTP failed!"
     fi
  fi
done

echo "SEARCH HITS: $hits"

IFS=$SAVEIFS 
exit 0



