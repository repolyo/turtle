--------------------------------------------------------
--  File created - Friday-August-17-2018   
--------------------------------------------------------
--------------------------------------------------------
--  DDL for View TESTCASE_CHECKSUMS_VIEW
--------------------------------------------------------

  CREATE OR REPLACE FORCE VIEW "TANCH"."TESTCASE_CHECKSUMS_VIEW" ("TGUID", "TLOC", "PID", "MODIFIED_BY", "CHECKSUMS") AS 
  SELECT 
    t.TGUID,
    t.TLOC,
    c.PID,
    c.MODIFIED_BY,
    c.CHECKSUMS
FROM 
    TESTCASE t JOIN TESTCASE_CHECKSUMS c ON t.TGUID = c.TGUID
;
