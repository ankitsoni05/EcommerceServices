# ?? Docker Setup - Quick Start

## ?? Get Running in 3 Minutes

### Prerequisites
- Docker Desktop installed ([Download](https://www.docker.com/products/docker-desktop))
- No PostgreSQL, MongoDB, or Redis installation needed!

### Step 1: Setup Environment
```bash
# Copy environment template
cp .env.example .env

# (Optional) Edit .env if you want to change passwords
nano .env
```

### Step 2: Start Everything
```bash
# Start all services
docker-compose up -d

# Wait 30 seconds for services to initialize

# Check status
docker-compose ps
```

### Step 3: Verify It's Working
```bash
# Test Catalog API
curl http://localhost:5001/api/catalog

# Or open in browser:
# http://localhost:5001/api/catalog
```

**? That's it! All services are running!**

---

## ?? What's Running?

| Service | URL | Purpose |
|---------|-----|---------|
| **Catalog API** | http://localhost:5001/api/catalog | Product catalog service |
| **PostgreSQL** | localhost:5432 | Database for Catalog & Orders |
| **MongoDB** | localhost:27017 | Document storage |
| **Redis** | localhost:6379 | Caching & sessions |
| **pgAdmin** | http://localhost:5050 | PostgreSQL management UI |
| **Mongo Express** | http://localhost:8081 | MongoDB management UI |
| **Redis Commander** | http://localhost:8082 | Redis management UI |

---

## ?? Common Tasks

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f catalog-service
docker-compose logs -f postgres
```

### Stop Services
```bash
# Stop (keeps data)
docker-compose stop

# Stop and remove (DELETES DATA!)
docker-compose down -v
```

### Restart After Code Changes
```bash
# Rebuild and restart
docker-compose up --build -d
```

### Access Database
```bash
# PostgreSQL CLI
docker-compose exec postgres psql -U postgres -d CatalogDb

# Or use pgAdmin UI
# http://localhost:5050
# Email: admin@ecommerce.com
# Password: admin
```

---

## ??? Architecture

```
???????????????????????????????????????????
?         Your Machine (Host)             ?
?                                         ?
?  Browser ? http://localhost:5001       ?
?                                         ?
???????????????????????????????????????????
                ?
???????????????????????????????????????????
?     Docker Network (Internal)           ?
?                                         ?
?  ????????????????                      ?
?  ? Catalog API  ? ? Your .NET app      ?
?  ? :5001        ?                      ?
?  ????????????????                      ?
?          ?                              ?
?  ????????????????  ????????????????   ?
?  ?  PostgreSQL  ?  ?    Redis     ?   ?
?  ?  :5432       ?  ?    :6379     ?   ?
?  ?              ?  ?              ?   ?
?  ?  CatalogDb   ?  ?  Cache       ?   ?
?  ????????????????  ????????????????   ?
???????????????????????????????????????????
```

---

## ?? Configuration

### Environment Variables (.env)

```bash
# PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres_dev_password

# MongoDB
MONGO_ROOT_USER=admin
MONGO_ROOT_PASSWORD=mongo_dev_password

# Application
ASPNETCORE_ENVIRONMENT=Development
```

**Important:** 
- Never commit `.env` to Git
- Use `.env.example` as a template
- Change passwords for production

---

## ?? More Information

- **Full Docker Guide:** [DOCKER_COMPOSE_GUIDE.md](DOCKER_COMPOSE_GUIDE.md)
- **API Documentation:** [README.md](CatalogService/README.md)
- **Quick Start:** [QUICKSTART.md](CatalogService/QUICKSTART.md)

---

## ?? Troubleshooting

### Port Already in Use
```bash
# Change port in .env
POSTGRES_PORT=5433
CATALOG_SERVICE_PORT=5011
```

### Can't Connect to Database
```bash
# Check if healthy
docker-compose ps

# Restart services
docker-compose restart postgres
docker-compose restart catalog-service
```

### Reset Everything
```bash
# Nuclear option (deletes all data!)
docker-compose down -v
docker system prune -a
docker-compose up -d
```

---

## ? Success Checklist

- [ ] Docker Desktop running
- [ ] `.env` file created
- [ ] `docker-compose up -d` executed
- [ ] All services show "healthy" in `docker-compose ps`
- [ ] Can access http://localhost:5001/api/catalog
- [ ] Can access pgAdmin at http://localhost:5050

**All checked? You're ready to develop! ??**

---

*Need help? Check [DOCKER_COMPOSE_GUIDE.md](DOCKER_COMPOSE_GUIDE.md) for detailed documentation.*
