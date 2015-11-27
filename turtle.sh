#!/bin/sh

# Initialize variables
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

if [ -z "$1" ]; then
  module=v1_2
else
  module=$1
fi


# const char * program_name
# int exit_status
help ()
{
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
   echo "    -t, --type";
   echo "        document type to filter.";
   echo "    -s, --search";
   echo "        input search keyword";
   echo "    -l, --size";
   echo "        file size limit";
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

# Parameters:
# $1 - input file
runtest()
{
    f=$1
    filter=$2
    fname=`basename $f | cut -d'.' -f1`
    
    fullname=`basename $f`
    rm -f $fname.cksum
    mkdir -p $outpath/$conf
    
    if [ -f "$testdir/$fullname" ]; then
       echo " -- $f found skipping tests!"
       return
    fi

    #$exec -e `pdls_sniff $f` -O checksum -o $testdir/$conf/$fname- $f &> $outlog
    if [ $type == 'xps' ]; then
       etrace.pl --exec $exec --file $f --ftp 10.194.15.187 --user chritan --pass welcome1 | tee $outlog
    else
       etrace.pl --exec ./pdls --file $f --ftp 10.194.15.187 --user chritan --pass welcome1 | tee $outlog
    fi
    
    # parse output log for keyword/tag      
    if [ -f "$outlog" ]; then
      if [ ${filter+defined} ]; then
        filter_file $outlog $filter
        if [ $? -eq 1 ]; then
          echo "[tag with: $filter]" >> $outlog
        fi
      fi
    fi
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

while [ "${1+defined}" ]; do
    case "$1" in
      -h | --help)
        help $0 0
        ;;
      -v | --verbose)
        verbose="verbose" 
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
      --) # End of all options
        break ;;
      -*)
        echo "Error: Unknown option: $1" >&2
        exit 1 ;;
      *)  # No more options
        break ;;
    esac
    shift
done

SAVEIFS=$IFS

if [ ! "${exec+defined}" ]; then
   echo "ERROR: exec path (-x option) not specified!"
   exit 0
fi

if [ ! -x "$exec" ]; then
  echo "ERROR: invalid pdls binary (-p option)!"
  exit 0
fi

if [ ! "${input_filter+defined}" ]; then
  echo "ERROR: Filter not specified"
  exit 0
fi

IFS='|' read -ra filters <<< "$input_filter"

echo "directory: $dir"
echo "document type: $type"
echo "config: $testdir/$conf.config"
echo "pdls: $exec"

#mysql --host=10.194.15.187 --user=tanch --password=tanch tutorial_db << EOF
#insert into files (fname,floc) values('samplefile', 'http://www.site.com/'); << EOF
#quit << EOF

IFS=$(echo -en "\n\b")
mkdir -p $testdir/$conf

for f in `find $dir -type f -name "*.$type"` ; do
  outlog=`echo "$f" | md5sum`
  [[ $outlog =~ $regex ]]
  outlog="${BASH_REMATCH[1]}.log"
  if [ -f $outlog ]; then
     echo "Skipping: $f, duplicate file: $outlog" 
     continue
  fi
  
  actualsize=$(wc -c <"$f")
  if [ $actualsize -ge $size_limit ]; then
    echo Skipping: $f, size is over $size_limit bytes
    continue
  fi

  START_TIME=$(date +"%D %T")

  # step 1: run executable on testcase file!
  runtest $f $filters

  echo "[@startTime: $START_TIME]" >> $outlog  
  # step 2: lookup tag/keywork inside testcase file
  for i in "${filters[@]}"; do
    filter_file $f $i
    if [ $? -eq 1 ]; then
      #echo "invoking = ./pdls -e `pdls_sniff $f` $f" > $outlog
      echo "[tag with: $i]" >> $outlog
    fi
  done
  
  END_TIME=$(date +"%D %T") 
  echo "[@endTime: $END_TIME]" >> $outlog
  sendftp -h 10.194.15.187 -u chritan -p welcome1 -f $outlog
#  break
done

echo "SEARCH HITS: $hits"

IFS=$SAVEIFS 
exit 0



