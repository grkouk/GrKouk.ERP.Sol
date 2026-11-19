/*
  Migrations_2026-11-18_SharedExpirySettings.sql : EF-equivalent, idempotent script for migration
  20261118200000_AddSharedExpirySettings (expiry settings travel between shops).

  Pure DDL. Runs only if the MigrationId is not yet in __EFMigrationsHistory (same shape as
  `dotnet ef migrations script --idempotent`). Safe to run twice.
  Run against the hub DB (GrKoukDB), after 20261110163448_AddSharedItemCodeReceivedAt.

  The item/category columns are nullable on purpose: null = no shop that knows the fields has
  pushed the row yet. An older ERP build on the same DB keeps working (it never reads them; its
  transfer-line inserts get the 0 default).
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261118200000_AddSharedExpirySettings')
BEGIN
    ALTER TABLE [SharedItems] ADD [ExpiryRequirement] int NULL;
    ALTER TABLE [SharedItems] ADD [ShelfLifeDays] int NULL;
    ALTER TABLE [SharedItemCategories] ADD [RequiresExpiry] bit NULL;
    ALTER TABLE [SharedItemCategories] ADD [ShelfLifeDays] int NULL;
    ALTER TABLE [SharedStockTransferLines] ADD [IsExpiryEstimated] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261118200000_AddSharedExpirySettings')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261118200000_AddSharedExpirySettings', N'10.0.11');
END;

COMMIT;
