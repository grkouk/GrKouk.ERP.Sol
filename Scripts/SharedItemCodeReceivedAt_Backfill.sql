/*
  SharedItemCodeReceivedAt_Backfill.sql : data script for migration AddSharedItemCodeReceivedAt (run AFTER it).

  The migration adds ReceivedAt (datetime2, NOT NULL) and ModifiedByShopId (nvarchar(50), NULL) to
  SharedItemCodes; existing rows get ReceivedAt = 0001-01-01. This sets ReceivedAt to the code's
  LastUsedAt (the old pull watermark), or its CreatedAt when it was never scanned, so every code looks
  exactly as old as it does today and nothing is re-sent to the shops. Codes with neither date stay at
  0001-01-01, which is correct: they travel with their item only. ModifiedByShopId stays NULL (unknown).
  Codes the hub stamped after the new ERP build went live (ReceivedAt > 2000-01-01) are left alone.

  Idempotent. DRY RUN by default: set @Commit = 1 to keep the changes.
  Run against the hub DB (GrKoukDB).
*/
SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Commit bit = 0;
DECLARE @Unset datetime2 = '2000-01-01';

IF COL_LENGTH('dbo.SharedItemCodes', 'ReceivedAt') IS NULL
BEGIN
    RAISERROR('ReceivedAt is missing: apply migration AddSharedItemCodeReceivedAt first.', 16, 1);
    RETURN;
END

DECLARE @total int, @unsetBefore int, @updated int, @unsetAfter int, @noDates int;

BEGIN TRANSACTION;

SELECT @total = COUNT(*),
       @unsetBefore = ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0),
       @noDates = ISNULL(SUM(CASE WHEN LastUsedAt IS NULL AND CreatedAt IS NULL THEN 1 ELSE 0 END), 0)
FROM dbo.SharedItemCodes;

UPDATE dbo.SharedItemCodes
SET ReceivedAt = COALESCE(LastUsedAt, CreatedAt)
WHERE ReceivedAt <= @Unset AND COALESCE(LastUsedAt, CreatedAt) IS NOT NULL;
SET @updated = @@ROWCOUNT;

SELECT @unsetAfter = COUNT(*) FROM dbo.SharedItemCodes WHERE ReceivedAt <= @Unset;

-- RowsUnsetAfter should equal RowsWithNoDates (codes never scanned and with no creation date).
SELECT CASE WHEN @Commit = 1 THEN 'COMMITTED' ELSE 'DRY RUN (rolled back)' END AS Mode,
       @total AS RowsTotal, @unsetBefore AS RowsUnsetBefore, @updated AS RowsUpdated,
       @unsetAfter AS RowsUnsetAfter, @noDates AS RowsWithNoDates;

IF @Commit = 1 COMMIT TRANSACTION; ELSE ROLLBACK TRANSACTION;
