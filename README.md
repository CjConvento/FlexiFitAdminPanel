# FlexiFit Admin Panel — Centralized Back-Office Management Dashboard

**Status: Active Management Framework** — Core administration interfaces, relational data grids, workout tutorial assets, and system audit logs are fully operational, verified, and running in production configuration.

A high-performance, dark-themed internal web enterprise application built using the **ASP.NET Core MVC Pattern (.NET 8.0)** to serve as the centralized administrative control hub for the entire FlexiFit platform. This control portal communicates directly with the central **FlexiFit REST API** to enable platform administrators, metrics handlers, and fitness coaches to securely oversee database stats, perform system data management, and audit user compliance records.

> **Distributed Domain Architecture:** Adhering to standard enterprise design patterns, this back-office subsystem is completely decoupled from the consumer-facing native mobile applications. It runs on a dedicated web server node, ensuring that administrative database operations do not compete with consumer mobile API request boundaries.

---

## Technology Stack & Core Framework Layers

| System Component | Selected Technology Stack & Context |
|---|---|
| **Presentation Tier** | ASP.NET Core Razor Views (.cshtml), HTML5, CSS3, Modern Responsive Layout Frameworks |
| **Application Core** | C# / .NET 8.0 (Strict Separation of Concerns Architecture Pattern) |
| **Relational Data ORM** | Enterprise ViewModels mapping state validation controls to database engines |
| **Analytical Visualization** | Interactive chart arrays plotting high-frequency data (Growth Analytics & Database Data Splits) |
| **Target Infrastructure** | On-Premise Internet Information Services (IIS) Server / Cross-Platform hosting bundle ready |
| **Version Control** | Distributed Git Lifecycle utilizing GitHub repository branch tracking |

---

## Operational Console UI Blueprints

The dashboard layout features production-tested, low-latency data rendering tables:

1. **Secure Administrative Gate:** An isolated authentication engine (`Account/Login`) featuring zero hardcoded access properties, verified directly against system master credentials.
2. **Central Analytics Dashboard:** Renders core real-time metrics including **Total active consumers, exercise counts, and database asset item distributions** via an interactive SQL Database Data Split visualization chart layout.
3. **User Management Console:** Granular admin workspace allowing managers to provision data rows, modify privilege parameters, and update account properties safely.
4. **Workout & Content Controls:** Active dashboard panels allowing coaches to index exercise tracking criteria (Muscle Groups, Difficulty Levels, Met Calories, Environments) and map video tutorial presentation vectors directly.
5. **Centralized Compliance Activity Logs:** Administrative audit view compiling user operations, historical progress logs, and tracking metrics with specialized `FROM/TO` chronological selection matrices.

---

## Project Directory Tree & Structural Hierarchy

The solution isolates administrative MVC request controllers, data transfer view models, and styling sheets within decoupled workspace directories:

```text
FlexiFitAdminPanel/
├── Controllers/                         # MVC Lifecycle Request Interceptors
│   ├── AccountController.cs             # Handles administrator session allocation & logouts
│   ├── ActLogsController.cs             # Governs system transaction audits & compliance telemetry rows
│   ├── FoodsController.cs               # Executes operational configurations for food catalogs
│   ├── HomeController.cs                # Routes initial metrics aggregation requests
│   ├── UsersController.cs               # Oversees account credentials configurations and records
│   └── WorkoutsController.cs            # Formats structural parameters for exercise lists
├── Models/                              # Decoupled Presentation Data Transfer Objects (DTOs)
│   ├── DashboardViewModel.cs            # Collects analytics data for summary growth chart engines
│   ├── ErrorViewModel.cs                # Structural object mapping unhandled runtime faults
│   ├── FoodItem.cs                      # Nutritional parameter definition data models
│   ├── LoginViewModel.cs                # Enforces model validation boundaries for incoming admins
│   ├── User.cs                          # System metadata reference for backend accounts
│   ├── UserSession.cs                   # Encapsulates state tokens for active login footprints
│   └── WorkoutItem.cs                   # Relational blueprint for physical routine attributes
├── Views/                               # Dynamic Server-Side Razor Compilation Canvas Layouts
│   ├── Account/                         # Account Gateway Presentation Components
│   │   └── Login.cshtml                 # Secure admin login control sheet
│   ├── ActLogs/                         # Telemetry Management Templates
│   │   └── Index.cshtml                 # Central matrix displaying compliance and audit trails
│   ├── Foods/                           # Food Catalog Asset Managers
│   │   ├── Create.cshtml                # New nutritional element intake form layout
│   │   ├── Edit.cshtml                  # Food database records editor tool
│   │   └── Index.cshtml                 # Master data layout list for food items
│   ├── Home/                            # Overview Dashboard Interfaces
│   │   ├── Index.cshtml                 # Primary analytics graph presentation layer
│   │   └── Privacy.cshtml               # Global compliance data protection notice page
│   ├── Shared/                          # Reusable Structural Application Templates
│   │   ├── _AdminLayout.cshtml          # Parent shell navigation grid layout template
│   │   ├── _AdminLayout.cshtml.css      # Component stylesheet for shell presentation
│   │   ├── _ValidationScriptsPartial..  # Client-side form entry validator plugins bundle
│   │   └── Error.cshtml                 # Generic runtime fault diagnostic views
│   ├── Users/                           # User Identity Asset Controllers
│   │   ├── Create.cshtml                # Administrator privilege account creation form
│   │   ├── Edit.cshtml                  # User account parameters editing panel
│   │   └── Index.cshtml                 # Relational summary data grid for active accounts
│   └── Workouts/                        # Exercise Platform Management Elements
│       ├── _ViewImports.cshtml          # Global template using directive inclusions
│       └── _ViewStart.cshtml            # Primary layout execution shell binding script
├── wwwroot/                             # Static Physical Asset Distribution Engine
│   ├── css/                             # Decoupled Interface Stylesheets
│   │   ├── admin-style.css              # Central application skin rules layout
│   │   └── dashboard.css                # Precise formatting grid rules for graphs and analytics charts
│   ├── images/                          # Visual Icons & Graphic Media Assets
│   │   └── flexifit.png                 # Global master system identity design banner
│   └── js/                              # UI client tracking behavioral interceptors
├── Properties/                          # Solution Runtime EnvironmentBlueprints
│   └── launchSettings.json              # Local Web host execution ports definition file
├── appsettings.json                     # Production Configurations Overlay (Excluded from Version Control)
├── appsettings.Development.json         # Local Testing Constants & Target Database strings
├── appsettings.template.json            # Configuration template for staging setup
├── FlexiFit_AdminPanel.csproj           # Global compilation framework definitions manifest
├── FlexiFit_AdminPanel.sln              # Unified monolithic Visual Studio tracking solution
├── Program.cs                           # Primary bootstrap engine setting up runtime middleware layers
└── README.md                            # Comprehensive Architecture System Documentation
```

---

## Security Hardening & Data Protection Implementations

- **Strict Environment Separation:** All raw server credentials, connection settings, and system parameters have been completely migrated away from hardcoded configurations in favor of dynamic `appsettings.json` structural injections.
- **Decoupled ViewModels Integration:** Presentation layouts never bind directly to domain entities. All data routing fields pass through dedicated validation ViewModels (such as `LoginViewModel` and `DashboardViewModel`) to prevent mass-assignment vulnerabilities.
- **Audit-Compliance Tracking:** Includes a native data tracking dashboard component (`ActLogs`) linked to centralized database transactions to ensure transparent compliance analytics across the platform ecosystem.

---

## Local Staging Setup & Local Installation

### Prerequisites
Ensure your local environment includes the following workspace parameters before running:
- [.NET Core 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Integrated Development Environment (Visual Studio 2022 / VS Code)
- A running instance of the central [FlexiFit REST API](https://github.com/CjConvento/FlexiFit.Api)

### Installation Instructions

1. **Clone the repository framework:**
   ```bash
   git clone https://github.com/CjConvento/FlexiFitAdminPanel
   cd FlexiFitAdminPanel
   ```

2. **Configure Local Environment Overlays:**
   Establish your own connection routes inside `appsettings.Development.json` using the global `appsettings.template.json` blueprint file. Direct the target endpoint configuration toward your active running backend API server:
   ```json
   "ApiSettings": {
     "BaseUrl": "http://localhost:5160/api/"
   }
   ```

3. **Launch the Administrative Web Portal:**
   Execute compilation parameters inside your terminal window to ignite the web host rendering engine loop:
   ```powershell
   # Purge legacy build assets cache
   dotnet clean

   # Ignite the server execution loop
   dotnet run
   ```

---

## Modernization & Scaling Roadmap

- [ ] **UI Modernization Track:** Migrate the server-side Razor rendering engine architecture to high-performance component sheets utilizing **Blazor Interactive Server workflows**.
- [ ] **Bulk Automation Panel:** Implement administrative batch data-upload controls to bulk-insert system video asset elements via `.csv` or `.xlsx` automation.
- [ ] **Granular Security Tiers:** Integrate strict role constraints for back-office administrators (e.g., `SuperAdmin` vs. `Coach` read-only viewing locks).

---

## Project Author

**Natajimura**
- GitHub: [@CjConvento](https://github.com/CjConvento)
- LinkedIn: (https://www.linkedin.com/in/cyrenz-jonathan-convento-650a931b7/)
- Email: conventocj110@gmail.com

