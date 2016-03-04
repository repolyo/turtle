--------------------------------------------------------
--  File created - Thursday-March-03-2016   
--------------------------------------------------------
DROP SEQUENCE "DBOBJECTID_SEQUENCE";
DROP SEQUENCE "FUNC_SEQ";
DROP SEQUENCE "PLATFORM_SEQ";
DROP SEQUENCE "TAG_SEQ";
DROP SEQUENCE "TC_RUN_SEQ";
DROP SEQUENCE "TESTCASE_SEQ";
--------------------------------------------------------
--  DDL for Sequence DBOBJECTID_SEQUENCE
--------------------------------------------------------

   CREATE SEQUENCE  "DBOBJECTID_SEQUENCE"  MINVALUE 1 MAXVALUE 999999999999999999999999 INCREMENT BY 50 START WITH 1 CACHE 50 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence FUNC_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "FUNC_SEQ"  MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 8021 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence PLATFORM_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "PLATFORM_SEQ"  MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 1301 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence TAG_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "TAG_SEQ"  MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence TC_RUN_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "TC_RUN_SEQ"  MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence TESTCASE_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "TESTCASE_SEQ"  MINVALUE 1 MAXVALUE 9999999999999999999999999999 INCREMENT BY 1 START WITH 12821 CACHE 20 NOORDER  NOCYCLE ;
