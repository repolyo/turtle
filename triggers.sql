--------------------------------------------------------
--  File created - Thursday-March-03-2016   
--------------------------------------------------------
DROP TRIGGER "FUNC_TRIGGER";
DROP TRIGGER "PLATFORM_SEQ";
DROP TRIGGER "TAG_SEQ";
DROP TRIGGER "TC_RUN_SEQ";
DROP TRIGGER "TESTCASE_TRIGGER";
--------------------------------------------------------
--  DDL for Trigger FUNC_TRIGGER
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "FUNC_TRIGGER" BEFORE INSERT ON tcprofiler.func
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

  CREATE OR REPLACE TRIGGER "PLATFORM_SEQ" BEFORE INSERT ON tcprofiler.platform
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

  CREATE OR REPLACE TRIGGER "TAG_SEQ" BEFORE INSERT ON tcprofiler.TAGS
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
--  DDL for Trigger TC_RUN_SEQ
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "TC_RUN_SEQ" BEFORE INSERT ON tcprofiler.TESTCASE_RUN
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.RID IS NULL THEN
    SELECT platform_seq.nextval INTO :NEW.RID FROM dual;
  END IF;
END;
/
ALTER TRIGGER "TC_RUN_SEQ" ENABLE;
--------------------------------------------------------
--  DDL for Trigger TESTCASE_TRIGGER
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "TESTCASE_TRIGGER" BEFORE INSERT ON tcprofiler.testcase
REFERENCING NEW AS NEW
FOR EACH ROW
BEGIN
  IF :NEW.tid IS NULL THEN
    SELECT testcase_seq.nextval INTO :NEW.tid FROM dual;
  END IF;
END;
/
ALTER TRIGGER "TESTCASE_TRIGGER" ENABLE;
