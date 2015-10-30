#!/usr/bin/perl
#
# etrace.pl script
# Author: Victor Chudnovsky
# Date: March 8, 2004

$NM  = 'nm ';     # unix nm utility to get symbol names and addresses
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

# Global identifiers
# %SYMBOLTABLE : a hash array from relative offsets to symbol names
# $IS_FIFO :     are we reading from a FIFO or a file?
# CALL_DATA :    file handle to input file/FIFO

sub readObjects {
  $objectFileName = shift;
  if (-x $objectFileName) {
    # Object code: extract symbol names via a pipe before parsing
    $handleName = $NM.$objectFileName.'|';
  } else {
    # A symbol table: simply parse the symbol names
    $handleName = '<'.$objectFileName;
  };
  
  open f, $handleName or die "$0: cannot open $objectFileName";
  while ($line = <f>) {
    #print "line: " . $line ;
    $line =~  m/^($HEX_NUMBER)\s+.*\s+($SYMBOL_NAME)$/;
    $hexLocation = $1;
    $symbolName = $2;
    $location = hex $hexLocation;
    $SYMBOLTABLE{$location} = $2
  }
  close f;
}

sub writeSymbolTable {
  while ( ($location,$name) = each %SYMBOLTABLE) {
    print "[$location] => $name\n";
  }
}; 

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
	    print $TAB1 x $level . $TAB2 . $thisSymbol . "\n";
	    $dbh->do('REPLACE INTO tags(tname,create_date) VALUES(?, NOW())', undef, $thisSymbol);
            $dbh->do('REPLACE INTO file_tags(tname,floc, create_date) VALUES(?, ?, NOW())', undef, $thisSymbol, $testcase);
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

#use rlib '/m/mls/pkg/ix86-Linux-RHEL5/lib/mysql';

# Main function
if (@ARGV==0) {
    die "Usage: $0 OBJECT [TRACE]\n" .
	"       OBJECT is either the object code or the output of $NM\n" .
	"       TRACE is either the etrace output file or of a FIFO to connect to etrace.\n";
};

use DBI;
$dbh = DBI->connect('DBI:mysql:tutorial_db;host=10.194.15.187', 'tanch', 'tanch'
          ) || die "Could not connect to database: $DBI::errstr";

#$dbh = DBI->connect('DBI:JDBC:hostname=10.194.15.187:1521;url=jdbc:oracle:thin:\@10.194.15.187:1521:xe;jdbc_character_set=ASCII'
#          ) || die "Could not connect to database: $DBI::errstr";

$objectFile = shift @ARGV;
$testcase = shift @ARGV;

if (-x $objectFile ) {
   print "invoking = " . $objectFile . ' ' . $testcase . "\n";
   system($objectFile . ' ' . $testcase);
}

$dbh->do('REPLACE INTO files(fname,floc, create_date) VALUES(?, ?, NOW())', undef, $testcase, $testcase);

readObjects $objectFile;
#writeSymbolTable

# $inputFile = establishInput $inputFile;
$inputFile = $FIFO_NAME;

open CALL_DATA,"<$inputFile";
processCallInfo;
close CALL_DATA;

deleteFifo $inputFile;
$dbh->disconnect();
print "Done: Exiting...\n";
