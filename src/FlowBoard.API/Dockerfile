# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY FlowBoard.Core/FlowBoard.Core.csproj FlowBoard.Core/
COPY FlowBoard.Application/FlowBoard.Application.csproj FlowBoard.Application/
COPY FlowBoard.Infrastructure/FlowBoard.Infrastructure.csproj FlowBoard.Infrastructure/
COPY FlowBoard.API/FlowBoard.API.csproj FlowBoard.API/
RUN dotnet restore FlowBoard.API/FlowBoard.API.csproj

# Copy all source code and build
COPY . .
WORKDIR /src/FlowBoard.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

USER appuser
ENTRYPOINT ["dotnet", "FlowBoard.API.dll"]
