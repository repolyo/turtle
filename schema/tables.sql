--------------------------------------------------------
--  File created - Friday-June-17-2016   
--------------------------------------------------------
--------------------------------------------------------
--  DDL for Table FUNC
--------------------------------------------------------

  CREATE TABLE "FUNC" 
   (	"FID" NUMBER(*,0), 
	"SOURCE_FILE" VARCHAR2(255 BYTE), 
	"LINE_NO" NUMBER(*,0), 
	"FUNC_NAME" VARCHAR2(255 BYTE), 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;
--------------------------------------------------------
--  DDL for Table PLATFORM
--------------------------------------------------------

  CREATE TABLE "PLATFORM" 
   (	"PID" NUMBER(*,0), 
	"CODELINE" VARCHAR2(255 BYTE) DEFAULT NULL, 
	"PERSONA" VARCHAR2(255 BYTE) DEFAULT NULL, 
	"VERSION" VARCHAR2(255 BYTE), 
	"MEMORY" NUMBER(32,0) DEFAULT 134217728, 
	"DISK" NUMBER(32,0) DEFAULT 0, 
	"USB" NUMBER(1,0) DEFAULT 0, 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;

   COMMENT ON COLUMN "PLATFORM"."CODELINE" IS 'origin/stable-incoming';
   COMMENT ON COLUMN "PLATFORM"."PERSONA" IS 'eg: sim-palazzo';
   COMMENT ON COLUMN "PLATFORM"."VERSION" IS 'fw3';
   COMMENT ON COLUMN "PLATFORM"."MEMORY" IS 'Defaults to 128MB';
--------------------------------------------------------
--  DDL for Table TAGS
--------------------------------------------------------

  CREATE TABLE "TAGS" 
   (	"TID" NUMBER(*,0), 
	"TAG_NAME" VARCHAR2(80 BYTE), 
	"TAG_DESCR" VARCHAR2(255 BYTE) DEFAULT '-', 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;
--------------------------------------------------------
--  DDL for Table TAGS_FUNC
--------------------------------------------------------

  CREATE TABLE "TAGS_FUNC" 
   (	"TID" NUMBER(*,0), 
	"FID" NUMBER, 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;
--------------------------------------------------------
--  DDL for Table TESTCASE
--------------------------------------------------------

  CREATE TABLE "TESTCASE" 
   (	"TGUID" VARCHAR2(32 BYTE), 
	"TID" NUMBER, 
	"TNAME" VARCHAR2(255 BYTE), 
	"TLOC" VARCHAR2(255 BYTE), 
	"TTYPE" VARCHAR2(20 BYTE), 
	"TSIZE" NUMBER, 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;

   COMMENT ON COLUMN "TESTCASE"."TGUID" IS 'test case guid';
   COMMENT ON COLUMN "TESTCASE"."TTYPE" IS 'test case type';
   COMMENT ON COLUMN "TESTCASE"."TSIZE" IS 'file size';
--------------------------------------------------------
--  DDL for Table TESTCASE_CHECKSUM
--------------------------------------------------------

  CREATE TABLE "TESTCASE_CHECKSUM" 
   (	"TGUID" VARCHAR2(32 BYTE), 
	"PAGE_NO" NUMBER, 
	"CHECKSUM" VARCHAR2(255 BYTE), 
	"PID" NUMBER DEFAULT 1
   ) ;

   COMMENT ON COLUMN "TESTCASE_CHECKSUM"."PID" IS 'Platform ID';
--------------------------------------------------------
--  DDL for Table TESTCASE_FUNC
--------------------------------------------------------

  CREATE TABLE "TESTCASE_FUNC" 
   (	"TGUID" VARCHAR2(32 BYTE), 
	"TID" NUMBER, 
	"SEQ" NUMBER, 
	"FID" NUMBER(*,0), 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;

   COMMENT ON COLUMN "TESTCASE_FUNC"."TGUID" IS 'testcase id';
   COMMENT ON COLUMN "TESTCASE_FUNC"."SEQ" IS 'order of invocation';
--------------------------------------------------------
--  DDL for Table TESTCASE_RUN
--------------------------------------------------------

  CREATE TABLE "TESTCASE_RUN" 
   (	"RID" NUMBER, 
	"PID" NUMBER(*,0) DEFAULT 1, 
	"TID" NUMBER(*,0), 
	"TGUID" VARCHAR2(32 BYTE), 
	"START_TIME" TIMESTAMP (6), 
	"END_TIME" TIMESTAMP (6)
   ) ;

   COMMENT ON COLUMN "TESTCASE_RUN"."RID" IS 'run id';
   COMMENT ON TABLE "TESTCASE_RUN"  IS 'Monitors the testcase individual run (performance) elapse time.
To be able to show testcase performance history across builds.';
--------------------------------------------------------
--  DDL for Table TESTCASE_TAGS
--------------------------------------------------------

  CREATE TABLE "TESTCASE_TAGS" 
   (	"TGUID" VARCHAR2(32 BYTE), 
	"TID" NUMBER(*,0), 
	"TAG_ID" NUMBER, 
	"CREATE_DATE" TIMESTAMP (6) DEFAULT CURRENT_TIMESTAMP
   ) ;

   COMMENT ON COLUMN "TESTCASE_TAGS"."TGUID" IS 'testcase id';
--------------------------------------------------------
--  DDL for Index SYS_C007064
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYS_C007064" ON "FUNC" ("FID") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C007095
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYS_C007095" ON "PLATFORM" ("PID") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C007060
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYS_C007060" ON "TAGS" ("TAG_NAME") 
  ;
--------------------------------------------------------
--  DDL for Index TAGS_FUNC_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "TAGS_FUNC_PK" ON "TAGS_FUNC" ("FID", "TID") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C007058
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYS_C007058" ON "TESTCASE" ("TLOC") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C007073
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYS_C007073" ON "TESTCASE" ("TID") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C7074
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYS_C7074" ON "TESTCASE" ("TGUID") 
  ;
--------------------------------------------------------
--  DDL for Index TESTCASE_CHECKSUM_PK1
--------------------------------------------------------

  CREATE UNIQUE INDEX "TESTCASE_CHECKSUM_PK1" ON "TESTCASE_CHECKSUM" ("TGUID", "PAGE_NO") 
  ;
--------------------------------------------------------
--  DDL for Index TESTCASE_FUNC_PK1
--------------------------------------------------------

  CREATE UNIQUE INDEX "TESTCASE_FUNC_PK1" ON "TESTCASE_FUNC" ("FID", "TGUID") 
  ;
--------------------------------------------------------
--  DDL for Index SYS_C007100
--------------------------------------------------------

  CREATE UNIQUE INDEX "SYS_C007100" ON "TESTCASE_RUN" ("RID") 
  ;
--------------------------------------------------------
--  DDL for Index TESTCASE_TAGS_PK
--------------------------------------------------------

  CREATE UNIQUE INDEX "TESTCASE_TAGS_PK" ON "TESTCASE_TAGS" ("TAG_ID", "TGUID") 
  ;
--------------------------------------------------------
--  Constraints for Table FUNC
--------------------------------------------------------

  ALTER TABLE "FUNC" MODIFY ("FID" NOT NULL ENABLE);
  ALTER TABLE "FUNC" MODIFY ("SOURCE_FILE" NOT NULL ENABLE);
  ALTER TABLE "FUNC" MODIFY ("FUNC_NAME" NOT NULL ENABLE);
  ALTER TABLE "FUNC" ADD PRIMARY KEY ("FID") ENABLE;
--------------------------------------------------------
--  Constraints for Table PLATFORM
--------------------------------------------------------

  ALTER TABLE "PLATFORM" ADD PRIMARY KEY ("PID") ENABLE;
  ALTER TABLE "PLATFORM" MODIFY ("USB" NOT NULL ENABLE);
  ALTER TABLE "PLATFORM" MODIFY ("DISK" NOT NULL ENABLE);
  ALTER TABLE "PLATFORM" MODIFY ("MEMORY" NOT NULL ENABLE);
  ALTER TABLE "PLATFORM" MODIFY ("VERSION" NOT NULL ENABLE);
  ALTER TABLE "PLATFORM" MODIFY ("PERSONA" NOT NULL ENABLE);
  ALTER TABLE "PLATFORM" MODIFY ("CODELINE" NOT NULL ENABLE);
  ALTER TABLE "PLATFORM" MODIFY ("PID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TAGS
--------------------------------------------------------

  ALTER TABLE "TAGS" MODIFY ("TAG_NAME" NOT NULL ENABLE);
  ALTER TABLE "TAGS" ADD PRIMARY KEY ("TAG_NAME") ENABLE;
--------------------------------------------------------
--  Constraints for Table TAGS_FUNC
--------------------------------------------------------

  ALTER TABLE "TAGS_FUNC" MODIFY ("TID" NOT NULL ENABLE);
  ALTER TABLE "TAGS_FUNC" MODIFY ("FID" NOT NULL ENABLE);
  ALTER TABLE "TAGS_FUNC" ADD CONSTRAINT "TAGS_FUNC_PK" PRIMARY KEY ("FID", "TID") ENABLE;
--------------------------------------------------------
--  Constraints for Table TESTCASE
--------------------------------------------------------

  ALTER TABLE "TESTCASE" ADD CONSTRAINT "SYS_C007073" PRIMARY KEY ("TGUID") ENABLE;
  ALTER TABLE "TESTCASE" MODIFY ("TNAME" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE" MODIFY ("TLOC" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE" MODIFY ("TGUID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TESTCASE_CHECKSUM
--------------------------------------------------------

  ALTER TABLE "TESTCASE_CHECKSUM" MODIFY ("CHECKSUM" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_CHECKSUM" ADD CONSTRAINT "TESTCASE_CHECKSUM_PK" PRIMARY KEY ("TGUID", "PAGE_NO") ENABLE;
  ALTER TABLE "TESTCASE_CHECKSUM" MODIFY ("TGUID" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_CHECKSUM" MODIFY ("PAGE_NO" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TESTCASE_FUNC
--------------------------------------------------------

  ALTER TABLE "TESTCASE_FUNC" MODIFY ("TGUID" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_FUNC" ADD CONSTRAINT "TESTCASE_FUNC_PK" PRIMARY KEY ("FID", "TGUID") ENABLE;
  ALTER TABLE "TESTCASE_FUNC" MODIFY ("FID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TESTCASE_RUN
--------------------------------------------------------

  ALTER TABLE "TESTCASE_RUN" MODIFY ("RID" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_RUN" ADD CONSTRAINT "SYS_C007100" PRIMARY KEY ("RID") ENABLE;
  ALTER TABLE "TESTCASE_RUN" MODIFY ("END_TIME" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_RUN" MODIFY ("START_TIME" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_RUN" MODIFY ("TGUID" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_RUN" MODIFY ("PID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TESTCASE_TAGS
--------------------------------------------------------

  ALTER TABLE "TESTCASE_TAGS" ADD CONSTRAINT "TESTCASE_TAGS_PK" PRIMARY KEY ("TAG_ID", "TGUID") ENABLE;
  ALTER TABLE "TESTCASE_TAGS" MODIFY ("TAG_ID" NOT NULL ENABLE);
  ALTER TABLE "TESTCASE_TAGS" MODIFY ("TGUID" NOT NULL ENABLE);
--------------------------------------------------------
--  Ref Constraints for Table TESTCASE_CHECKSUM
--------------------------------------------------------

  ALTER TABLE "TESTCASE_CHECKSUM" ADD CONSTRAINT "TESTCASE_CHECKSUM_FK1" FOREIGN KEY ("TGUID")
	  REFERENCES "TESTCASE" ("TGUID") ON DELETE CASCADE ENABLE;
--------------------------------------------------------
--  Ref Constraints for Table TESTCASE_FUNC
--------------------------------------------------------

  ALTER TABLE "TESTCASE_FUNC" ADD CONSTRAINT "TESTCASE_FUNC_FK1" FOREIGN KEY ("TGUID")
	  REFERENCES "TESTCASE" ("TGUID") ON DELETE CASCADE ENABLE;
--------------------------------------------------------
--  Ref Constraints for Table TESTCASE_RUN
--------------------------------------------------------

  ALTER TABLE "TESTCASE_RUN" ADD CONSTRAINT "TESTCASE_RUN_FK1" FOREIGN KEY ("TGUID")
	  REFERENCES "TESTCASE" ("TGUID") ON DELETE CASCADE ENABLE;
--------------------------------------------------------
--  Ref Constraints for Table TESTCASE_TAGS
--------------------------------------------------------

  ALTER TABLE "TESTCASE_TAGS" ADD CONSTRAINT "TESTCASE_TAGS_FK1" FOREIGN KEY ("TGUID")
	  REFERENCES "TESTCASE" ("TGUID") ON DELETE CASCADE ENABLE;
--------------------------------------------------------
--  DDL for Trigger FUNC_TRIGGER
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "FUNC_TRIGGER" BEFORE INSERT ON func
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.fid IS NULL THEN
    SELECT func_seq.nextval INTO :NEW.fid FROM dual;
  END IF;
END;
/
ALTER TRIGGER "FUNC_TRIGGER" ENABLE;
--------------------------------------------------------
--  DDL for Trigger PLATFORM_SEQ
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "PLATFORM_SEQ" BEFORE INSERT ON platform
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.pid IS NULL THEN
    SELECT platform_seq.nextval INTO :NEW.pid FROM dual;
  END IF;
END;
/
ALTER TRIGGER "PLATFORM_SEQ" ENABLE;
--------------------------------------------------------
--  DDL for Trigger TAG_SEQ
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "TAG_SEQ" BEFORE INSERT ON TAGS
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.TID IS NULL THEN
    SELECT platform_seq.nextval INTO :NEW.TID FROM dual;
  END IF;
END;
/
ALTER TRIGGER "TAG_SEQ" ENABLE;
--------------------------------------------------------
--  DDL for Trigger TESTCASE_TRIGGER
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "TESTCASE_TRIGGER" BEFORE INSERT ON testcase
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.tid IS NULL THEN
    SELECT testcase_seq.nextval INTO :NEW.tid FROM dual;
  END IF;
END;
/
ALTER TRIGGER "TESTCASE_TRIGGER" ENABLE;
--------------------------------------------------------
--  DDL for Trigger TC_RUN_SEQ
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "TC_RUN_SEQ" BEFORE INSERT ON TESTCASE_RUN
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.RID IS NULL THEN
    SELECT platform_seq.nextval INTO :NEW.RID FROM dual;
  END IF;
END;
/
ALTER TRIGGER "TC_RUN_SEQ" ENABLE;
