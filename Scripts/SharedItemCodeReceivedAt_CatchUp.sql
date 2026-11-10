/*
  SharedItemCodeReceivedAt_CatchUp.sql : re-send the scan stamps the shops missed before the fix.

  Sets ReceivedAt = now (and ModifiedByShopId = NULL, so no shop is skipped) on every hub item code
  scanned on or after @ScannedSince. Each shop's next sync then pulls those codes and their items.
  Safe to send widely: a shop only ever moves its own "last scanned" time forward (MAX-merge) and
  applies the item by last-writer-wins, so a row it already holds is left as it is.

  It does NOT change a code's fields on the hub, and it cannot send a barcode the hub does not have
  (one added in a shop's quick details dialog before the till fix: re-save that item in the shop).

  @ScannedSince = NULL sends every code that was ever scanned. Re-running only moves the time forward
  again. DRY RUN by default: set @Commit = 1. Run against the hub DB (GrKoukDB), after the backfill.
*/
SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Commit bit = 0;
DECLARE @ScannedSince datetime2 = NULL;

IF COL_LENGTH('dbo.SharedItemCodes', 'ReceivedAt') IS NULL
BEGIN
    RAISERROR('ReceivedAt is missing: apply migration AddSharedItemCodeReceivedAt and the backfill first.', 16, 1);
    RETURN;
END

DECLARE @Now datetime2 = SYSUTCDATETIME();
DECLARE @codes int, @items int;

BEGIN TRANSACTION;

SELECT @items = COUNT(DISTINCT ItemId)
FROM dbo.SharedItemCodes
WHERE LastUsedAt IS NOT NULL AND (@ScannedSince IS NULL OR LastUsedAt >= @ScannedSince);

UPDATE dbo.SharedItemCodes
SET ReceivedAt = @Now, ModifiedByShopId = NULL
WHERE LastUsedAt IS NOT NULL AND (@ScannedSince IS NULL OR LastUsedAt >= @ScannedSince);
SET @codes = @@ROWCOUNT;

SELECT CASE WHEN @Commit = 1 THEN 'COMMITTED' ELSE 'DRY RUN (rolled back)' END AS Mode,
       @Now AS ReceivedAt, @ScannedSince AS ScannedSince, @codes AS CodesReSent, @items AS ItemsPulledWithThem;

IF @Commit = 1 COMMIT TRANSACTION; ELSE ROLLBACK TRANSACTION;
