-- Only create indexes if the flight tables exist.
-- These will be skipped safely if the tables haven't been imported yet.

SET @tbl_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'deltas');
SET @sql = IF(@tbl_exists > 0, 'CREATE INDEX idx_deltas_depart ON deltas (DepartAirport, DepartDateTime)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @tbl_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'southwests');
SET @sql = IF(@tbl_exists > 0, 'CREATE INDEX idx_southwests_depart ON southwests (DepartAirport, DepartDateTime)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
