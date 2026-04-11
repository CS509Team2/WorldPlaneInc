CREATE INDEX IF NOT EXISTS idx_deltas_depart ON deltas (DepartAirport, DepartDateTime);
CREATE INDEX IF NOT EXISTS idx_southwests_depart ON southwests (DepartAirport, DepartDateTime);
