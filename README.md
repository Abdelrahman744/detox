# Dopamine Detox Ledger

A focused, minimalist web application built to track deep-work sessions and behavioral restriction protocols (e.g., social media fasts, sugar restrictions). 

## 🚀 Features

- **The Archive**: View all past focus blocks and restriction sessions in a clean, filterable list.
- **The Terminal**: Log new focus blocks, capturing the activity restricted, duration, success status, and reflection notes.
- **Multi-Unit Durations**: Track sessions accurately in Seconds, Hours, Days, or Months.
- **User Isolation**: Full multi-tenant security architecture ensuring users only see their own logs.
- **Spotify-Inspired Theme**: Modern, high-contrast dark mode UI featuring Google's Inter font and vibrant green accents.

## 🛠 Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 10.0)
- **Database**: SQLite (Embedded lightweight database)
- **ORM**: Entity Framework Core (Code-First)
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Views (`.cshtml`), Vanilla CSS (Spotify-inspired dark theme)

---

## 🗄️ Database & Schema

The application uses a lightweight **SQLite** database (`app.db`). The schema is strictly constrained to ensure a minimal footprint.

### `AspNetUsers` (Identity Table)
Managed automatically by ASP.NET Core Identity.
- `Id` (NVARCHAR) - Primary Key
- `Email`, `PasswordHash`, `UserName`, etc.

### `FocusBlocks` (Core Data Table)
The core table storing session metrics.
- `Id` (INTEGER) - Primary Key, Auto-Increment
- `UserId` (NVARCHAR) - Foreign Key linking to `AspNetUsers.Id`
- `ActivityRestricted` (NVARCHAR) - e.g., "Social Media", "Video Games"
- `DurationValue` (DECIMAL) - The numerical length of the session
- `DurationUnit` (NVARCHAR) - The unit of time ("Seconds", "Hours", "Days", "Months")
- `SuccessStatus` (BOOLEAN) - `True` if the fast was maintained, `False` if broken
- `Date` (DATETIME) - The date the block occurred
- `Reflection` (NVARCHAR) - Optional field for analytical notes (Max 500 chars)

---

## 💻 How to Run Locally

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) installed.
- (Optional) Entity Framework Core CLI tools installed (`dotnet tool install --global dotnet-ef`).

### Setup & Run

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd detox
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Apply Database Migrations:**
   *(Note: The `app.db` file will be generated automatically upon running this command).*
   ```bash
   dotnet ef database update
   ```

4. **Run the Server:**
   ```bash
   dotnet run --urls "http://localhost:5000"
   ```

5. **Access the Application:**
   Open your browser and navigate to `http://localhost:5000`. Register a new account to begin tracking your protocol.

---

## 🔒 Security Architecture
- **Data Isolation:** The application strictly prevents users from accessing focus blocks logged by others. The `UserId` is securely extracted server-side via the `UserManager` class during HTTP POST actions.
- **Route Protection:** The `[Authorize]` attribute protects all endpoints in the `FocusBlocksController`.
- **Validation:** Entity Framework Data Annotations strictly enforce required fields and data types before committing to the database.
