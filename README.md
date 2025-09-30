# Boardis ![.NET](https://img.shields.io/badge/.NET-9.0-blue) ![License](https://img.shields.io/badge/license-MIT-green)

A Kanban-style web API built with ASP.NET Core 9, featuring board, list, and card management with real-time notifications via SignalR. It integrates the Gemma2:2b LLM with Ollama, Keycloak for authentication, Redis for caching, PostgreSQL for data storage, Azurite for blob storage, and follows Clean Architecture principles. The frontend is built using React with Nginx as a reverse proxy. Supports localization (English, Polish) and permission-based access control.

Features also an optional observability stack(OpenTelemetry, Grafana, Prometheus, Loki, Jaeger)
## Features

- **User Accounts**
  - Register and log in using Keycloak authentication.
  - Secure JWT token exchange via Keycloak.

- **Boards**
  - Create boards with a title and optional description.
  - Delete boards (restricted to board owners).
  - Search boards by content criteria.
  - Board members with **Update** permission can:
    - Update board title and description.
    - Change board wallpaper.
  - Board members with **Delete** permission can delete boards.

- **Lists**
  - Create lists with a title.
  - Board members with **Update** permission can:
    - Update list title, color, and position.
  - Board members with **Delete** permission can delete lists.

- **Cards**
  - Create cards with a title.
  - Board members with **Update** permission can:
    - Update card title, description, and position.
  - Board members with **Delete** permission can delete cards.

- **Notifications via SignalR**
  - Receive real-time notifications for:
    - New board creation.
    - Removal from a board.
    - Updates to boards, lists, or cards you are a member of.
    - Invitations to or removal from a board.

- **AI-Powered Features**
  - Leverage Gemma2:2b LLM (via Ollama) for content generation and chat functionalities.


  ![Showcase](Showcase.gif)

## Technical Overview

- **Backend**: ASP.NET Core 9
- **Frontend**: React
- **Database**: PostgreSQL for storing boards, lists, and cards
- **Caching**: Redis for fast data lookups
- **Authentication**: Keycloak with JWT-based authentication and session revocation
- **Blob Storage**: Azurite (local Azure Blob Storage emulator) for board wallpapers
- **Reverse Proxy**: Nginx
- **AI Integration**: Ollama with Gemma2:2b LLM for content generation and chat
- **Containerization**: Docker Compose for service orchestration
- **Architecture**: Follows Clean Architecture principles
- **Monitoring and Observability**: OpenTelemetry for tracing, Prometheus for metrics, Grafana for visualization, Jaeger for distributed tracing, and Loki for log aggregation

## Requirements

- [.NET SDK 9.0.304](https://dotnet.microsoft.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer/) (for local blob containers)
- [Playwright](https://playwright.dev/dotnet/) (for functional tests)


## Setup and Run

1. Clone the repository:
   ```bash
   git clone https://github.com/Vandal166/Boardis.git
   ```

2. Run Docker Compose:
   ```bash
   docker compose up --build -d
   ```
   OR for ease of development:
   ```bash
   docker compose -f compose.yaml -f compose.dev.yaml up -d
   ```
**Optional Note:** The monitoring stack (including OpenTelemetry, Prometheus, Grafana, Jaeger, and Loki) is not required for the API to function but can be enabled for observability. To run only the monitoring stack:
   ```bash
   docker compose -f compose.monitoring.yaml up --build -d
   ```
   To run the main services along with the monitoring stack:
   ```bash
   docker compose -f compose.yaml -f compose.monitoring.yaml up --build -d
   ```
   
3. Navigate to the Web.API project:
   ```bash
   cd src/Web.API
   ```

4. Apply EF migrations:
   ```bash
   dotnet ef database update
   ```

5. Configure **Keycloak**:

   - Access the Keycloak admin panel at http://localhost:8081
   - Log in using:
     ```
     Username: admin
     Password: admin
     ```
   - Go to **Manage Realms → Create realm**.  
     Name it `BoardisRealm` and import the configuration from `/keycloak/config/realm-export.json`.

6. Configure client secrets in Keycloak:

   - For client `boardis-admin-cli`:  
     Go to **Credentials → Regenerate Client Secret**.  
     Copy the secret into `application.Development.json` under `AdminCliSecret` section.

   - For client `boardis-api`:  
     Do the same and copy into `ClientSecret` section.

7. Set up storage containers with [Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer)

   - Navigate to **Emulator & Attached → Emulator (Default Ports) → Blob Containers**
   - Create an container called: `media`.

8. Re-run docker compose for changes to apply:
   See point 2.

9. Run the frontend:
   ```bash
   cd frontend
   npm install
   npm run build
   ```


## Tests

Located under the `/tests` directory:

- **Web.API.FunctionalTests** → Tests authentication flow.

Before running functional tests, configure [Playwright](https://github.com/microsoft/playwright):

```powershell
cd .\tests\Web.API.FunctionalTests\
powershell bin/Debug/net9.0/playwright.ps1 install
```

---

## Notes

- Images/icons used in the app:  
  [Logo](https://www.flaticon.com/free-icons/to-do-list)

- If ollama-entrypoint.sh does not work try:
  docker exec -it ollama ollama pull gemma2:2b
