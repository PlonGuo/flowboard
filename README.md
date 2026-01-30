# FlowBoard 🎯

> Real-time collaborative kanban board with AI assistant and interactive whiteboard

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)](https://angular.io/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

## ✨ Features

### 🎯 Real-time Kanban Board
- **Drag & Drop**: Smooth task management with instant synchronization
- **Live Collaboration**: See your team's changes in real-time
- **Conflict Resolution**: Smart handling of concurrent edits

### 🤖 AI Assistant
- **Natural Language**: Create tasks with simple commands
- **Smart Analysis**: Get insights about your board and team workload
- **Intelligent Suggestions**: AI-powered task assignment recommendations

### 🎨 Interactive Whiteboard
- **System Design**: Collaborate on architecture diagrams
- **Real-time Cursors**: See where your teammates are working
- **Export Options**: Save your work as PNG/SVG

### ⚡ Performance
- **Virtual Scrolling**: Handle 500+ tasks smoothly
- **Optimistic Updates**: Instant UI feedback
- **Smart Caching**: Fast page loads

## 🏗️ Architecture

```
FlowBoard/
├── src/                          # Backend (.NET 8)
│   ├── FlowBoard.API             # Web API + SignalR
│   ├── FlowBoard.Core            # Domain models
│   ├── FlowBoard.Application     # Business logic (CQRS)
│   └── FlowBoard.Infrastructure  # Data access
│
└── flowboard-web/                # Frontend (Angular 17)
    ├── src/app/
    │   ├── core/                 # Singletons
    │   ├── shared/               # Reusable components
    │   └── features/             # Feature modules
    └── ...
```

## 🚀 Quick Start

### Prerequisites

#### macOS/Linux
```bash
# .NET 8 SDK
brew install dotnet-sdk

# Node.js 20+
brew install node

# Angular CLI
npm install -g @angular/cli

# Verify installations
dotnet --version  # Should be 8.x.x
node --version    # Should be 20.x.x
ng version
```

#### Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- Install Angular CLI: `npm install -g @angular/cli`

### VS Code Setup

**Required Extensions:**
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) - .NET development
- [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) - C# language support
- [Angular Language Service](https://marketplace.visualstudio.com/items?itemName=Angular.ng-template) - Angular support

**Recommended Extensions:**
- [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) - AI pair programmer
- [GitLens](https://marketplace.visualstudio.com/items?itemName=eamodio.gitlens) - Git supercharged
- [Thunder Client](https://marketplace.visualstudio.com/items?itemName=rangav.vscode-thunder-client) - API testing
- [Prettier](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode) - Code formatter

### Clone & Setup

```bash
# Clone repository
git clone https://github.com/PlonGuo/flowboard.git
cd flowboard

# Open in VS Code
code .
```

### Backend Setup

```bash
# Navigate to API project
cd src/FlowBoard.API

# Restore dependencies
dotnet restore

# Update database (when available)
dotnet ef database update

# Run API
dotnet run
# or use VS Code debugger (F5)
```

Backend will run on `https://localhost:5001`

**VS Code Debug:** Press `F5` or use the Run and Debug panel

### Frontend Setup

```bash
# Open new terminal in VS Code
# Navigate to frontend
cd flowboard-web

# Install dependencies
npm install

# Start dev server
ng serve
```

Frontend will run on `http://localhost:4200`

### Running Both (Recommended)

**Option 1: VS Code Integrated Terminal**
```bash
# Split terminal in VS Code (Cmd+Shift+5 on macOS)

# Terminal 1: Backend
cd src/FlowBoard.API
dotnet watch run

# Terminal 2: Frontend  
cd flowboard-web
ng serve
```

**Option 2: VS Code Tasks**
```bash
# Run both with one command (when configured)
Cmd+Shift+P → Tasks: Run Task → "Run Full Stack"
```

## 🔧 Configuration

### appsettings.json (Backend)

Create `src/FlowBoard.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlowBoard;Trusted_Connection=true"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### environment.ts (Frontend)

Update `flowboard-web/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  signalrUrl: 'https://localhost:5001/hubs'
};
```

## 🛠️ Tech Stack

### Backend
- **Framework**: ASP.NET Core 8
- **Real-time**: SignalR
- **Database**: Entity Framework Core + SQL Server
- **Architecture**: Clean Architecture + CQRS (MediatR)
- **AI**: Semantic Kernel + Azure OpenAI
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Caching**: IMemoryCache

### Frontend
- **Framework**: Angular 17 (Standalone Components)
- **UI Library**: Angular Material
- **Drag & Drop**: Angular CDK
- **State Management**: RxJS
- **Whiteboard**: Excalidraw
- **Real-time**: @microsoft/signalr

## 📁 Project Structure

### Backend (.NET)
```
src/
├── FlowBoard.API/              # Entry point, Controllers, Hubs
├── FlowBoard.Core/             # Domain entities, Interfaces
├── FlowBoard.Application/      # Commands, Queries, Handlers
├── FlowBoard.Infrastructure/   # Data access, Services
└── FlowBoard.Tests/            # Unit & Integration tests
```

### Frontend (Angular)
```
flowboard-web/
└── src/app/
    ├── core/                   # Services, Guards, Interceptors
    ├── shared/                 # Shared components, Pipes
    ├── features/               # Feature modules
    │   ├── boards/             # Kanban board
    │   ├── canvas/             # Whiteboard
    │   └── ai/                 # AI assistant
    └── layout/                 # App layout
```

## 🧪 Development

### Backend Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Watch mode (auto-reload)
dotnet watch run

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Frontend Commands
```bash
# Development server
ng serve

# Build for production
ng build --configuration production

# Run tests
ng test

# Run E2E tests
ng e2e

# Generate component
ng generate component features/boards/board-list
```

## 🐛 Debugging

### Backend (VS Code)
1. Set breakpoints in `.cs` files
2. Press `F5` or click Run → Start Debugging
3. Debugger will attach to the running process

### Frontend (Chrome DevTools)
1. Open Chrome DevTools (`F12`)
2. Sources tab → Set breakpoints in `.ts` files
3. Or use `debugger;` statement in code

### SignalR Debugging
```typescript
// Enable SignalR logging in Angular
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
  .withUrl('/hubs/board')
  .configureLogging(LogLevel.Debug)  // Enable debug logs
  .build();
```

## 🚀 Deployment

### Azure (Recommended for MVP)

**Prerequisites:**
- Azure Student account ($100 credit)
- Azure CLI installed

```bash
# Login to Azure
az login

# Create resources (when ready)
# Details in docs/DEPLOYMENT.md
```

**Estimated Costs:**
- App Service (B1): $13/month
- SQL Database (Basic): $5/month
- Azure OpenAI: $5-10/month
- **Total**: ~$25-30/month (4-5 months with $100 credit)

## 📚 Documentation

- [Architecture Overview](docs/ARCHITECTURE.md) _(coming soon)_
- [API Documentation](docs/API.md) _(coming soon)_
- [Contributing Guide](CONTRIBUTING.md) _(coming soon)_
- [Deployment Guide](docs/DEPLOYMENT.md) _(coming soon)_
- [Design Document](docs/DESIGN.md) - Complete project design

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) first.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 🎯 Roadmap

- [x] Project setup and architecture
- [ ] User authentication & authorization
- [ ] Real-time kanban board
- [ ] Task management (CRUD)
- [ ] AI assistant integration
- [ ] Interactive whiteboard
- [ ] Team collaboration features
- [ ] Performance optimization
- [ ] Azure deployment
- [ ] Mobile responsive design

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Jason Guo** - [@PlonGuo](https://github.com/PlonGuo)

## 🙏 Acknowledgments

- [Excalidraw](https://excalidraw.com/) - For the amazing whiteboard component
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) - For real-time capabilities
- [Angular Material](https://material.angular.io/) - For beautiful UI components
- [Semantic Kernel](https://github.com/microsoft/semantic-kernel) - For AI integration

---

⭐ **Star this repo if you find it helpful!**

💡 **Questions?** Open an [issue](https://github.com/PlonGuo/flowboard/issues)

🚀 **Ready to contribute?** Check out [good first issues](https://github.com/PlonGuo/flowboard/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22)
