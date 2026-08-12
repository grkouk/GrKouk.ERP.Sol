-- Pre-flight check for the AddPasskeySupport migration.
--
-- Enabling passkeys sets IdentityOptions.Stores.SchemaVersion = Version3. Schema versions are cumulative,
-- so that also applies the Version2 column narrowing to tables that already hold data. SQL Server refuses
-- an ALTER COLUMN that would truncate an existing value, so the migration FAILS (rather than silently
-- losing data) if any row below comes back non-zero.
--
-- Run this against production BEFORE applying the migration. Every count must be 0.

SELECT 'AspNetUsers.PhoneNumber > 256'      AS Check_Name,
       COUNT(*)                             AS Offending_Rows
FROM   AspNetUsers
WHERE  LEN(PhoneNumber) > 256

UNION ALL
SELECT 'AspNetUserLogins.LoginProvider > 128',
       COUNT(*)
FROM   AspNetUserLogins
WHERE  LEN(LoginProvider) > 128

UNION ALL
SELECT 'AspNetUserLogins.ProviderKey > 128',
       COUNT(*)
FROM   AspNetUserLogins
WHERE  LEN(ProviderKey) > 128

UNION ALL
SELECT 'AspNetUserTokens.LoginProvider > 128',
       COUNT(*)
FROM   AspNetUserTokens
WHERE  LEN(LoginProvider) > 128

UNION ALL
SELECT 'AspNetUserTokens.Name > 128',
       COUNT(*)
FROM   AspNetUserTokens
WHERE  LEN(Name) > 128;
