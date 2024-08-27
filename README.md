# TMS Project

Этот проект состоит из нескольких модулей, которые в совокупности обеспечивают комплексное применение. Каждый модуль имеет свою собственную функциональность и ответственность в системе.

## Modules Overview

- **TMS.Application.Consul**: Handles Consul configuration and service registration.
- **TMS.Application.Security**: Manages user authentication, registration, and security-related functionalities.
- **TMS.Application.UseCases.DI**: Contains Dependency Injection (DI) setup for use cases.
- **TMS.Application.UseCases**: Defines the use cases and business logic of the application.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (for running services in containers)
- [Consul](https://www.consul.io/downloads) (for service discovery, optional)

## Configuration
### Example Docker Compose Configuration
Below is the docker-compose.yml file for setting up the services:
  ```yaml
version: '3.8'

services:
  postgres:
    container_name: Postgres
    image: postgres:latest
    environment:
      POSTGRES_DB: "db"
      POSTGRES_USER: "postgres"
      POSTGRES_PASSWORD: "291203"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d db"]
      interval: 10s
      retries: 5
      start_period: 30s
      timeout: 10s
    ports:
      - "5432:5432"
    restart: always
  
  consul:
    image: hashicorp/consul:latest
    container_name: Consul
    ports:
      - '8500:8500'
    restart: always

  baget:
    image: loicsharma/baget:latest
    container_name: Baget
    ports:
      - "5000:80"
    volumes:
      - ./data:/var/baget
    restart: always

  notes.service:
    image: notes_service:latest
    container_name: Notes.Service
    build:
      context: TMS.Notes
      dockerfile: Dockerfile
    depends_on:
      - postgres
    environment:
      - ASPNETCORE_ENVIRONMENT=Release
      - ConnectionStrings__DatabaseConnection=Server=postgres;Port=5432;Database=db;User Id=postgres;Password=admin;
    ports:
      - "8081:8081"
    restart: always

  security.service:
    image: security_service:latest
    container_name: Security.Service
    build:
      context: TMS.Security
      dockerfile: Dockerfile
    depends_on:
      postgres:
        condition: service_healthy
        restart: true
    environment:
      - ASPNETCORE_ENVIRONMENT=Release
      - ConnectionStrings__DatabaseConnection=Server=postgres;Port=5432;Database=db;User Id=postgres;Password=admin;
    ports:
      - "8082:8082"
    restart: always

volumes:
  postgres-data:
