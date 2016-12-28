CREATE OR REPLACE DIRECTORY test_dir AS 'D:\\TEST_DIR';
GRANT READ, WRITE ON DIRECTORY test_dir TO tcprofiler;


DROP TABLESPACE tbs_temp_01;
CREATE TEMPORARY TABLESPACE tbs_temp_01
  TEMPFILE 'D:\\db\\tbs_temp_01.dbf' 
    SIZE 6G
    AUTOEXTEND ON;

CREATE TABLESPACE TBS_PERM_01
  DATAFILE 'D:\\db\\tbs_perm_01.dbf'
    SIZE 3G
    REUSE
    AUTOEXTEND ON NEXT 3G MAXSIZE 15G;

CREATE USER tcprofiler
  IDENTIFIED BY tcprofiler
  DEFAULT   TABLESPACE tbs_perm_01
  TEMPORARY TABLESPACE tbs_temp_01;

GRANT ALL PRIVILEGES to tcprofiler IDENTIFIED BY tcprofiler;
GRANT UNLIMITED TABLESPACE TO tcprofiler;
ALTER USER tcprofiler quota UNLIMITED ON TBS_PERM_01;



expdp tcprofiler/tcprofiler@127.0.0.1 schemas=tcprofiler directory=TEST_DIR dumpfile=turtle_exp112416.dmp logfile=turtle_exp112416.log
impdp tcprofiler/tcprofiler@127.0.0.1 schemas=tcprofiler directory=TEST_DIR dumpfile=turtle_exp112616.dmp logfile=turtle_impp112616.log


expdp tcprofiler/tcprofiler@127.0.0.1 schemas=tcprofiler directory=TEST_DIR dumpfile=chritan-win7_exp112616.dmp logfile=chritan-win7_exp112616.log

DROP USER tcprofiler CASCADE; 



GRANT ALL PRIVILEGES to tcprofiler IDENTIFIED BY tcprofiler;
GRANT UNLIMITED TABLESPACE TO tcprofiler;
ALTER USER tcprofiler quota UNLIMITED ON TBS_PERM_01;

select * from dba_data_files;

DROP TABLESPACE tbs_undo_01 INCLUDING CONTENTS AND DATAFILES; 

CREATE BIGFILE TABLESPACE TBS_PERM_01
  DATAFILE 'TBS_PERM_01.dbf'
    SIZE 10G
    REUSE
    AUTOEXTEND ON NEXT 10G MAXSIZE 50G;

CREATE TEMPORARY TABLESPACE tbs_temp_01 TEMPFILE 'D:\\db\\tbs_temp_01.dbf' 
     SIZE 1G REUSE
     EXTENT MANAGEMENT LOCAL UNIFORM SIZE 16M;
     
DROP TABLESPACE tbs_undo_01 INCLUDING CONTENTS AND DATAFILES; 
CREATE BIGFILE UNDO TABLESPACE tbs_undo_01
  DATAFILE 'tbs_undo_01.dbf'
    SIZE 30G 
     AUTOEXTEND ON MAXSIZE 40G;

ALTER BIGFILE TABLESPACE tbs_undo_01 RETENTION NOGUARANTEE;
ALTER DATABASE DATAFILE 'tbs_undo_01.dbf' AUTOEXTEND ON MAXSIZE 20G;

CREATE USER tcprofiler
  IDENTIFIED BY tcprofiler
  DEFAULT   TABLESPACE TBS_PERM_01
  TEMPORARY TABLESPACE tbs_temp_01;
  --UNDO TABLESPACE tbs_undo_01;
  
GRANT ALL PRIVILEGES to tcprofiler IDENTIFIED BY tcprofiler;
GRANT UNLIMITED TABLESPACE TO tcprofiler;
ALTER USER tcprofiler quota UNLIMITED ON TBS_PERM_01;


ALTER TABLE tcprofiler.TESTCASE_FUNC DROP COLUMN RID;


create bigfile temporary tablespace TEMP1 tempfile 'TEMP1.dbf' 
     size 10G autoextend on next 1G maxsize 20G;
create bigfile undo tablespace UNDO1 datafile 'UNDO1.dbf' 
     size 10G autoextend on next 1G maxsize 20G;
alter system set undo_tablespace=UNDO1 scope=both sid='*';
alter database default temporary tablespace TEMP1;
drop tablespace UNDOTBS1;
drop tablespace TEMP;