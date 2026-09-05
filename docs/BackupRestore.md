# Database Backup & Restore Guide

This guide covers PostgreSQL database backups, log storage, and restoration procedures.

## Daily Database Dumps (pg_dump)
To perform a complete backup of the database:
```bash
pg_dump -h localhost -U postgres -d travel_system_db -F c -b -v -f C:\backups\travel_db_backup.dump
```
- `-F c`: Output format is custom, which is compressed and restorable via pg_restore.
- `-b`: Include large objects in the dump.

## Complete Restore Procedure (pg_restore)
To restore a backup into a clean database:
1. Re-create the database:
   ```sql
   DROP DATABASE travel_system_db;
   CREATE DATABASE travel_system_db;
   ```
2. Restore the dump file:
   ```bash
   pg_restore -h localhost -U postgres -d travel_system_db -v C:\backups\travel_db_backup.dump
   ```
