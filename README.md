### Миграции
(Запуск в корне проекта)
Не забывать выполнять Clear и Build Solution.
#### - Без билда 
```sh
dotnet ef migrations add MigrationName -p EmployeeAccounting/src/Infrastructure -s EmployeeAccounting/src/Web --no-build
```

#### - С билдом
```sh
dotnet ef migrations add MigrationName -p EmployeeAccounting/src/Infrastructure -s EmployeeAccounting/src/Web --no-build
```

#### Обновить БД
```sh
dotnet ef database update -p EmployeeAccounting/src/Infrastructure -s EmployeeAccounting/src/Web --no-build
```

#### Откатить миграцию 
```sh
dotnet ef database update MigrationName -p EmployeeAccounting/src/Infrastructure -s EmployeeAccounting/src/Web --no-build &&/
dotnet ef migrations remove -p EmployeeAccounting/src/Infrastructure -s EmployeeAccounting/src/Web --no-build
```