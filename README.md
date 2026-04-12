# WorldPlaneInc

A .NET 10 web application for flight data management, built with a layered architecture and a MySQL database, all running inside a Dev Container.

## Project Structure

```
backend/
├── api/          # ASP.NET Core Web API (entry point)
│   └── Controllers/
│       ├── FlightsController.cs   # Flight search & random flights
│       ├── LoginController.cs     # Login & signup
│       └── SeatsController.cs     # Seat map & booking
├── model/        # Domain models (references dal, utils)
│   ├── FlightSearchModel.cs       # Multi-leg itinerary search
│   ├── LoginModel.cs              # Auth logic
│   └── SeatModel.cs               # Seat map & reservation logic
├── dal/          # Data Access Layer (MySQL via MySql.Data)
│   ├── Dal.cs                     # Login queries
│   ├── FlightSearchDal.cs         # Flight search queries
│   ├── FlightQueryDal.cs          # Random flight sampling
│   └── SeatDal.cs                 # Seat CRUD & booking transactions
└── utils/        # Shared utilities

frontend/
├── login.html         # Login page
├── signup.html        # Registration page
├── home.html          # Dashboard with hero + feature cards
├── search.html        # Flight search, results, sort & filter
├── seats.html         # Interactive seat map selection
├── confirmation.html  # Booking confirmation
├── app.js             # All API calls, search/filter/sort logic, seat map
└── style.css          # Brand overrides on top of Bootstrap 5
```

- `create_user.sql` — Creates the `users` table and sample user
- `create_seats.sql` — Creates `seats` and `bookings` tables
- `create_indexes.sql` — Adds performance indexes on flight tables
- `flightdata_*.sql` — SQL dumps for flight data (Delta & Southwest)
- `.devcontainer/` — Dev Container configuration (Docker Compose, setup script)

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [VS Code](https://code.visualstudio.com/) or [Cursor](https://cursor.sh/) with the **Dev Containers** extension (`ms-vscode-remote.remote-containers`)

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone git@github.com:CS509Team2/WorldPlaneInc.git
   cd WorldPlaneInc
   ```

2. **Open in VS Code/Cursor** and reopen in the Dev Container:
   - Open the Command Palette (`Cmd+Shift+P` / `Ctrl+Shift+P`)
   - Select **Dev Containers: Reopen in Container**
   - Wait for the container to build (installs dependencies and sets up the database automatically)

3. **If the database setup didn't run automatically**, run it manually from the devcontainer terminal:
   ```bash
   bash .devcontainer/setup-db.sh
   ```

4. **Start the backend API:**
   ```bash
   cd /workspace/backend/api && dotnet run
   ```
   The API starts on `http://localhost:5237`.

5. **Serve the frontend** (in a second terminal):
   ```bash
   dotnet tool install -g dotnet-serve
   export PATH="$PATH:$HOME/.dotnet/tools"
   dotnet serve -d /workspace/frontend -p 8080
   ```

6. **Open the app** in your browser at `http://localhost:8080/login.html`.

## Using the App

1. **Login** with `user1` / `1111`, or click **Sign Up** to create a new account
2. On the **Home** dashboard, click **Search Flights**
3. Enter departure/arrival airports (use suggestions like `Atlanta (ATL)`, `Los Angeles (LAX)`), pick a date, and click **Search Flights**
4. **Sort** results by duration, stops, or departure time; **filter** by airline or max stops
5. Click **Select Seat** on a flight to open the **Seat Map**
6. Click a green/gold/orange seat to select it, then click **Confirm Seat**
7. View your **Booking Confirmation**

## API Endpoints

| Method | Route                          | Description                          |
|--------|--------------------------------|--------------------------------------|
| POST   | `/Login/api/login`             | Authenticate user                    |
| POST   | `/Login/api/signup`            | Register new user                    |
| GET    | `/Flights/getNextFlights`      | Random sample of flights             |
| POST   | `/Flights/search`              | Search itineraries (direct + connecting) |
| GET    | `/Seats?flightNumber=&airline=`| Get seat map for a flight            |
| POST   | `/Seats/book`                  | Book a seat                          |

## Database

The Dev Container runs a **MySQL 8.0** instance automatically.

| Setting  | Value          |
|----------|----------------|
| Host     | `db` (in container) / `127.0.0.1` (from host) |
| Port     | `3306` (in container) / `3333` (from host) |
| User     | `root`         |
| Password | `rootpassword` |
| Database | `app`          |

### Tables

- **users** — Username/password for authentication
- **deltas** — Delta flight data (imported from `flightdata_*.sql`)
- **southwests** — Southwest flight data (imported from `flightdata_*.sql`)
- **seats** — Seat inventory per flight (auto-generated on first access)
- **bookings** — Seat reservations linked to users

## Recent Changes (feature/ui-performance-seat-selection)

### UI Overhaul
- Added **Bootstrap 5** via CDN to all pages with a consistent navbar
- Restyled login, signup, and home pages with Bootstrap components
- Created **flight search page** with form, inline results, sort (duration/stops/departure), and filter (airline, max stops)
- Created **seat selection page** with interactive 30-row x 6-column seat grid (color-coded by class and availability)
- Created **booking confirmation page**

### Seat Selection (End-to-End)
- New `seats` and `bookings` database tables
- Backend: `SeatDal`, `SeatModel`, `SeatsController` with `GET /Seats` and `POST /Seats/book`
- Seats auto-generated per flight (First/Business/Economy) on first access
- Transactional booking with `SELECT ... FOR UPDATE` to prevent double-booking

### Backend Performance
- **Login**: Replaced `SELECT * FROM users` + in-memory scan with parameterized `SELECT COUNT(*)` query
- **Random flights**: Replaced `ORDER BY RAND()` (full table scan) with offset-based random sampling
- **Indexes**: Added composite indexes on `(DepartAirport, DepartDateTime)` for both flight tables
- **Async I/O**: All DAL methods converted to `async/await`
- **Response compression**: Added Gzip compression middleware
- **Config-based connection strings**: `LoginDal` no longer uses a hardcoded connection string

## VS Code Extensions (auto-installed in Dev Container)

- C# (`ms-dotnettools.csharp`)
- C# Dev Kit (`ms-dotnettools.csdevkit`)
- MySQL Client (`cweijan.vscode-mysql-client2`)
- EditorConfig (`editorconfig.editorconfig`)
