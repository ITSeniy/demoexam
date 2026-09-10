# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Demonstration exam — ООО «Обувь» shoe store information system. WPF (C# / .NET 10) desktop app with MS SQL Server database.

## Commands

```bash
# Build
cd ObuVApp && dotnet build

# Run
cd ObuVApp && dotnet run

# Recreate and seed database (PowerShell)
$sql = Get-Content ObuVApp/Database/CreateDatabase.sql -Encoding UTF8 -Raw
Invoke-Sqlcmd -ServerInstance "localhost" -Query $sql
```

## Connection string

`Server=localhost;Database=ObuVDB;Trusted_Connection=True;TrustServerCertificate=True;`

Defined in `ObuVApp/Database/DbHelper.cs`.

## Architecture

```
ObuVApp/
  Database/
    DbHelper.cs           — connection string constant
    TovarRepository.cs    — CRUD for Товары table
    UserRepository.cs     — login/auth query
    OrderRepository.cs    — CRUD for Заказы + СоставЗаказа + ПунктыВыдачи
    CreateDatabase.sql    — full schema + seed data
  Models/
    User.cs               — User with role helpers (IsAdmin, IsManager, IsClient)
    Tovar.cs              — Product with computed ЦенаСоСкидкой, БольшаяСкидка
    Order.cs              — Order, OrderItem, PickupPoint
  Windows/
    LoginWindow           — startup; guest or authenticated entry
    ProductsWindow        — product catalog, role-gated filter/sort/search/CRUD
    TovarEditWindow       — add/edit product dialog
    OrdersWindow          — orders list, role-gated edit/delete
    OrderEditWindow       — add/edit order dialog with line items
  Resources/              — images (1.jpg–10.jpg), logo (picture.png), Icon.ico
```

## Role access matrix

| Feature | Гость | Клиент | Менеджер | Администратор |
|---------|-------|--------|----------|---------------|
| Просмотр товаров | ✓ | ✓ | ✓ | ✓ |
| Фильтр/сортировка/поиск товаров | — | — | ✓ | ✓ |
| CRUD товаров | — | — | — | ✓ |
| Просмотр заказов | — | — | ✓ | ✓ |
| CRUD заказов | — | — | — | ✓ |

## Style guide (Модуль 2)

- Font: Times New Roman everywhere
- Main background: `#FFFFFF`
- Additional background (headers, panels): `#7FFF00`
- Accent (primary action buttons): `#00FA9A`
- Product card background when discount > 15%: `#2E8B57`
- Logo (`picture.png`) must not be distorted — always `Stretch="Uniform"`
- App icon: `Icon.ico` (converted from PNG via Pillow to proper ICO format)

## Database schema

Key tables: `Товары` (Артикул PK), `Пользователи` → `Роли`, `Заказы` → `ПунктыВыдачи`, `СоставЗаказа` (join between заказы and товары). Cascade delete on СоставЗаказа when order is deleted.
