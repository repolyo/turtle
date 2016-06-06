-DROP TABLESPACE tbs_temp_01;
-CREATE TEMPORARY TABLESPACE tbs_temp_01
-  TEMPFILE 'tbs_temp_01.dbf'
-    SIZE 6G
-    AUTOEXTEND ON;
-
-CREATE TABLESPACE tbs_perm_01
-  DATAFILE 'tbs_perm_01.dbf'
-    SIZE 3G
-    REUSE
-    AUTOEXTEND ON NEXT 3G MAXSIZE 15G;
-
-CREATE USER tc_profiler
-  IDENTIFIED BY tc_profiler
-  DEFAULT   TABLESPACE tbs_perm_01
-  TEMPORARY TABLESPACE tbs_temp_01;
-
-GRANT ALL PRIVILEGES to tc_profiler IDENTIFIED BY tc_profiler;
-GRANT UNLIMITED TABLESPACE TO tc_profiler;
-ALTER USER tc_profiler quota UNLIMITED ON TBS_PERM_01;