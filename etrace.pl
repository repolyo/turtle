#!/usr/bin/perl
#
# etrace.pl script

$NM  = 'nm -l ';     # unix nm utility to get symbol names and addresses
                  # -l, --line-numbers Use debugging information to find a filename

# Output format
$TAB1 = "";            # indentation: function still on the stack   
$TAB2 = "";           # indentation: function just placed on the stack
$UNDEFINED_SYMBOL = "???"; # when an offset cannot be mapped to a symbol

# Input format
$FIFO_NAME = "trace.out";             # default FIFO name
$REFERENCE_OFFSET = "REFERENCE:"; # marks a known symbol/object pair for dynamic libraries
$FUNCTION_ENTRY = "E";        # marks entry into a function
$FUNCTION_EXIT  = "L";        # marks exit from function
$END_TRACE      = "EXIT";         # end of input
$HEX_NUMBER = '0?x?[0-9a-fA-F]+'; # hex number
$SYMBOL_NAME = '[\w@.]+';         # valid identifier characters
$SRC_LOC = '[0-9a-fA-F]+';

$exec;
$testcase;
$runpage4 = false;
$fhost = "10.194.15.187";
$fuser = "anonymous";
$fpass = "";
$emul = "-";

# Global identifiers
# %SYMBOLTABLE : a hash array from relative offsets to symbol names
# $IS_FIFO :     are we reading from a FIFO or a file?
# CALL_DATA :    file handle to input file/FIFO

sub readObjects {
  $objectFileName = shift;
  if (-x $objectFileName) {
    # Object code: extract symbol names via a pipe before parsing
    if ( $runpage4 eq true ) {
       $handleName = $NM. ' Page4 |';
    }
    else {
       $handleName = $NM.$objectFileName . '|';
    }
  } else {
    # A symbol table: simply parse the symbol names
    $handleName = '<'.$objectFileName;
  };
  
  open f, $handleName or die "$0: cannot open $objectFileName";
  while ($line = <f>) {
    #print $line ;
    $line =~  m/^($HEX_NUMBER)\s+.*\s+($SYMBOL_NAME)\s+(.*)/g;
    $hexLocation = $1;
    $symbolName = $2;
    $location = hex $hexLocation;
    $SYMBOLTABLE{$location} = $2 . " " .$3;
  }
  close f;
}

sub writeSymbolTable {
  while ( ($location,$name) = each %SYMBOLTABLE) {
    print "[$location] => $name\n";
  }
}; 

sub dumpCallFunc {
  while ( ($location,$name) = each %CALL_TABLE) {
    print "[$location] => $name\n";
  }
}; 

sub uploadfile {
  use strict;
  use warnings;
  use Net::FTP;

  my ($ftp, $host, $user, $pass, $dir, $fpath);

  $dir = "";
  $host = shift; 
  $user = shift;
  $pass = shift;
  $fpath = shift;

  $ftp = Net::FTP->new($host, Debug => 0);
  $ftp->login($user, $pass) || die $ftp->message;
  $ftp->cwd($dir);
  $ftp->put($fpath) || die $ftp->message;
  $ftp->quit;

  print $ftp->message;
}


sub establishInput {
  $inputName = shift;
  if (!defined($inputName)) {
      $inputName = $FIFO_NAME;
  };

  if (-p $inputName) {
      # delete a previously existing FIFO
      unlink $inputName;
  };
  
  
  if (!-e $inputName) {
      # need to create a FIFO
      system('mknod', $inputName, 'p') && die "$0: could not create FIFO $inputName\n";
      $IS_FIFO = 1;
  } else {
      # assume input comes from a file
      die "$0: file $inputName is not readable" if (!-r $inputName);
      $IS_FIFO = 0;
  }
  $inputName; # return value
}

sub deleteFifo {
  if ($IS_FIFO) {
      $inputName = shift;
      unlink $inputName or die "$0: could not unlink $inputName\n";
  };
};


sub processCallInfo {

  $offsetLine = <CALL_DATA>;
  if ($offsetLine =~ m/^$REFERENCE_OFFSET\s+($SYMBOL_NAME)\s+($HEX_NUMBER)$/) {
	   # This is a dynamic library; need to calculate the load offset
	   $offsetSymbol  = $1;
	   $offsetAddress = hex $2;

	   %offsetTable = reverse %SYMBOLTABLE;
	   $baseAddress = $offsetTable{$offsetSymbol} - $offsetAddress;
	   $offsetLine = <CALL_DATA>;
  }
  else {
	   # This is static
	   $baseAddress = 0;
  }
    
  while (!($offsetLine =~/^$END_TRACE/)) {
     if ($offsetLine =~ m/^($FUNCTION_ENTRY|$FUNCTION_EXIT)\s+($HEX_NUMBER)$/) {
        $action = $1;
        $offset = hex $2;
        $address = $offset + $baseaddress;
        if ($1=~m/$FUNCTION_ENTRY/) {
	    $thisSymbol = $SYMBOLTABLE{$offset+$baseAddress};
	    if (! defined($thisSymbol)) {
	        $thisSymbol = $UNDEFINED_SYMBOL;
	    };

            $thisSymbol =~  m/^(.*)\s+(.*)/g;
            if (! exists $CALL_TABLE{$2}) {
               $CALL_TABLE{$2} = $1;
               print "[$2] => $1\n";
            }
            $level++;
        }
        else {
	    $level--;
        }
      }
      else {
        if ( (length $offsetLine) > 0 ) {
	    chomp $offsetLine;
            print "Unrecognized line format: $offsetLine\n";
        };
      }
      $offsetLine = <CALL_DATA>;	
  };
}

use Getopt::Long qw(GetOptions);

# Main function
use DBI;
use File::Basename;

GetOptions(
  'exec=s' => \$exec,
  'pdl=s' => \$emul,
  'file=s' => \$testcase,
  'ftp=s' => \$fhost,
  'user=s' => \$fuser,
  'pass=s' => \$fpass,
) or die "Usage: etrace.pl --exec ./pdls --file /bonus/scratch/tanch/tcases/PRNFile.JPG --ftp 10.194.15.187 --user chritan --pass **** | tee etrace.log\n";

$sniffer='/users/chritan/bin/pdls_sniff';

#$dbh = DBI->connect('DBI:mysql:tutorial_db;host=10.194.15.187', 'tanch', 'tanch') || die "Could not connect to database: $DBI::errstr";

my $filename = fileparse($testcase);
#$dbh->do('REPLACE INTO files(fname,floc, create_date) VALUES(?, ?, NOW())', undef, $filename, $testcase);

if (index($exec, "runpage4") != -1) {
    $runpage4 = true;
}

if ($exec) {
    if (-x $exec ) {
       if ( $runpage4 eq true ) {
         print "invoking = " . $exec . ' < ' . $testcase . "\n";
         system($exec . ' -s < ' . $testcase);
       }
       else {
         if (length $emul > 1) {
            $type=$emul;
         }
         else {
            open f, $sniffer.' '.$testcase.'|' or die "$0: cannot open $sniffer";
            $type = <f>;
            close f;
         }
         if ( $type eq 'XPS' or ($testcase =~ /\.xps$/i) ) {
             print "invoking = " . $exec . ' -O checksum ' . $testcase . "\n";
             system($exec.' -O checksum '.$testcase);
         }
         else {
             print "invoking = " . $exec . ' -e '. $type . ' ' . $testcase . "\n";
             system($exec.' -e '.$type.' -s '.$testcase);
         }
       }
    }

    readObjects $exec;
    # writeSymbolTable

    # $inputFile = establishInput $inputFile;
    $inputFile = $FIFO_NAME;

    open CALL_DATA,"<$inputFile";
    processCallInfo;
    close CALL_DATA;

    deleteFifo $inputFile;
    if (length $fpass > 0) {
        uploadfile($fhost, $fuser, $fpass, "etrace.log");
    }
}

print "Done: Exiting...\n";
