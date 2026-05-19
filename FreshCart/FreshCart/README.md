# FreshCart - E-Grocery System

An ASP.NET MVC e-grocery system built with .NET 8, Entity Framework Core, and SQL Server Express.

## Features

- User authentication with session management
- Role-based access control (Customer, Staff, Admin)
- Product browsing with search and category filtering
- Shopping cart with database persistence
- Order management with status tracking
- Admin dashboard with analytics
- Password reset (simulated email)
- Responsive Bootstrap 5 UI

## Demo Credentials

| Role     | Username | Password    |
|----------|----------|-------------|
| Admin    | admin    | Admin@123   |
| Staff    | staff    | staff123    |
| Customer | user     | user123     |

## Prerequisites

- .NET 8 SDK
- SQL Server Express
- SQL Server Management Studio (SSMS)
- Visual Studio 2022 (or VS Code)

## Setup Instructions

### 1. Clone or create the project

```bash
dotnet new mvc -n FreshCart.Web
cd FreshCart.Web