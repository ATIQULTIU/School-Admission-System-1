# 🎓 School Admission System-1

A modern, full-stack, Dockerized School Admission Management System designed to simplify and streamline the student admission process.

Built with **.NET 8, PostgreSQL, React 18, and Vite**, this application provides a complete admission workflow featuring student identification, class and shift selection, temporary seat reservations, simulated payment confirmation, application tracking, and administrative management.

**Developer:** MD. Atiqul Islam (Atik)  
**Email:** atik.cmttiu1001@gmail.com

---

## 📌 Table of Contents

- [Project Overview](#-project-overview)
- [Technology Stack](#-technology-stack)
- [Key Features](#-key-features)
- [Application Workflow](#-application-workflow)
- [Project Architecture](#-project-architecture)
- [Getting Started](#-getting-started)
- [Application Access](#-application-access)
- [REST API Documentation](#-rest-api-documentation)
- [Environment Configuration](#-environment-configuration)
- [Automated Testing](#-automated-testing)
- [Docker Commands](#-docker-commands)
- [Database Management](#-database-management)
- [Project Documentation](#-project-documentation)
- [Developer Information](#-developer-information)

---

## 📖 Project Overview

The **School Admission System** is a web-based application that enables students to apply for school admission through a structured and transparent process.

Students can browse available classes and shifts, check seat availability, reserve seats temporarily, and confirm their admission through a simulated payment process.

The system also provides an administrative interface for creating classes and shifts and monitoring seat availability.

The application is containerized using Docker Compose, allowing developers to run the entire system without manually installing .NET, Node.js, or PostgreSQL.

---

## 🛠️ Technology Stack

| Component | Technology |
|---|---|
| Backend | .NET 8 |
| Backend Framework | ASP.NET Core Web API |
| Frontend | React 18 |
| Frontend Build Tool | Vite |
| Database | PostgreSQL |
| Containerization | Docker |
| Container Orchestration | Docker Compose |
| API Documentation | Swagger / OpenAPI |
| Automated Testing | .NET Testing Framework |

---

## ✨ Key Features

### 👨‍🎓 Student Features

- Student identification using a 17-digit birth registration number.
- Browse available classes and shifts.
- View available seat counts.
- Temporarily reserve an available seat.
- Confirm admission through simulated payment.
- View personal admission applications.
- Cancel personal active reservations.
- Track reservation and admission statuses.
- Automatic expiration of temporary seat reservations.

### ⏳ Seat Reservation Management

- Temporary seat reservation system.
- Default reservation duration of 600 seconds (10 minutes).
- Automatic expiration of reservations after the configured time.
- Prevents multiple active reservations for the same student and class.
- Protects seat capacity during concurrent reservation requests.
- Only the reservation owner can confirm or cancel their reservation.

### 🛡️ Administration Features

- Create school classes.
- Configure shifts and admission capacity.
- Monitor seat availability.
- View class information and seat counts.
- Manage admission-related information through the admin interface.

### 💳 Simulated Payment System

- Confirm admission using a simulated payment button.
- Validate reservation ownership.
- Prevent confirmation after reservation expiration.
- Support idempotent confirmation requests.

**Note:** The payment functionality is simulated and does not process real financial transactions.

---

## 🔄 Application Workflow

The admission process follows these steps:

1. **Student Identification:** Enter the student's 17-digit birth registration number.
2. **Class Selection:** Browse available classes and shifts.
3. **Seat Availability:** Check available seats before applying.
4. **Temporary Reservation:** Create a temporary seat reservation.
5. **Reservation Countdown:** Complete the admission process within the reservation period.
6. **Payment Confirmation:** Confirm admission using the simulated payment option.
7. **Application Tracking:** View the application and admission status.
8. **Cancellation:** Cancel an active reservation if necessary.

### Reservation Statuses

| Status | Description |
|---|---|
| ACTIVE | Reservation is currently active. |
| CONFIRMED | Admission has been successfully confirmed. |
| CANCELLED | Reservation has been cancelled. |
| EXPIRED | Reservation time has elapsed. |

---

## 🏗️ Project Architecture

The application follows a client-server architecture with a PostgreSQL database.

```text
SchoolAdmissionSystem/
│
├── backend/
│   ├── SchoolAdmission.API/
│   ├── SchoolAdmission.Tests/
│   └── ...
│
├── frontend/
│   ├── src/
│   ├── public/
│   ├── package.json
│   └── ...
│
├── docker-compose.yml
├── ASSUMPTIONS.md
├── AI_USAGE.md
└── README.md
```

*Note: This is a high-level architectural illustration. Actual directory names may vary depending on the repository structure.*

### Architecture Overview

```text
          STUDENT / ADMIN
                 |
                 v
       +--------------------+
       |   React 18 + Vite  |
       |     Frontend       |
       +--------------------+
                 |
                 | HTTP REST API
                 v
       +--------------------+
       |   ASP.NET Core     |
       |     .NET 8 API     |
       +--------------------+
                 |
                 | Database Connection
                 v
       +--------------------+
       |     PostgreSQL     |
       |      Database      |
       +--------------------+
```

Docker Compose manages the application services and PostgreSQL database.

---

## 🚀 Getting Started

### Prerequisites

The application requires only:

- Docker Engine
- Docker Compose

You do not need to install the following separately to run the Dockerized application:

- .NET SDK
- Node.js
- PostgreSQL

### 1. Clone the Repository

```bash
git clone <YOUR_REPOSITORY_URL>
```

### 2. Navigate to the Project Directory

```bash
cd SchoolAdmissionSystem
```

### 3. Build and Run the Application

Execute the following command:

```bash
docker compose up --build
```

Docker Compose will build the required images and start the backend, frontend, and PostgreSQL services.

Wait until the services finish starting before accessing the application.

---

## 🌐 Application Access

After successfully starting the containers, access the application using the following URLs:

| Service | URL |
|---|---|
| Frontend Application | http://localhost:3000 |
| Backend API | http://localhost:8080 |
| Swagger API Documentation | http://localhost:8080/swagger |

### Frontend

The React application provides the student admission interface and school administration panel.

### Backend API

The ASP.NET Core API handles admission operations, reservation management, and database communication.

### Swagger

Swagger provides interactive API documentation for exploring and testing available endpoints.

---

## 🔌 REST API Documentation

The backend exposes the following RESTful API endpoints.

### Class Management

| HTTP Method | Endpoint | Description |
|---|---|---|
| POST | `/classes` | Create a new class (admin demo endpoint). |
| GET | `/classes` | Retrieve all classes with seat counts. |
| GET | `/classes/{classId}` | Retrieve details of a specific class. |

### Seat Reservation Management

| HTTP Method | Endpoint | Description |
|---|---|---|
| POST | `/classes/{classId}/holds` | Create a temporary seat reservation. |
| GET | `/holds/{holdId}` | Retrieve reservation status. |
| POST | `/holds/{holdId}/confirm` | Confirm admission using simulated payment. |
| DELETE | `/holds/{holdId}` | Cancel an owned reservation. |
| GET | `/me/holds` | Retrieve the current student's applications. |

### Student Identification Header

The following endpoints require the `X-Student-Id` HTTP header:

- `POST /classes/{classId}/holds`
- `POST /holds/{holdId}/confirm`
- `DELETE /holds/{holdId}`
- `GET /me/holds`

Example:

```http
X-Student-Id: 12345678901234567
```

**Important:** The student ID header is used for student identification in this application. It should not be considered production-grade authentication or authorization.

---

## ⚙️ Environment Configuration

The application supports environment-based configuration.

### Backend Configuration

| Environment Variable | Description |
|---|---|
| `ConnectionStrings__Default` | PostgreSQL database connection string. |
| `HOLD_DURATION_SECONDS` | Duration of temporary seat reservations. |
| `ASPNETCORE_URLS` | ASP.NET Core application listening URL. |

### Frontend Configuration

| Environment Variable | Description |
|---|---|
| `VITE_API_URL` | Backend API base URL. |

Default frontend API configuration:

```env
VITE_API_URL=http://localhost:8080
```

### Reservation Duration

The default reservation duration is:

```env
HOLD_DURATION_SECONDS=600
```

This represents a 10-minute temporary seat reservation.

---

## 🧪 Automated Testing

The backend includes automated tests to validate important business rules and system reliability.

### Test Coverage

**1. Seat Capacity Concurrency**

Validates that simultaneous reservation requests cannot exceed the available seat capacity.

**2. Duplicate Active Reservations**

Ensures that the same student cannot create duplicate active reservations for the same class.

**3. Idempotent Confirmation**

Ensures that repeated admission confirmation requests do not create duplicate admissions.

### Run Backend Tests

To execute the automated backend tests locally, install the .NET 8 SDK and configure the PostgreSQL test database.

Run:

```bash
dotnet test backend/SchoolAdmission.Tests/SchoolAdmission.Tests.csproj
```

### PostgreSQL Test Database

The concurrency test requires a PostgreSQL database connection.

The test project reads the database connection string from:

```env
TEST_DATABASE
```

The test project defaults to a local test database connection string.

**Note:** Docker Compose starts the application's database. Refer to the test project's README for the specific test database setup instructions.

---

## 🐳 Docker Commands

### Start All Services

```bash
docker compose up
```

### Build and Start Services

```bash
docker compose up --build
```

### Run in Background

```bash
docker compose up -d --build
```

### View Running Containers

```bash
docker compose ps
```

### View Application Logs

```bash
docker compose logs
```

### View Logs in Real Time

```bash
docker compose logs -f
```

### Stop All Services

```bash
docker compose down
```

### Stop and Remove Database Volume

```bash
docker compose down -v
```

**Important:** 

- `docker compose down` stops and removes containers and networks while preserving persistent PostgreSQL data.
- `docker compose down -v` also removes the associated Docker volumes, including `postgres_data`. This permanently deletes the database data stored in that volume.

---

## 🗄️ Database Management

The application uses PostgreSQL for persistent data storage.

Docker Compose manages the PostgreSQL service and its persistent data through a Docker volume.

**Database Volume:**

```text
postgres_data
```

The database volume ensures that application data remains available after stopping and restarting containers.

To reset the database completely:

```bash
docker compose down -v
docker compose up --build
```

This removes the existing database volume and starts with a fresh database.

---

## 📚 Project Documentation

Additional documentation is available in the repository:

| File | Description |
|---|---|
| `ASSUMPTIONS.md` | Documents project assumptions, business rules, and implementation decisions. |
| `AI_USAGE.md` | Describes AI tools and assistance used during project development. |

---

## 🔐 Security & Implementation Notes

- Student identification is handled through the `X-Student-Id` request header.
- Reservation ownership is validated during confirmation and cancellation.
- Seat capacity is protected against concurrent reservation requests.
- Temporary reservations have a configurable expiration period.
- Payment confirmation is simulated, not connected to a real payment gateway.
- The admin creation endpoint is a demonstration endpoint, not a production-secured administrative interface.

For production deployment, implement proper authentication, role-based authorization, secure student identity verification, input validation, HTTPS, and payment gateway integration.

---

## 👨‍💻 Developer Information

**MD. Atiqul Islam (Atik)**

Software Engineer | Full-Stack Developer

- **Email:** atik.cmttiu1001@gmail.com
- **GitHub:** [ATIQULTIU](https://github.com/ATIQULTIU)
- **Portfolio:** [View Portfolio](https://md-atiqul-islam.my.canva.site/)

---

## ⭐ Acknowledgements

This project demonstrates practical implementation of:

- Full-stack web application development.
- RESTful API design and integration.
- Relational database management.
- Docker-based application deployment.
- Concurrent seat reservation handling.
- Automated backend testing.
- Modern frontend development.

---

<p align="center">
  <strong>🎓 School Admission System</strong>
  <br />
  Designed and developed by MD. Atiqul Islam (Atik)
  <br />
  <em>Building practical solutions through modern software engineering.</em>
</p>
