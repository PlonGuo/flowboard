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
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server Express LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended for backend)
- [VS Code](https://code.visualstudio.com/) (recommended for frontend)

### Backend Setup

```bash
# Clone repository
git clone https://github.com/PlonGuo/flowboard.git
cd flowboard

# Open in Visual Studio
# File → Open → Project/Solution → src/FlowBoard.sln

# Or use CLI
cd src/FlowBoard.API
dotnet restore
dotnet ef database update
dotnet run
```

Backend will run on `https://localhost:5001`

### Frontend Setup

```bash
# Navigate to frontend
cd flowboard-web

# Install dependencies
npm install

# Start dev server
ng serve
```

Frontend will run on `http://localhost:4200`

## 🔧 Configuration

### appsettings.json (Backend)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlowBoard;Trusted_Connection=true"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4"
  }
}
```

### environment.ts (Frontend)
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

### Frontend
- **Framework**: Angular 17 (Standalone Components)
- **UI Library**: Angular Material
- **State Management**: RxJS
- **Whiteboard**: Excalidraw
- **Real-time**: @microsoft/signalr

## 📚 Documentation

- [Architecture Overview](docs/ARCHITECTURE.md) _(coming soon)_
- [API Documentation](docs/API.md) _(coming soon)_
- [Contributing Guide](CONTRIBUTING.md) _(coming soon)_
- [Deployment Guide](docs/DEPLOYMENT.md) _(coming soon)_

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) first.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Jason Guo** - [@PlonGuo](https://github.com/PlonGuo)

## 🙏 Acknowledgments

- [Excalidraw](https://excalidraw.com/) - For the amazing whiteboard component
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) - For real-time capabilities
- [Angular Material](https://material.angular.io/) - For beautiful UI components

---

⭐ Star this repo if you find it helpful!