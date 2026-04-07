# Local Development Setup

## Prerequisites

- Docker Desktop
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Start MSSQL

```bash
docker compose up -d
```

To stop it:

```bash
docker compose down
```

## Apply Migrations

```bash
dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub
```

## Connect with DBeaver

1. New Database Connection > SQL Server
2. Use these settings:
   - **Host**: `localhost`
   - **Port**: `1433`
   - **Authentication**: SQL Server Authentication
   - **Username**: `sa`
   - **Password**: `EventHub@Dev123`
   - **Database**: `EventHub`
3. On the **Driver properties** tab set:
   - `encrypt` = `true`
   - `trustServerCertificate` = `true`
4. Test Connection > Finish

## Local config

EventHub\appsettings.json

```bash
"DevConnection": "Server=localhost,1433; Database=EventHub; User Id=sa; Password=EventHub@Dev123; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;",
```