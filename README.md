# FlowBoard

> Real-time collaborative kanban board with AI assistant and interactive whiteboard

[![Live Site](https://img.shields.io/badge/Live-FlowBoard-blue?logo=microsoft-azure)](https://red-pond-0f305f80f.4.azurestaticapps.net)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)](https://angular.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Live Demo:** [https://red-pond-0f305f80f.4.azurestaticapps.net](https://red-pond-0f305f80f.4.azurestaticapps.net)

## Features

### Real-time Kanban Board
- **Drag & Drop** - Smooth task management with instant synchronization
- **Live Collaboration** - See your team's changes in real-time via SignalR
- **Conflict Resolution** - Smart handling of concurrent edits

### AI Assistant
- **Natural Language** - Create tasks with simple commands
- **Smart Analysis** - Get insights about your board and team workload
- **Intelligent Suggestions** - AI-powered task assignment recommendations

### Interactive Whiteboard
- **System Design** - Collaborate on architecture diagrams with Excalidraw
- **Real-time Cursors** - See where your teammates are working
- **Export Options** - Save your work as PNG/SVG

### Performance
- **Virtual Scrolling** - Handle 500+ tasks smoothly
- **Optimistic Updates** - Instant UI feedback
- **Smart Caching** - Fast page loads

## Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| ASP.NET Core 10 | Web API framework |
| SignalR | Real-time communication |
| Entity Framework Core + PostgreSQL | Data access |
| MediatR (CQRS) | Clean Architecture |
| FluentValidation | Request validation |
| Azure Key Vault | Secrets management |
| Application Insights | Monitoring & telemetry |

### Frontend
| Technology | Purpose |
|---|---|
| Angular 21 | UI framework (Standalone Components) |
| Angular Material | UI component library |
| Angular CDK | Drag & Drop |
| RxJS | Reactive state management |
| Excalidraw | Interactive whiteboard |
| @microsoft/signalr | Real-time client |

## Architecture

```
FlowBoard/
├── src/                              # Backend (.NET 10)
│   ├── FlowBoard.API/               # Web API + SignalR Hubs
│   ├── FlowBoard.Core/              # Domain entities & interfaces
│   ├── FlowBoard.Application/       # Commands, Queries, Handlers (CQRS)
│   └── FlowBoard.Infrastructure/    # Data access & external services
│
├── flowboard-web/                    # Frontend (Angular 21)
│   └── src/app/
│       ├── core/                     # Services, Guards, Interceptors
│       ├── shared/                   # Reusable components & pipes
│       ├── features/                 # Feature modules (lazy-loaded)
│       │   ├── boards/              # Kanban board
│       │   ├── canvas/              # Whiteboard
│       │   └── ai/                  # AI assistant
│       └── layout/                  # App layout
│
├── .github/workflows/                # CI/CD
│   ├── deploy-backend.yml           # Backend → Azure App Service
│   └── deploy-frontend.yml          # Frontend → Azure Static Web Apps
│
└── docs/                             # Documentation
```

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [pnpm](https://pnpm.io/)
- PostgreSQL (local or Docker)

### Setup

```bash
# Clone repository
git clone https://github.com/PlonGuo/flowboard.git
cd flowboard

# Start backend (with hot reload)
pnpm dev:back

# Start frontend (in a separate terminal)
pnpm dev:front
```

- Backend: `http://localhost:5254`
- Frontend: `http://localhost:4200`

### Configuration

**Backend** - Create `src/FlowBoard.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=flowboard;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "FlowBoard",
    "Audience": "FlowBoard"
  }
}
```

**Frontend** - Environment is auto-configured:
- Development: `flowboard-web/src/environments/environment.ts` (localhost)
- Production: `flowboard-web/src/environments/environment.prod.ts` (Azure)

## Deployment

FlowBoard is deployed on **Microsoft Azure**:

| Service | Azure Resource | URL |
|---|---|---|
| Frontend | Azure Static Web Apps | [red-pond-0f305f80f.4.azurestaticapps.net](https://red-pond-0f305f80f.4.azurestaticapps.net) |
| Backend API | Azure App Service | flowboard-api-0209.azurewebsites.net |
| Database | Azure PostgreSQL Flexible Server | flowboard-db-0209 |
| Secrets | Azure Key Vault | flowboard-kv-0209 |
| Monitoring | Application Insights | flowboard-insights |

CI/CD is handled via GitHub Actions - pushing to `main` automatically deploys both frontend and backend.

For detailed deployment instructions, see:
- [Azure Deployment Guide](docs/AZURE_DEPLOYMENT.md)
- [Deployment Troubleshooting](docs/DEPLOYMENT_TROUBLESHOOTING.md)

## Development

### Common Commands

```bash
# Start backend with hot reload
pnpm dev:back

# Start frontend dev server
pnpm dev:front

# Run all tests
pnpm test

# Backend only
cd src/FlowBoard.API && dotnet watch run

# Frontend only
cd flowboard-web && pnpm start

# Create EF migration
dotnet ef migrations add MigrationName -p src/FlowBoard.Infrastructure -s src/FlowBoard.API

# Update database
dotnet ef database update -p src/FlowBoard.Infrastructure -s src/FlowBoard.API
```

## Documentation

- [Azure Deployment Guide](docs/AZURE_DEPLOYMENT.md) - Full deployment walkthrough
- [Deployment Troubleshooting](docs/DEPLOYMENT_TROUBLESHOOTING.md) - Common deployment issues & fixes
- [Design Document](docs/DESIGN.md) - Complete technical design
- [API Specification](docs/API_SPEC.md) - API endpoints & formats
- [Testing Guide](docs/TESTING.md) - Testing standards

## Roadmap

- [x] Project setup and Clean Architecture
- [x] User authentication & authorization (JWT)
- [x] Real-time kanban board (SignalR)
- [x] Task management (CRUD + drag & drop)
- [x] Interactive whiteboard (Excalidraw)
- [x] Team collaboration features
- [x] Azure deployment (App Service + Static Web Apps + PostgreSQL)
- [x] CI/CD with GitHub Actions
- [ ] AI assistant integration
- [ ] Mobile responsive design
- [ ] Performance optimization (virtual scrolling)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Jason Guo** - [@PlonGuo](https://github.com/PlonGuo)

## Acknowledgments

- [Excalidraw](https://excalidraw.com/) - Whiteboard component
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) - Real-time capabilities
- [Angular Material](https://material.angular.io/) - UI components
- [MediatR](https://github.com/jbogard/MediatR) - CQRS implementation

---

**Star this repo if you find it helpful!**

**Questions?** Open an [issue](https://github.com/PlonGuo/flowboard/issues)
