/*
  SharedRowReceivedAt_CatchUp.sql : re-send approved catalog rows a shop missed before the fix.

  Sets ReceivedAt = now on exactly the rows listed below, so each shop's next sync pulls them
  (the pull asks for ReceivedAt after its last sync). ModifiedAt is NOT touched: the receiving shop
  still applies by last-writer-wins, so a row it holds newer is skipped, as always.

  Pulled INTERSHOP prices are re-derived into the shop's BASIC price at once, so the list must be
  reviewed with the owner first (plan section 3).

  Fill @Items / @Prices / @Mappings with the approved Ids (from the step 0 measurement).
  Re-running only moves the time forward again. DRY RUN by default: set @Commit = 1.
  Run against the hub DB (GrKoukDB), after the backfill.
*/
SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Commit bit = 0;

DECLARE @Items    TABLE (Id uniqueidentifier PRIMARY KEY);
DECLARE @Prices   TABLE (Id uniqueidentifier PRIMARY KEY);
DECLARE @Mappings TABLE (Id uniqueidentifier PRIMARY KEY);

-- INSERT @Items    (Id) VALUES ('00000000-0000-0000-0000-000000000000');
-- INSERT @Prices   (Id) VALUES ('00000000-0000-0000-0000-000000000000');
-- INSERT @Mappings (Id) VALUES ('00000000-0000-0000-0000-000000000000');

IF COL_LENGTH('dbo.SharedItems', 'ReceivedAt') IS NULL
BEGIN
    RAISERROR('ReceivedAt is missing: apply migration AddSharedRowReceivedAt and the backfill first.', 16, 1);
    RETURN;
END

DECLARE @Now datetime2 = SYSUTCDATETIME();

-- Listed Ids the hub does not have: reported, never silently skipped.
SELECT 'missing on hub' AS Problem, 'SharedItems' AS TableName, l.Id FROM @Items l WHERE NOT EXISTS (SELECT 1 FROM dbo.SharedItems h WHERE h.Id = l.Id)
UNION ALL SELECT 'missing on hub', 'SharedItemPrices', l.Id FROM @Prices l WHERE NOT EXISTS (SELECT 1 FROM dbo.SharedItemPrices h WHERE h.Id = l.Id)
UNION ALL SELECT 'missing on hub', 'SharedItemErpMappings', l.Id FROM @Mappings l WHERE NOT EXISTS (SELECT 1 FROM dbo.SharedItemErpMappings h WHERE h.Id = l.Id);

DECLARE @r TABLE (TableName sysname, Listed int, Updated int);
DECLARE @n int;

BEGIN TRANSACTION;

UPDATE h SET ReceivedAt = @Now FROM dbo.SharedItems h JOIN @Items l ON l.Id = h.Id; SET @n = @@ROWCOUNT;
INSERT @r VALUES ('SharedItems', (SELECT COUNT(*) FROM @Items), @n);

UPDATE h SET ReceivedAt = @Now FROM dbo.SharedItemPrices h JOIN @Prices l ON l.Id = h.Id; SET @n = @@ROWCOUNT;
INSERT @r VALUES ('SharedItemPrices', (SELECT COUNT(*) FROM @Prices), @n);

UPDATE h SET ReceivedAt = @Now FROM dbo.SharedItemErpMappings h JOIN @Mappings l ON l.Id = h.Id; SET @n = @@ROWCOUNT;
INSERT @r VALUES ('SharedItemErpMappings', (SELECT COUNT(*) FROM @Mappings), @n);

SELECT CASE WHEN @Commit = 1 THEN 'COMMITTED' ELSE 'DRY RUN (rolled back)' END AS Mode, @Now AS ReceivedAt, * FROM @r;

-- What the shops will pull: the listed rows as the hub holds them.
SELECT 'item' AS Kind, h.Id, h.Code, h.Name, CAST(NULL AS decimal(18,4)) AS BrutPrice, h.ModifiedAt, h.ModifiedByShopId
FROM dbo.SharedItems h JOIN @Items l ON l.Id = h.Id
UNION ALL
SELECT 'price', p.Id, i.Code, i.Name, p.BrutPrice, p.ModifiedAt, p.ModifiedByShopId
FROM dbo.SharedItemPrices p JOIN @Prices l ON l.Id = p.Id LEFT JOIN dbo.SharedItems i ON i.Id = p.ItemId
UNION ALL
SELECT 'mapping', m.Id, i.Code, i.Name, NULL, m.ModifiedAt, m.ModifiedByShopId
FROM dbo.SharedItemErpMappings m JOIN @Mappings l ON l.Id = m.Id LEFT JOIN dbo.SharedItems i ON i.Id = m.LocalItemId
ORDER BY Kind, Code;

IF @Commit = 1 COMMIT TRANSACTION; ELSE ROLLBACK TRANSACTION;
