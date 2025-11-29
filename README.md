# E-Commerce Microservices - Clean Architecture

This repository contains a complete e-commerce microservices system built with **Clean Architecture**, **.NET 10**, **EF Core**, **PostgreSQL**, and **Docker Compose**.

---

## ?? Complete Documentation

**All documentation has been organized in the [`docs/`](docs/) folder.**

### ?? [**Start Here: Documentation Index**](docs/INDEX.md)

---

## ?? Quick Start with Docker (Recommended)

### Prerequisites
- Docker Desktop ([Download](https://www.docker.com/products/docker-desktop))
- That's it! No .NET, PostgreSQL, or Redis installation needed.

### Start Everything in 3 Steps

```bash
# 1. Copy environment template
cp .env.example .env

# 2. Start all services
docker-compose up -d

# 3. Test the API
curl http://localhost:5001/api/catalog
```

**? Done! All services are running in Docker.**

**?? Detailed guide:** [docs/docker/DOCKER_QUICK_START.md](docs/docker/DOCKER_QUICK_START.md)

---

## ?? Documentation Structure

```
docs/
??? INDEX.md                              ? Main documentation index
?
??? docker/                               ? Docker & Deployment
?   ??? DOCKER_QUICK_START.md            (Get running in 3 min)
?   ??? DOCKER_COMPOSE_GUIDE.md          (Complete reference)
?   ??? DOCKER_ARCHITECTURE_DIAGRAMS.md  (Visual diagrams)
?   ??? DOCKER_SETUP_SUMMARY.md          (Implementation recap)
?
??? architecture/                         ? Clean Architecture
?   ??? CLEAN_ARCHITECTURE_GUIDE.md      (Theory & principles)
?   ??? ARCHITECTURE_DIAGRAMS.md         (Visual flows)
?   ??? PRACTICAL_EXAMPLES.md            (Real-world examples)
?
??? guides/                               ? Implementation Guides
?   ??? IMPLEMENTATION_GUIDE.md          (EF Core + PostgreSQL)
?   ??? MIGRATIONS_GUIDE.md              (Database versioning)
?   ??? QUICKSTART.md                    (Local development)
?
??? SUMMARY.md                            ? Project summary
```

**?? [Browse all documentation ?](docs/INDEX.md)**

---

## ?? Learning Paths

### ?? For Beginners (Docker Approach)
1. [Docker Quick Start](docs/docker/DOCKER_QUICK_START.md)
2. [Docker Compose Guide](docs/docker/DOCKER_COMPOSE_GUIDE.md)
3. [Clean Architecture Guide](docs/architecture/CLEAN_ARCHITECTURE_GUIDE.md)
4. [Architecture Diagrams](docs/architecture/ARCHITECTURE_DIAGRAMS.md)

### ?? For Developers (Deep Dive)
1. [Clean Architecture Guide](docs/architecture/CLEAN_ARCHITECTURE_GUIDE.md)
2. [Practical Examples](docs/architecture/PRACTICAL_EXAMPLES.md)
3. [Implementation Guide](docs/guides/IMPLEMENTATION_GUIDE.md)
4. [Migrations Guide](docs/guides/MIGRATIONS_GUIDE.md)

### ?? For DevOps (Deployment)
1. [Docker Compose Guide](docs/docker/DOCKER_COMPOSE_GUIDE.md)
2. [Docker Architecture](docs/docker/DOCKER_ARCHITECTURE_DIAGRAMS.md)
3. [Docker Setup Summary](docs/docker/DOCKER_SETUP_SUMMARY.md)

---

## ??? Architecture Overview

### Microservices Structure

```
???????????????????????????????????????????????????????????
?               Docker Compose Orchestration              ?
?                                                         ?
?  ????????????????  ????????????????  ???????????????? ?
?  ?   Catalog    ?  ?    Order     ?  ?   Basket     ? ?
?  ?   Service    ?  ?   Service    ?  ?   Service    ? ?
?  ?   :5001      ?  ?   :5002      ?  ?   :5003      ? ?
?  ????????????????  ????????????????  ???????????????? ?
?          ?                 ?                 ?         ?
?          ?????????????????????????????????????         ?
?                           ?                            ?
?         ?????????????????????????????????????          ?
?         ?                                   ?          ?
?  ???????????????  ???????????????  ????????????????  ?
?  ?  PostgreSQL ?  ?   MongoDB   ?  ?    Redis     ?  ?
?  ?    :5432    ?  ?   :27017    ?  ?    :6379     ?  ?
?  ???????????????  ???????????????  ????????????????  ?
???????????????????????????????????????????????????????????
```

**?? [View detailed architecture diagrams ?](docs/docker/DOCKER_ARCHITECTURE_DIAGRAMS.md)**

---

## ?? What's Included

### ? Microservices
- **Catalog Service** - Product catalog management (? Implemented)
- **Order Service** - Order processing (?? Coming soon)
- **Basket Service** - Shopping cart (?? Coming soon)

### ? Infrastructure
- **PostgreSQL** - Relational database
- **MongoDB** - Document storage
- **Redis** - Caching & sessions

### ? Development Tools
- **pgAdmin** - PostgreSQL management UI
- **Mongo Express** - MongoDB UI
- **Redis Commander** - Redis UI

### ? Documentation
- **6,000+ lines** of comprehensive guides
- Organized into categories
- Cross-referenced and searchable

---

## ?? Technology Stack

- **.NET 10** - Latest .NET
- **ASP.NET Core** - Web framework
- **Entity Framework Core 10** - ORM
- **PostgreSQL 16** - Database
- **MongoDB 7** - Document store
- **Redis 7** - Cache
- **Docker & Docker Compose** - Containerization
- **Clean Architecture** - Design pattern

---

## ?? Service URLs (Docker)

| Service | URL | Credentials |
|---------|-----|-------------|
| **Catalog API** | http://localhost:5001/api/catalog | N/A |
| **PostgreSQL** | localhost:5432 | postgres / postgres_dev_password |
| **MongoDB** | localhost:27017 | admin / mongo_dev_password |
| **Redis** | localhost:6379 | N/A |
| **pgAdmin** | http://localhost:5050 | admin@ecommerce.com / admin |
| **Mongo Express** | http://localhost:8081 | admin / mongo_dev_password |
| **Redis Commander** | http://localhost:8082 | N/A |

---

## ?? Common Commands

### Docker Commands

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f catalog-service

# Stop services
docker-compose down

# Restart after code changes
docker-compose up --build -d
```

**?? [View all commands ?](docs/docker/DOCKER_COMPOSE_GUIDE.md#common-commands)**

---

## ?? Testing

### Test API with Docker

```bash
# Get all catalog items
curl http://localhost:5001/api/catalog

# Get specific item
curl http://localhost:5001/api/catalog/1
```

**?? [View testing guide ?](docs/docker/DOCKER_QUICK_START.md#verify-its-working)**

---

## ?? Project Structure

```
EcommerceServices/
?
??? ?? docs/                            ? All documentation
?   ??? INDEX.md                        ? Documentation index
?   ??? docker/                         ? Docker guides
?   ??? architecture/                   ? Architecture docs
?   ??? guides/                         ? Implementation guides
?
??? ?? Docker Configuration
?   ??? docker-compose.yml
?   ??? docker-compose.override.yml
?   ??? docker-compose.prod.yml
?
??? ?? CatalogService/                  ? Catalog microservice
?   ??? Domain/
?   ??? Application/
?   ??? Infrastructure/
?   ??? Controllers/
?
??? ?? databases/                       ? Database init scripts
    ??? postgres/
        ??? init-scripts/
```

---

## ?? Key Features

? **Clean Architecture** - Separation of concerns
? **Repository Pattern** - Data access abstraction
? **EF Core + PostgreSQL** - Production-ready ORM
? **Docker Compose** - Easy multi-service setup
? **Microservices Ready** - Scalable architecture
? **Comprehensive Documentation** - 6,000+ lines organized
? **Development Tools** - pgAdmin, Mongo Express, Redis Commander
? **Hot Reload** - Fast development cycle

---

## ?? Learning Outcomes

After exploring this project, you'll understand:

1. How to structure microservices with Clean Architecture
2. How to use Docker Compose for multi-service orchestration
3. How to implement Repository Pattern with EF Core
4. How to set up PostgreSQL with Docker
5. How dependencies flow in Clean Architecture
6. How to write comprehensive technical documentation

**?? [Start learning ?](docs/INDEX.md)**

---

## ?? Contributing

This is a learning project demonstrating best practices for:
- Clean Architecture in .NET
- Microservices with Docker
- EF Core with PostgreSQL
- Repository Pattern
- Technical Documentation

---

## ?? License

This project is for educational purposes.

---

**?? [View Complete Documentation ?](docs/INDEX.md)**

**?? [Quick Start Guide ?](docs/docker/DOCKER_QUICK_START.md)**
