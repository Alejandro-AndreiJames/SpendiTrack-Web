# SpendiTrack

SpendiTrack is a personal finance web application designed to help users manage monthly budgets, monitor spending, and maintain better financial discipline. The application enables users to establish an income-based spending limit, allocate budgets across expense categories, and track daily transactions while providing clear visibility into budget utilization.

The application is built using **ASP.NET Core MVC**, **Entity Framework Core**, and **ASP.NET Core Identity**.

---

## Features

### Dashboard

The home dashboard provides quick access to essential financial planning tools.

* **Budget Calculator** – Calculates estimated daily and weekly disposable income based on monthly income, savings goals, and fixed expenses.
* **Standard Calculator** – Includes a built-in calculator with keyboard support for desktop users.
* **User Guide** – Provides step-by-step instructions for account setup, budget configuration, and expense tracking.

---

### Budget Management

Users can establish a monthly budget by specifying:

* Monthly income
* Savings percentage
* Fixed monthly expenses

Based on these values, the system automatically calculates the available monthly spending limit.

---

### Category Budget Allocation

Users can distribute their available spending limit across multiple expense categories.

Key capabilities include:

* Allocation of category-specific budgets
* Remaining budget calculation
* Validation to prevent budget over-allocation
* Budget modification throughout the month

---

### Expense Tracking

The application provides complete expense management functionality, allowing users to:

* Add expenses
* View expense records
* Edit existing entries
* Delete expenses
* Duplicate previous transactions
* Assign categories
* Record transaction dates and descriptions

Expense tracking is organized on a monthly basis to simplify financial monitoring.

---

### Budget Utilization

SpendiTrack continuously monitors spending against allocated category budgets.

Users can view:

* Current spending per category
* Remaining available budget
* Budget utilization percentage
* Overall monthly spending progress

---

### Transaction History

Historical records can be accessed through:

* Transaction search
* Monthly history browsing
* CSV export of the current month's transactions

---

### Budget Initialization

Expense management features remain unavailable until a monthly budget has been configured, ensuring that all recorded transactions are associated with an active budget.

---

## Account Management

User authentication and account management are implemented using **ASP.NET Core Identity**.

Supported features include:

* User registration
* Email confirmation
* Secure authentication
* Password reset
* Profile management
* Optional two-factor authentication (2FA)

---

## User Experience

The application is designed to provide a consistent experience across desktop and mobile devices.

Features include:

* Responsive user interface
* Sticky navigation bar
* Customizable application themes
* Persistent theme preferences stored per user account

---

## Technology Stack

| Component                | Technology                                      |
| ------------------------ | ----------------------------------------------- |
| Framework                | ASP.NET Core MVC (.NET 8)                       |
| Programming Language     | C#                                              |
| Database                 | SQL Server                                      |
| Object-Relational Mapper | Entity Framework Core                           |
| Authentication           | ASP.NET Core Identity                           |
| Email Service            | SMTP                                            |
| Frontend                 | Razor Views, HTML5, CSS3, Bootstrap, JavaScript |

---

## Project Structure

```text
SpendiTrackWeb/
├── Areas/
│   └── Identity/          # Authentication and account management
├── Controllers/           # MVC controllers
├── Data/                  # Database context and migrations
├── Models/                # Domain models and view models
├── Services/              # Business logic and supporting services
├── Views/                 # Razor views and shared layouts
└── wwwroot/               # Static assets (CSS, JavaScript, images)
```

---

## Getting Started

### Prerequisites

* .NET 8 SDK
* SQL Server
* Visual Studio 2022 or later

### Installation

Clone the repository:

```bash
git clone https://github.com/your-username/SpendiTrack.git
```

Navigate to the project directory:

```bash
cd SpendiTrack
```

Restore project dependencies:

```bash
dotnet restore
```

Apply the Entity Framework Core migrations:

```bash
dotnet ef database update
```

Run the application:

```bash
dotnet run
```
