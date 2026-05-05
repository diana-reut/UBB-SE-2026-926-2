# UBB-SE-2026-926-2

## Local configuration

The merged desktop app reads its database connection string from a local file that is intentionally not committed to git.

Tracked example file:

`UBB-SE-2026-MysteryInc-main/HospitalManagement/config/appsettings.example.json`

Local file you must create on your machine:

`UBB-SE-2026-MysteryInc-main/HospitalManagement/config/appsettings.local.json`

## Setup steps

1. Copy `appsettings.example.json` to `appsettings.local.json` in the same `config` folder.
2. Edit `DefaultConnection` in `appsettings.local.json` to point to your SQL Server or LocalDB instance.
3. Build and run `HospitalManagement`.

Example LocalDB connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HospitalManagementDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  }
}
```

## Notes

- `appsettings.local.json` is ignored by git.
- `appsettings.example.json` is the only config template that should be committed.
- On startup, the app will fail fast with a clear message if `appsettings.local.json` is missing.

## Database setup

If you want to use your local SQL Server instance, set `DefaultConnection` like this:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ros_tony_asus\\MSSQLSERVER01;Database=HospitalManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Recommended script order for a clean local demo database:

1. `UBB-SE-2026-House-MD-main/ERManagementSystem/Database/script1_create_database.sql`
2. `UBB-SE-2026-House-MD-main/ERManagementSystem/Database/script2_seed_data.sql`
3. `UBB-SE-2026-House-MD-main/ERManagementSystem/Database/script6_seed.sql`

Optional extra demo/test scripts:

- `UBB-SE-2026-House-MD-main/ERManagementSystem/Database/script3_insert_more_data.sql`
- `UBB-SE-2026-House-MD-main/ERManagementSystem/Database/script4_seed_more_data_for_blood.sql`
- `UBB-SE-2026-House-MD-main/ERManagementSystem/Database/script4_drug_shopping_test_data.sql`
- `UBB-SE-2026-House-MD-main/ERManagementSystem/Database/script5_seed_Ion_went_to_ER.sql`

Recommended first run:

- Run only `script1_create_database.sql`, `script2_seed_data.sql`, and `script6_seed.sql`.
- Start the app and verify the merged flows work.
- Add the optional scripts only if you need more demo scenarios.
