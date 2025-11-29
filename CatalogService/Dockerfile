# =====================================================
# Multi-Stage Dockerfile for Catalog Service
# =====================================================
# 
# Stages:
# 1. base        - Runtime base image
# 2. build       - Build the application
# 3. publish     - Publish optimized output
# 4. development - Development with hot reload
# 5. final       - Production-ready image
# =====================================================

# ===================================================
# Stage 1: Base Runtime Image
# ===================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for health checks
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID

# ===================================================
# Stage 2: Build
# ===================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies (cached layer)
COPY ["CatalogService.csproj", "CatalogService/"]
RUN dotnet restore "CatalogService/CatalogService.csproj"

# Copy everything else and build
COPY . CatalogService/
WORKDIR "/src/CatalogService"
RUN dotnet build "CatalogService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===================================================
# Stage 3: Publish
# ===================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CatalogService.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ===================================================
# Stage 4: Development (Hot Reload)
# ===================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /app

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy project files
COPY . .

# Entry point for development
ENTRYPOINT ["dotnet", "watch", "run", "--no-launch-profile"]

# ===================================================
# Stage 5: Final Production Image
# ===================================================
FROM base AS final
WORKDIR /app

# Copy published output
COPY --from=publish /app/publish .

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "CatalogService.dll"]