# Hospital Management System - Architecture Overview

## System Overview

The Hospital Management System (HIS) is a microservices-based platform built on .NET 9 and Clean Architecture principles. It provides comprehensive hospital operations management including patient registration, outpatient visits, inpatient admissions, pharmacy dispensing, health examinations, and a unified service catalog. Each bounded context is deployed as an independent service with its own database, communicating asynchronously via RabbitMQ and exposed through a centralized API Gateway using YARP reverse proxy.

## Services

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| API Gateway | 5100 | — | YARP-based reverse proxy; routes, aggregates, and secures all client requests |
| Identity Service | 5101 | hospitalms_identity | Authentication, authorization, user & role management, JWT token issuance |
| Patient Service | 5102 | hospitalms_patient | Patient demographics, registration, medical record number generation |
| Catalog Service | 5103 | hospitalms_catalog | Shared reference data: departments, doctors, diagnosis codes, fee items |
| Outpatient Service | 5104 | hospitalms_outpatient | Outpatient visits, triage, doctor consultations, prescriptions, billing |
| Inpatient Service | 5105 | hospitalms_inpatient | Admissions, bed management, ward rounds, discharge summaries |
| Pharmacy Service | 5106 | hospitalms_pharmacy | Drug inventory, prescription dispensing, stock management |
| Health Exam Service | 5107 | hospitalms_healthexam | Health examination packages, scheduling, results, and reporting |

## Infrastructure

| Component | Port(s) | Purpose |
|-----------|---------|---------|
| PostgreSQL | 5432 | Primary relational database (one logical DB per service) |
| RabbitMQ | 5672 / 15672 | Asynchronous messaging bus / Management UI |
| Redis | 6379 | Distributed caching and session store |
| Seq | 5341 / 8081 | Structured log ingestion / Log dashboard UI |
| pgAdmin | 5050 | Database administration UI |

## Clean Architecture

Each microservice follows a four-layer Clean Architecture:

```
┌─────────────────────────────────────────────┐
│                  API Layer                  │
│         (Controllers, Middleware)           │
├─────────────────────────────────────────────┤
│             Application Layer              │
│    (Commands, Queries, Validators, DTOs)    │
├─────────────────────────────────────────────┤
│               Domain Layer                 │
│     (Entities, Value Objects, Events,       │
│      Interfaces, Domain Services)           │
├─────────────────────────────────────────────┤
│           Infrastructure Layer             │
│   (EF Core, Repositories, Messaging,       │
│    External Services, Caching)              │
└─────────────────────────────────────────────┘
```

**Dependency Rule:** Dependencies point inward. The Domain layer has zero external dependencies. The Application layer depends only on the Domain. Infrastructure and API layers depend on inner layers but never the reverse.

## Tech Stack

| Category | Technology |
|----------|-----------|
| Runtime | .NET 9 / C# 12 |
| API Style | REST + MediatR (CQRS) |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL 16 |
| Messaging | RabbitMQ via MassTransit |
| Caching | Redis via StackExchange.Redis |
| Auth | JWT Bearer tokens |
| Logging | Serilog → Seq |
| Validation | FluentValidation |
| Mapping | Mapster |
| API Docs | Swagger / Swashbuckle |
| Gateway | YARP Reverse Proxy |
| Testing | xUnit, Moq, FluentAssertions |
| Frontend | Vue 3 + TypeScript (planned) |
| Containers | Docker & Docker Compose |
