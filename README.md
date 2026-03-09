# WorldPlaneInc

A .NET 10 web application for flight data management, built with a layered architecture and a MySQL database, all running inside a Dev Container.

## Project Structure

```
backend/
├── api/          # ASP.NET Core Web API (entry point)
├── model/        # Domain models (references dal, utils)
├── dal/          # Data Access Layer (MySQL via MySql.Data)
└── utils/        # Shared utilities
```

- `create_user.sql` — SQL script that creates the `users` table
- `flightdata_*.sql` — SQL dumps for flight data
- `.devcontainer/` — Dev Container configuration (Docker Compose, setup script)

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [VS Code](https://code.visualstudio.com/) with the **Dev Containers** extension (`ms-vscode-remote.remote-containers`)

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone  git clone --recursive --branch env-setup git@github.com:CS509Team2/WorldPlaneInc.git FILENAME

2. **Open in VS Code** and reopen in the Dev Container:
   - Open the Command Palette (`Cmd+Shift+P` / `Ctrl+Shift+P`)
   - Select **Dev Containers: Reopen in Container**
   - Wait for the container to build (this installs dependencies and sets up the database automatically)

3. **If the database setup didn't run automatically**, run it manually from the devcontainer terminal:
   ```bash
   .devcontainer/setup-db.sh db 3306
   ```

4. Run .devcontainer/setup-db.sh db 3306

## Database

The Dev Container runs a **MySQL 8.0** instance automatically.

| Setting  | Value         |
|----------|---------------|
| Host     | `db`          |
| Port     | `3306`        |
| User     | `root`        |
| Password | `rootpassword`|
| Database | `app`         |

To connect from your **host machine** (e.g., a local MySQL client), use `127.0.0.1` on port `3333`.

## VS Code Extensions (auto-installed)

- C# (`ms-dotnettools.csharp`)
- C# Dev Kit (`ms-dotnettools.csdevkit`)
- MySQL Client (`cweijan.vscode-mysql-client2`)
- EditorConfig (`editorconfig.editorconfig`)
