/*
  SharedRowReceivedAt_Backfill.sql : data script for migration AddSharedRowReceivedAt (run AFTER it).

  The migration adds ReceivedAt (datetime2, NOT NULL) to the 9 shared catalog tables; existing rows
  get 0001-01-01. This sets ReceivedAt = ModifiedAt on those rows, so every row looks exactly as old
  as it does today and nothing is re-sent to the shops. Rows the hub stamped after the new ERP build
  went live (ReceivedAt > 2000-01-01) are left alone.

  Idempotent. DRY RUN by default: set @Commit = 1 to keep the changes.
  Run against the hub DB (GrKoukDB).
*/
SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Commit bit = 0;
DECLARE @Unset datetime2 = '2000-01-01';

IF COL_LENGTH('dbo.SharedItems', 'ReceivedAt') IS NULL
BEGIN
    RAISERROR('ReceivedAt is missing: apply migration AddSharedRowReceivedAt first.', 16, 1);
    RETURN;
END

DECLARE @r TABLE (TableName sysname, RowsTotal int, RowsUnsetBefore int, RowsUpdated int, RowsUnsetAfter int);
DECLARE @n int;

BEGIN TRANSACTION;

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedItems', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedItems;
UPDATE dbo.SharedItems SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedItems WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedItems';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedItemPrices', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedItemPrices;
UPDATE dbo.SharedItemPrices SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedItemPrices WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedItemPrices';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedItemErpMappings', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedItemErpMappings;
UPDATE dbo.SharedItemErpMappings SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedItemErpMappings WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedItemErpMappings';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedItemCategories', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedItemCategories;
UPDATE dbo.SharedItemCategories SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedItemCategories WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedItemCategories';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedItemFamilies', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedItemFamilies;
UPDATE dbo.SharedItemFamilies SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedItemFamilies WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedItemFamilies';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedItemBrands', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedItemBrands;
UPDATE dbo.SharedItemBrands SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedItemBrands WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedItemBrands';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedVatClasses', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedVatClasses;
UPDATE dbo.SharedVatClasses SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedVatClasses WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedVatClasses';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedMeasureUnits', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedMeasureUnits;
UPDATE dbo.SharedMeasureUnits SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedMeasureUnits WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedMeasureUnits';

INSERT @r (TableName, RowsTotal, RowsUnsetBefore) SELECT 'SharedCashierDepartments', COUNT(*), ISNULL(SUM(CASE WHEN ReceivedAt <= @Unset THEN 1 ELSE 0 END), 0) FROM dbo.SharedCashierDepartments;
UPDATE dbo.SharedCashierDepartments SET ReceivedAt = ModifiedAt WHERE ReceivedAt <= @Unset; SET @n = @@ROWCOUNT;
UPDATE @r SET RowsUpdated = @n, RowsUnsetAfter = (SELECT COUNT(*) FROM dbo.SharedCashierDepartments WHERE ReceivedAt <= @Unset) WHERE TableName = 'SharedCashierDepartments';

SELECT CASE WHEN @Commit = 1 THEN 'COMMITTED' ELSE 'DRY RUN (rolled back)' END AS Mode, * FROM @r;

-- A row whose ModifiedAt is itself at or before 2000-01-01 stays "unset"; it is reported, not hidden.
IF EXISTS (SELECT 1 FROM @r WHERE RowsUnsetAfter > 0)
    SELECT 'WARNING: rows still at or before 2000-01-01 (their ModifiedAt is that old)' AS Note;

IF @Commit = 1 COMMIT TRANSACTION; ELSE ROLLBACK TRANSACTION;
