--------------------------------------------------------
--  File created - Friday-August-17-2018   
--------------------------------------------------------
--------------------------------------------------------
--  DDL for Table CHECKSUM_HISTORY
--------------------------------------------------------

  CREATE TABLE "TANCH"."CHECKSUM_HISTORY" 
   (	"TGUID" VARCHAR2(32 BYTE), 
	"PID" NUMBER, 
	"MODIFIED_BY" VARCHAR2(20 BYTE), 
	"ACTION" VARCHAR2(20 BYTE), 
	"UPDATE_DATE" DATE, 
	"CHECKSUMS" CLOB
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "TBS_PERM_01" 
 LOB ("CHECKSUMS") STORE AS BASICFILE (
  TABLESPACE "TBS_PERM_01" ENABLE STORAGE IN ROW CHUNK 8192 RETENTION 
  NOCACHE LOGGING 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)) ;

   COMMENT ON COLUMN "TANCH"."CHECKSUM_HISTORY"."CHECKSUMS" IS 'comma separated checksums';
