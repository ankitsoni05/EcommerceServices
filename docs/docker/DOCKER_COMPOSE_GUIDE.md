# Docker Compose Guide for E-Commerce Microservices

## ?? Table of Contents
1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Architecture](#architecture)
4. [Configuration](#configuration)
5. [Common Commands](#common-commands)
6. [Development Workflow](#development-workflow)
7. [Production Deployment](#production-deployment)
8. [Troubleshooting](#troubleshooting)

---

## Overview

### What is Docker Compose?

Docker Compose is a tool for defining and running multi-container Docker applications. For our e-commerce microservices, it manages:

- **Multiple API services** (Catalog, Order, Basket, etc.)
- **Shared databases** (PostgreSQL, MongoDB, Redis)
- **Service networking** (Inter-service communication)
- **Development tools** (pgAdmin, Mongo Express, Redis Commander)

### Why Docker Compose?

? **Single Command Setup** - Start entire system with one command
? **Service Dependencies** - Automatic startup order and health checks
? **Shared Network** - Services communicate easily
? **Environment Management** - Separate dev/prod configurations
? **Volume Management** - Persistent data storage

---

## Quick Start

### 1. Prerequisites

```bash
# Install Docker Desktop (includes Docker Compose)
# Windows/Mac: https://www.docker.com/products/docker-desktop
# Linux: https://docs.docker.com/compose/install/

# Verify installation
docker --version
docker-compose --version
```

### 2. Setup Environment Variables

```bash
# Copy example environment file
cp .env.example .env

# Edit .env with your settings (optional)
nano .env
```

### 3. Start All Services

```bash
# Start in foreground (see logs)
docker-compose up

# Start in background (detached mode)
docker-compose up -d

# Start specific service
docker-compose up catalog-service
```

### 4. Verify Services

```bash
# Check running containers
docker-compose ps

# Check logs
docker-compose logs -f catalog-service
docker-compose logs -f postgres

# Check health
docker-compose ps
```

### 5. Access Services

| Service | URL | Credentials |
|---------|-----|-------------|
| **Catalog API** | http://localhost:5001/api/catalog | N/A |
| **PostgreSQL** | localhost:5432 | postgres / postgres_dev_password |
| **MongoDB** | localhost:27017 | admin / mongo_dev_password |
| **Redis** | localhost:6379 | N/A |
| **pgAdmin** | http://localhost:5050 | admin@ecommerce.com / admin |
| **Mongo Express** | http://localhost:8081 | admin / mongo_dev_password |
| **Redis Commander** | http://localhost:8082 | N/A |

### 6. Stop Services

```bash
# Stop all services (keeps containers)
docker-compose stop

# Stop and remove containers
docker-compose down

# Stop and remove containers + volumes (DELETES DATA!)
docker-compose down -v
```

---

## Architecture

### Service Structure

```
????????????????????????????????????????????????????????????
?                    Docker Network                         ?
?                  (ecommerce-network)                      ?
?                                                           ?
?  ???????????????  ???????????????  ???????????????     ?
?  ?   Catalog   ?  ?    Order    ?  ?   Basket    ?     ?
?  ?   Service   ?  ?   Service   ?  ?   Service   ?     ?
?  ?   :5001     ?  ?   :5002     ?  ?   :5003     ?     ?
?  ???????????????  ???????????????  ???????????????     ?
?         ?                 ?                 ?            ?
?         ?????????????????????????????????????            ?
?                           ?                              ?
?         ?????????????????????????????????????            ?
?         ?                                   ?            ?
?  ???????????????  ???????????????  ????????????????   ?
?  ?  PostgreSQL ?  ?   MongoDB   ?  ?    Redis     ?   ?
?  ?    :5432    ?  ?   :27017    ?  ?    :6379     ?   ?
?  ?             ?  ?             ?  ?              ?   ?
?  ? CatalogDb   ?  ? ReviewsDb   ?  ? Cache/       ?   ?
?  ? OrderDb     ?  ?             ?  ? Sessions     ?   ?
?  ???????????????  ???????????????  ????????????????   ?
?                                                          ?
????????????????????????????????????????????????????????????
```

### File Structure

```
EcommerceServices/
?
??? docker-compose.yml              # Main orchestration
??? docker-compose.override.yml     # Development overrides
??? docker-compose.prod.yml         # Production config
??? .env.example                    # Environment template
??? .env                            # Local environment (not in git)
?
??? CatalogService/
?   ??? Dockerfile
?   ??? ...
?
??? OrderService/
?   ??? Dockerfile
?   ??? ...
?
??? databases/
    ??? postgres/
        ??? init-scripts/
            ??? 00-create-multiple-databases.sh
            ??? 01-catalog-db.sql
            ??? 02-order-db.sql
```

---

## Configuration

### docker-compose.yml (Main Configuration)

Defines all services, networks, and volumes.

**Key sections:**
- `networks` - Service communication
- `volumes` - Persistent data storage
- `services` - Container definitions

### docker-compose.override.yml (Development)

Automatically loaded in development. Adds:
- Volume mounts for hot reload
- Debug ports
- Development tools enabled

**Usage:** Automatically used with `docker-compose up`

### docker-compose.prod.yml (Production)

Production-specific settings:
- Optimized builds
- Resource limits
- No development tools
- Security hardening

**Usage:**
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### .env (Environment Variables)

**Security best practices:**
- ? Copy `.env.example` to `.env`
- ? Modify values for your environment
- ? Add `.env` to `.gitignore`
- ? Never commit `.env` to source control
- ? Use secrets management in production

**Example .env:**
```bash
# Application
ASPNETCORE_ENVIRONMENT=Development

# PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password

# MongoDB
MONGO_ROOT_USER=admin
MONGO_ROOT_PASSWORD=your_secure_password
```

---

## Common Commands

### Starting Services

```bash
# Start all services
docker-compose up

# Start in background
docker-compose up -d

# Start specific service
docker-compose up catalog-service postgres redis

# Rebuild and start (after code changes)
docker-compose up --build

# Force recreate containers
docker-compose up --force-recreate
```

### Stopping Services

```bash
# Stop all services (keeps containers)
docker-compose stop

# Stop specific service
docker-compose stop catalog-service

# Stop and remove containers
docker-compose down

# Stop and remove containers + volumes (DELETES DATA!)
docker-compose down -v

# Stop and remove containers + images
docker-compose down --rmi all
```

### Managing Containers

```bash
# List containers
docker-compose ps

# View logs
docker-compose logs
docker-compose logs -f                    # Follow logs
docker-compose logs -f catalog-service    # Follow specific service
docker-compose logs --tail=100            # Last 100 lines

# Execute commands in container
docker-compose exec catalog-service bash
docker-compose exec postgres psql -U postgres

# Restart service
docker-compose restart catalog-service

# Scale service (multiple instances)
docker-compose up -d --scale catalog-service=3
```

### Database Operations

```bash
# Access PostgreSQL
docker-compose exec postgres psql -U postgres

# List databases
docker-compose exec postgres psql -U postgres -c "\l"

# Access specific database
docker-compose exec postgres psql -U postgres -d CatalogDb

# Run SQL script
docker-compose exec -T postgres psql -U postgres -d CatalogDb < script.sql

# Backup database
docker-compose exec postgres pg_dump -U postgres CatalogDb > backup.sql

# Restore database
docker-compose exec -T postgres psql -U postgres -d CatalogDb < backup.sql
```

### Build & Deploy

```bash
# Build images
docker-compose build

# Build without cache
docker-compose build --no-cache

# Build specific service
docker-compose build catalog-service

# Pull latest images
docker-compose pull

# Push images to registry
docker-compose push
```

---

## Development Workflow

### Day-to-Day Development

#### 1. Start Development Environment

```bash
# Start all services with hot reload
docker-compose up

# Or in background
docker-compose up -d

# Check everything is running
docker-compose ps
```

#### 2. Make Code Changes

Code changes are automatically detected (hot reload enabled in development).

```bash
# Watch logs to see reload
docker-compose logs -f catalog-service
```

#### 3. Apply Database Migrations

```bash
# Option A: Inside container
docker-compose exec catalog-service dotnet ef database update

# Option B: From host machine
cd CatalogService
dotnet ef database update
```

#### 4. Debug a Service

```bash
# View service logs
docker-compose logs -f catalog-service

# Access container shell
docker-compose exec catalog-service bash

# Check environment variables
docker-compose exec catalog-service env

# Test database connection
docker-compose exec catalog-service curl http://postgres:5432
```

#### 5. Reset Environment

```bash
# Reset everything (CAREFUL: deletes data!)
docker-compose down -v
docker-compose up -d
```

### Adding a New Service

#### Step 1: Create Service Directory

```bash
mkdir OrderService
cd OrderService
dotnet new webapi -n OrderService
```

#### Step 2: Create Dockerfile

```dockerfile
# OrderService/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OrderService.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]
```

#### Step 3: Add to docker-compose.yml

```yaml
  order-service:
    image: order-service:latest
    container_name: order-service
    build:
      context: ./OrderService
      dockerfile: Dockerfile
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Development}
      ConnectionStrings__OrderDb: "Host=postgres;Port=5432;Database=OrderDb;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
    ports:
      - "5002:80"
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - ecommerce-network
```

#### Step 4: Start New Service

```bash
# Build and start
docker-compose up --build order-service

# Or rebuild all
docker-compose up --build
```

---

## Production Deployment

### Preparation

#### 1. Create Production Environment File

```bash
# .env.prod
ASPNETCORE_ENVIRONMENT=Production
POSTGRES_PASSWORD=very_secure_password_here
MONGO_ROOT_PASSWORD=another_secure_password
REDIS_PASSWORD=redis_secure_password
```

#### 2. Build Production Images

```bash
# Build with production config
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build

# Tag images for registry
docker tag catalog-service:latest myregistry.azurecr.io/catalog-service:1.0.0
```

#### 3. Push to Registry

```bash
# Login to registry
docker login myregistry.azurecr.io

# Push images
docker-compose -f docker-compose.yml -f docker-compose.prod.yml push
```

### Deployment

#### Option 1: Docker Swarm

```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml -c docker-compose.prod.yml ecommerce

# Check services
docker stack services ecommerce

# Check logs
docker service logs ecommerce_catalog-service
```

#### Option 2: Kubernetes (using Kompose)

```bash
# Convert Docker Compose to Kubernetes manifests
kompose convert -f docker-compose.yml -f docker-compose.prod.yml

# Apply to Kubernetes
kubectl apply -f .

# Check pods
kubectl get pods
```

#### Option 3: Azure Container Instances

```bash
# Create context
docker context create aci myaci

# Use context
docker context use myaci

# Deploy
docker compose up
```

### Production Monitoring

```bash
# Check service status
docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps

# View logs
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f

# Check resource usage
docker stats

# Export metrics (Prometheus format)
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}"
```

---

## Troubleshooting

### Common Issues

#### Issue 1: "Port already in use"

**Problem:** Another service is using the required port.

**Solution:**
```bash
# Find what's using the port (Windows)
netstat -ano | findstr :5432

# Find what's using the port (Linux/Mac)
lsof -i :5432

# Stop the conflicting service or change port in .env
POSTGRES_PORT=5433
```

#### Issue 2: "Cannot connect to database"

**Problem:** Service started before database was ready.

**Solution:**
```bash
# Check health status
docker-compose ps

# Wait for healthy status
docker-compose up -d postgres
docker-compose exec postgres pg_isready

# Then start service
docker-compose up catalog-service
```

#### Issue 3: "Build failed"

**Problem:** Docker build context or dependencies issue.

**Solution:**
```bash
# Clear build cache
docker-compose build --no-cache

# Remove old images
docker image prune -a

# Check Dockerfile path
docker-compose config
```

#### Issue 4: "Volume permission denied"

**Problem:** Linux file permissions.

**Solution:**
```bash
# Fix ownership
sudo chown -R $USER:$USER ./volumes

# Or run as root (not recommended)
docker-compose up --user root
```

#### Issue 5: "Service unhealthy"

**Problem:** Health check failing.

**Solution:**
```bash
# Check logs
docker-compose logs catalog-service

# Check health check
docker inspect catalog-service | grep -A 10 Health

# Disable health check temporarily
# Remove healthcheck from docker-compose.yml
```

### Debugging Commands

```bash
# Check configuration
docker-compose config

# Validate compose file
docker-compose config --quiet

# Check container logs
docker-compose logs --tail=50 catalog-service

# Access container shell
docker-compose exec catalog-service bash

# Check environment variables
docker-compose exec catalog-service env | grep CONNECTION

# Test network connectivity
docker-compose exec catalog-service ping postgres
docker-compose exec catalog-service curl http://postgres:5432

# Check disk usage
docker system df

# Clean up everything
docker system prune -a --volumes
```

### Performance Issues

```bash
# Check resource usage
docker stats

# Limit resources (in docker-compose.yml)
services:
  catalog-service:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M

# Optimize images
docker-compose build --compress
```

---

## Best Practices

### Development

? **DO:**
- Use `.env` for local configuration
- Mount volumes for hot reload
- Enable development tools (pgAdmin, etc.)
- Use health checks
- Follow logs during development

? **DON'T:**
- Commit `.env` file
- Run production config in development
- Skip health checks
- Use `latest` tag in production
- Expose unnecessary ports

### Production

? **DO:**
- Use specific image tags (not `latest`)
- Set resource limits
- Enable health checks
- Use secrets management
- Monitor resource usage
- Regular backups
- Use read-only file systems where possible

? **DON'T:**
- Use development tools in production
- Expose database ports publicly
- Store secrets in docker-compose.yml
- Run as root user
- Skip health checks

---

## Summary

### Key Commands Quick Reference

| Task | Command |
|------|---------|
| Start all services | `docker-compose up -d` |
| Stop all services | `docker-compose down` |
| View logs | `docker-compose logs -f` |
| Rebuild services | `docker-compose up --build` |
| Check status | `docker-compose ps` |
| Access container | `docker-compose exec <service> bash` |
| Scale service | `docker-compose up -d --scale <service>=3` |

### Service URLs (Development)

| Service | URL |
|---------|-----|
| Catalog API | http://localhost:5001/api/catalog |
| PostgreSQL | localhost:5432 |
| MongoDB | localhost:27017 |
| Redis | localhost:6379 |
| pgAdmin | http://localhost:5050 |

---

*Docker Compose setup complete! Ready for microservices development!* ??
