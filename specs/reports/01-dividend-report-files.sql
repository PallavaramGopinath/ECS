-- Point the dividend reports at the ECS-adapted .rdlc files (commit 74a1768).
-- reports_master still named the TSIS originals, whose field names (memberNo, memberrName)
-- do not match the ECS row DTOs (MemberNo, MemberName), so these reports failed to bind.
-- Apply to every ECS database. Idempotent.

UPDATE reports_master SET reportfilename = 'DividendWorkingSheet.rdlc'
 WHERE report_id = 56  AND reportfilename = 'DividendTDFWDWorkSheet.rdlc';

UPDATE reports_master SET reportfilename = 'DividendPendingYearWise.rdlc'
 WHERE report_id = 97  AND reportfilename = 'DividendIntOnTDPending.rdlc';

UPDATE reports_master SET reportfilename = 'DividendPendingMemWise.rdlc'
 WHERE report_id = 128 AND reportfilename = 'DividendIntOnTDPendingMemWiseYearWise.rdlc';
