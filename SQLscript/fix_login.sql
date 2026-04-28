-- Fix users seeded by data.sql that have a dummy salt (0x1234, length < 16)
-- This resets Salt to empty (0x) so plain-text password comparison is used
UPDATE Users
SET Salt = 0x
WHERE LEN(Salt) < 16
  AND UserName IN ('user1', 'user2');

-- Verify
SELECT UserName, LEN(Salt) AS SaltLen, IsActive FROM Users;
