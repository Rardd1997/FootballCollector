# Add new migration
```
dotnet ef migrations add <migration-name> --project Football.Collector.Data --startup-project Football.Collector.Api
```

[Useful Link](https://rajbos.github.io/blog/2020/04/23/EntityFramework-Core-NET-Standard-Migrations)

# Update database

```
dotnet ef database update --project Football.Collector.Data --startup-project Football.Collector.Api
```
