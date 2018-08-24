--------------------------------------------------------
--  File created - Friday-August-17-2018   
--------------------------------------------------------
--------------------------------------------------------
--  DDL for Trigger CHECKSUM_HISTORY_TRIGGER
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "TANCH"."CHECKSUM_HISTORY_TRIGGER" 
AFTER UPDATE OR DELETE OR INSERT ON TESTCASE_CHECKSUMS 
FOR EACH ROW
BEGIN
    IF inserting THEN
        MERGE INTO CHECKSUM_HISTORY using DUAL on (TGUID=:old.TGUID AND PID=:old.PID)
        WHEN NOT matched THEN
            INSERT (TGUID,PID,MODIFIED_BY,ACTION,UPDATE_DATE,CHECKSUMS) VALUES (:old.TGUID, :old.PID, :old.MODIFIED_BY, 'insert', SYSDATE, :old.CHECKSUMS)
        WHEN matched THEN 
            UPDATE SET CHECKSUMS=:old.CHECKSUMS,MODIFIED_BY=:old.MODIFIED_BY,ACTION='insert',UPDATE_DATE=SYSDATE;
    ELSIF updating THEN
        MERGE INTO CHECKSUM_HISTORY using DUAL on (TGUID=:old.TGUID AND PID=:old.PID)
        WHEN NOT matched THEN
            INSERT (TGUID,PID,MODIFIED_BY,ACTION,UPDATE_DATE,CHECKSUMS) VALUES (:old.TGUID, :old.PID, :old.MODIFIED_BY, 'update', SYSDATE, :old.CHECKSUMS)
        WHEN matched THEN 
            UPDATE SET CHECKSUMS=:old.CHECKSUMS,MODIFIED_BY=:old.MODIFIED_BY,ACTION='update',UPDATE_DATE=SYSDATE;
    ELSIF deleting THEN
        MERGE INTO CHECKSUM_HISTORY using DUAL on (TGUID=:old.TGUID AND PID=:old.PID)
        WHEN NOT matched THEN
            INSERT (TGUID,PID,MODIFIED_BY,ACTION,UPDATE_DATE,CHECKSUMS) VALUES (:old.TGUID, :old.PID, :old.MODIFIED_BY, 'delete', SYSDATE, :old.CHECKSUMS)
        WHEN matched THEN 
            UPDATE SET CHECKSUMS=:old.CHECKSUMS,MODIFIED_BY=:old.MODIFIED_BY,ACTION='delete',UPDATE_DATE=SYSDATE;
    END IF;
END; 


/
ALTER TRIGGER "TANCH"."CHECKSUM_HISTORY_TRIGGER" ENABLE;
