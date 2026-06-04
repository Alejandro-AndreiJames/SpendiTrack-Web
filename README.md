# SpendiTrack

SpendiTrack is a web-based personal expenditure tracker and budget calculator designed to help users record daily spending, view comprehensive financial summaries, and plan disposable income effectively.

The application is built using modern cross-platform patterns with an ASP.NET Core MVC framework on the backend and an interactive web fronted.

---

## 🚀 Features

*   **Budget Calculator:** Estimate daily and weekly disposable income caps dynamically after accounting for savings goals and fixed monthly costs (e.g., rent, utilities, insurance).
*   **Expense Tracker:** A central dashboard visualizing financial health metrics including Lifetime Spending, Current Month Outlays, and total Recorded Transactions.
*   **Search Engine:** Find previously logged expenses instantly by description keywords.
*   **Route Protection & Authorization:** Integrates secure identity guarding to lock down tracker modifications and log entry creation behind an authentication requirement.

---

## 🛠️ Tech Stack & Languages

As tracked in the repository metrics (`image_69290f.png`):

*   **Backend:** C# (.NET 8 / .NET Core MVC)
*   **Identity Provider:** ASP.NET Core Identity
*   **Frontend UI:** HTML5, CSS3 (Custom responsive layouts), and JavaScript

---

## 📂 Project Structure

```text
SpendiTrackWeb/
├── Controllers/         # Application logic controllers (Home, Expenses, Auth)
├── Data/                # Database contexts and data migrations
├── Models/              # Domain models and request/response viewmodels
├── Views/               # Razor views for presentation layer rendering
└── wwwroot/             # Static web assets (custom CSS layouts, JS, images)
