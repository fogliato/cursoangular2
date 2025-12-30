# ProAgil - Event Management System

A full-stack event management application built with **Angular 9** and **.NET 8 Web API**, featuring JWT authentication, SQL Server database, and an AI-powered agent using Google Gemini.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Features](#features)

---

## Overview

ProAgil is an event management system that allows users to:
- Create, edit, and delete events
- Manage speakers and their associations with events
- Handle event batches and social media links
- Upload images for events
- User authentication with JWT tokens
- AI-powered natural language API interaction (Agent)

---

## Architecture

The project follows a **layered architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    ProAgil-App (Frontend)                   │
│                      Angular 9 + Bootstrap                  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   ProAgil.WebApi (API Layer)                │
│              ASP.NET Core 8 Web API + Swagger               │
│         Controllers, DTOs, AutoMapper, JWT Auth             │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 ProAgil.Domain (Business Layer)             │
│           Entities, Identity Models, Agent Services         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│               ProAgil.Repository (Data Layer)               │
│         Entity Framework Core, DbContext, Migrations        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      SQL Server Database                    │
└─────────────────────────────────────────────────────────────┘
```

### Project Layers

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Presentation** | `ProAgil-App` | Angular SPA with user interface |
| **API** | `ProAgil.WebApi` | REST API, authentication, DTOs, controllers |
| **Domain** | `ProAgil.Domain` | Business entities, Identity models, AI Agent |
| **Data** | `ProAgil.Repository` | EF Core DbContext, repositories, migrations |

---

## Technologies

### Backend (.NET 8)
- **ASP.NET Core 8** - Web API framework
- **Entity Framework Core 8** - ORM with Code-First migrations
- **ASP.NET Core Identity** - User authentication and authorization
- **JWT Bearer Authentication** - Token-based authentication
- **AutoMapper** - Object-to-object mapping
- **Swagger/Swashbuckle** - API documentation
- **Microsoft Semantic Kernel** - AI integration with Google Gemini

### Frontend (Angular 9)
- **Angular 9** - Frontend framework
- **Bootstrap 4 + Bootswatch** - CSS framework
- **ngx-bootstrap** - Angular Bootstrap components
- **ngx-toastr** - Toast notifications
- **RxJS** - Reactive programming
- **@auth0/angular-jwt** - JWT handling

### Database
- **SQL Server** - Relational database

---

## Project Structure

```
cursoangular2/
├── ProAgil.sln                    # Solution file
│
├── ProAgil.WebApi/                # API Layer
│   ├── Controllers/
│   │   ├── EventController.cs     # Event CRUD operations
│   │   ├── UserController.cs      # Authentication (login/register)
│   │   ├── SpeakerController.cs   # Speaker CRUD operations
│   │   ├── ContactController.cs   # Contact endpoint
│   │   └── AgentController.cs     # AI Agent endpoint
│   ├── Dtos/                      # Data Transfer Objects
│   │   ├── EventDto.cs
│   │   ├── SpeakerDto.cs
│   │   ├── BatchDto.cs
│   │   ├── SocialNetworkDto.cs
│   │   ├── ContactDto.cs
│   │   └── UserDto.cs
│   ├── Mapper/                    # AutoMapper profiles
│   ├── Properties/
│   ├── Resources/Images/          # Uploaded images
│   ├── appsettings.json           # Configuration
│   ├── Startup.cs                 # Service configuration
│   └── Program.cs                 # Entry point
│
├── ProAgil.Domain/                # Domain Layer
│   ├── Event.cs                   # Event entity
│   ├── Speaker.cs                 # Speaker entity
│   ├── Batch.cs                   # Batch entity
│   ├── SocialNetwork.cs           # Social network entity
│   ├── SpeakerEvent.cs            # Many-to-many relationship
│   ├── Identity/                  # ASP.NET Identity models
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   └── UserRole.cs
│   └── Agent/                     # AI Agent services
│       ├── AgentService.cs        # Main orchestrator
│       ├── GeminiApiService.cs    # Gemini API integration
│       ├── ControllerInvoker.cs   # Dynamic controller invocation
│       ├── MultiStepExecutor.cs   # Multi-step operations
│       ├── AgentPrompts.cs        # AI prompts
│       └── ...
│
├── ProAgil.Repository/            # Data Layer
│   ├── ProAgilContext.cs          # EF Core DbContext
│   ├── ProAgilRepository.cs       # Repository pattern
│   ├── IProAgilRepository.cs      # Repository interface
│   └── Migrations/                # EF Core migrations
│
├── ProAgil.Test/                  # Unit Tests
│   └── EventControllerTest.cs     # Controller tests
│
└── ProAgil-App/                   # Angular Frontend
    └── src/app/
        ├── events/                # Events module
        │   ├── events.component.ts/html/css
        │   └── eventEdit/         # Event edit component
        ├── speakers/              # Speakers module
        ├── contacts/              # Contacts module
        ├── dashboard/             # Dashboard module
        ├── user/                  # Login/Registration
        │   ├── login/
        │   └── registration/
        ├── auth/                  # Auth guard & interceptor
        ├── services/              # HTTP services
        │   ├── event.service.ts
        │   ├── speaker.service.ts
        │   ├── contact.service.ts
        │   └── auth.service.ts
        ├── models/                # TypeScript models
        │   ├── Event.ts
        │   ├── Speaker.ts
        │   ├── Batch.ts
        │   ├── SocialNetwork.ts
        │   ├── Contact.ts
        │   └── User.ts
        └── shared/                # Shared components
```

---

## Prerequisites

Before running the project, ensure you have installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 12+](https://nodejs.org/) (for Angular CLI)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli@9`)
- [SQL Server](https://www.microsoft.com/sql-server) (or Docker with SQL Server image)
- [Git](https://git-scm.com/)

### Optional (for AI Agent)
- Google Gemini API Key (set as environment variable `GEMINI_API_KEY`)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd cursoangular2
```

### 2. Setup SQL Server Database

You can use Docker to run SQL Server:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=pass@word191" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Or update the connection string in `ProAgil.WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ProAgil;User Id=SA;Password=pass@word191;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  }
}
```

### 3. Apply Database Migrations

```bash
cd ProAgil.WebApi

# Create new migration (if needed)
dotnet ef migrations add InitialCreate --project ../ProAgil.Repository

# Apply migrations to create database tables
dotnet ef database update --project ../ProAgil.Repository
```

### 4. Run the Backend API

```bash
cd ProAgil.WebApi
dotnet run
```

The API will be available at:
- **HTTP**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

### 5. Run the Frontend (Angular)

In a new terminal:

```bash
cd ProAgil-App

# Install dependencies (first time only)
npm install

# Start development server
npm start
```

The Angular app will be available at: http://localhost:4200

---

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/user/register` | Register new user |
| POST | `/api/user/login` | Login and get JWT token |

### Events
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/event` | Get all events |
| GET | `/api/event/{id}` | Get event by ID |
| GET | `/api/event/getByTheme/{theme}` | Search events by theme |
| GET | `/api/event/getLatestEvents` | Get latest events |
| POST | `/api/event` | Create new event |
| PUT | `/api/event/{id}` | Update event |
| PUT | `/api/event/simpleUpdate/{id}` | Simple update event |
| DELETE | `/api/event/{id}` | Delete event |
| POST | `/api/event/upload` | Upload event image |

### Speakers
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/speaker` | Get all speakers |
| GET | `/api/speaker/{id}` | Get speaker by ID |
| GET | `/api/speaker/getByName/{name}` | Search speakers by name |
| POST | `/api/speaker` | Create new speaker |
| PUT | `/api/speaker/{id}` | Update speaker |
| DELETE | `/api/speaker/{id}` | Delete speaker |

### Contacts
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/contact` | Send contact message |

### AI Agent
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/agent` | Send natural language message to AI agent |

---

## Features

### User Authentication
- User registration with ASP.NET Core Identity
- JWT token-based authentication
- Protected routes with authorization policies
- Password validation rules

### Event Management
- Full CRUD operations for events
- Image upload functionality
- Batch management per event
- Social media links per event
- Speaker associations

### Speaker Management
- Full CRUD operations for speakers
- Image upload functionality
- Short bio with rich text editor
- Event associations

### AI Agent (Experimental)
- Natural language processing using Google Gemini
- Automatic API endpoint identification
- Multi-step operation execution
- Dynamic parameter extraction

---

## Database Entities

### Event
| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Location | string | Event location |
| EventDate | DateTime | Event date and time |
| Theme | string | Event theme/title |
| PeopleCount | int | Number of attendees |
| ImageUrl | string | Event image URL |
| Phone | string | Contact phone |
| Email | string | Contact email |
| Batches | List<Batch> | Event batches |
| SocialNetworks | List<SocialNetwork> | Social media links |
| SpeakerEvents | List<SpeakerEvent> | Associated speakers |

### Speaker
| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Name | string | Speaker name |
| ShortBio | string | Short biography |
| ImageUrl | string | Speaker image URL |
| Phone | string | Contact phone |
| Email | string | Contact email |
| SocialNetworks | List<SocialNetwork> | Social media links |
| SpeakerEvents | List<SpeakerEvent> | Associated events |

### Batch
| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Name | string | Batch name |
| Price | decimal | Ticket price |
| StartDate | DateTime | Sale start date |
| EndDate | DateTime | Sale end date |
| Quantity | int | Available tickets |
| EventId | int | Associated event |

### SocialNetwork
| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Name | string | Network name |
| URL | string | Profile URL |
| EventId | int? | Associated event |
| SpeakerId | int? | Associated speaker |

---

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `GEMINI_API_KEY` | Google Gemini API key for AI agent | - |
| `GEMINI_MODEL` | Gemini model to use | `gemini-2.5-flash` |

---

## Development Notes

### Adding New Migrations

```bash
cd ProAgil.WebApi
dotnet ef migrations add MigrationName --project ../ProAgil.Repository
dotnet ef database update --project ../ProAgil.Repository
```

### Running Tests

```bash
cd ProAgil.Test
dotnet test
```

### Building for Production

```bash
# Backend
cd ProAgil.WebApi
dotnet publish -c Release

# Frontend
cd ProAgil-App
ng build --prod
```

---

## License

This project is for educational purposes as part of an Angular + .NET Core course.
