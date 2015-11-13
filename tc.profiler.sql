--------------------------------------------------------
--  File created - Friday-November-13-2015   
--------------------------------------------------------
DROP TABLE "TC_PROFILER"."FUNC" cascade constraints;
DROP TABLE "TC_PROFILER"."TAGS" cascade constraints;
DROP TABLE "TC_PROFILER"."TESTCASE" cascade constraints;
DROP TABLE "TC_PROFILER"."TESTCASE_FUNC_MAP" cascade constraints;
--------------------------------------------------------
--  DDL for Table FUNC
--------------------------------------------------------

  CREATE TABLE "TC_PROFILER"."FUNC" 
   (	"FID" NUMBER(*,0), 
	"SOURCE_FILE" VARCHAR2(255 BYTE), 
	"LINE_NO" NUMBER(*,0), 
	"FUNC_NAME" VARCHAR2(255 BYTE), 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;
--------------------------------------------------------
--  DDL for Table TAGS
--------------------------------------------------------

  CREATE TABLE "TC_PROFILER"."TAGS" 
   (	"TAG_NAME" VARCHAR2(255 BYTE), 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;
--------------------------------------------------------
--  DDL for Table TESTCASE
--------------------------------------------------------

  CREATE TABLE "TC_PROFILER"."TESTCASE" 
   (	"TNAME" VARCHAR2(255 BYTE), 
	"TLOC" VARCHAR2(255 BYTE), 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;
--------------------------------------------------------
--  DDL for Table TESTCASE_FUNC_MAP
--------------------------------------------------------

  CREATE TABLE "TC_PROFILER"."TESTCASE_FUNC_MAP" 
   (	"SEQ" NUMBER,
   "TLOC" VARCHAR2(255 BYTE), 
	"FID" NUMBER(*,0), 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;

   COMMENT ON COLUMN "TC_PROFILER"."TESTCASE_FUNC_MAP"."SEQ" IS 'order of invocation';
--------------------------------------------------------
--  DDL for Index SYS_C007064
--------------------------------------------------------

  CREATE UNIQUE INDEX "TC_PROFILER"."SYS_C007064" ON "TC_PROFILER"."FUNC" ("FID") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C007060
--------------------------------------------------------

  CREATE UNIQUE INDEX "TC_PROFILER"."SYS_C007060" ON "TC_PROFILER"."TAGS" ("TAG_NAME") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C007058
--------------------------------------------------------

  CREATE UNIQUE INDEX "TC_PROFILER"."SYS_C007058" ON "TC_PROFILER"."TESTCASE" ("TLOC") 
  ;
--------------------------------------------------------
--  Constraints for Table FUNC
--------------------------------------------------------

  ALTER TABLE "TC_PROFILER"."FUNC" ADD PRIMARY KEY ("FID") ENABLE;
  ALTER TABLE "TC_PROFILER"."FUNC" MODIFY ("FUNC_NAME" NOT NULL ENABLE);
  ALTER TABLE "TC_PROFILER"."FUNC" MODIFY ("SOURCE_FILE" NOT NULL ENABLE);
  ALTER TABLE "TC_PROFILER"."FUNC" MODIFY ("FID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TAGS
--------------------------------------------------------

  ALTER TABLE "TC_PROFILER"."TAGS" ADD PRIMARY KEY ("TAG_NAME") ENABLE;
  ALTER TABLE "TC_PROFILER"."TAGS" MODIFY ("TAG_NAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TESTCASE
--------------------------------------------------------

  ALTER TABLE "TC_PROFILER"."TESTCASE" ADD PRIMARY KEY ("TLOC") ENABLE;
  ALTER TABLE "TC_PROFILER"."TESTCASE" MODIFY ("TLOC" NOT NULL ENABLE);
  ALTER TABLE "TC_PROFILER"."TESTCASE" MODIFY ("TNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TESTCASE_FUNC_MAP
--------------------------------------------------------

  ALTER TABLE "TC_PROFILER"."TESTCASE_FUNC_MAP" MODIFY ("FID" NOT NULL ENABLE);
  ALTER TABLE "TC_PROFILER"."TESTCASE_FUNC_MAP" MODIFY ("TLOC" NOT NULL ENABLE);
--------------------------------------------------------
--  Ref Constraints for Table TESTCASE_FUNC_MAP
--------------------------------------------------------

  ALTER TABLE "TC_PROFILER"."TESTCASE_FUNC_MAP" ADD FOREIGN KEY ("TLOC")
	  REFERENCES "TC_PROFILER"."TESTCASE" ("TLOC") ENABLE;
  ALTER TABLE "TC_PROFILER"."TESTCASE_FUNC_MAP" ADD FOREIGN KEY ("FID")
	  REFERENCES "TC_PROFILER"."FUNC" ("FID") ENABLE;
--------------------------------------------------------
--  DDL for Trigger FUNC_TRIGGER
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "TC_PROFILER"."FUNC_TRIGGER" BEFORE INSERT ON tc_profiler.func
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.fid IS NULL THEN
    SELECT func_seq.nextval INTO :NEW.fid FROM dual;
  END IF;
END;
/
ALTER TRIGGER "TC_PROFILER"."FUNC_TRIGGER" ENABLE;
