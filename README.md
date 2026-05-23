# MediRoute — Real-Time Hospital Resource Navigator

> A live, geolocation-aware platform where patients can find the **nearest hospital with available ICU beds, specialists, emergency services, or OTs** — updated in real time as hospital admins change resource status.

**Author:** Garv Goel · [github.com/garv99goel15](https://github.com/garv99goel15)
**Live demo:** _(add your Azure URL here)_

---

## The problem

In an emergency, finding a hospital that *actually has the resource you need right now* (an ICU bed, a free OT, a cardiologist on duty) is painful. Public lists are static and out-of-date. MediRoute closes that gap by letting hospital staff update availability in seconds and broadcasting those updates to every patient currently searching, **without a page refresh**.

---

## Architecture

```
                                +-------------------+
                                |     Patients      |
                                |   (React + Vite)  |
                                +---------+---------+
                                          | HTTPS (REST + WebSocket)
                                          v
+----------------+      JWT      +-------------------+      EF Core      +---------------+
|  Hospital      | <---------->  |   ASP.NET Core 8  |  <------------->  |  SQL Server   |
|  Admin Panel   |               |   Web API + Hub   |                   |  (or InMemory)|
+----------------+               +---------+---------+                   +---------------+
                                          |
                                          | SignalR broadcast
                                          v
                                 Patient browsers
                            (live availability badges)
```

Key flow on an admin update:
1. Admin updates a bed/doctor/ambulance.
2. Controller validates + persists via the repository.
3. `AvailabilityService` invalidates the 60-second `IMemoryCache` entry.
4. New availability snapshot is computed and broadcast via `ResourceHub` to the SignalR group `hospital-{id}`.
5. All connected patient browsers in that group receive an `AvailabilityUpdated` event and re-render badges instantly.

---

## Tech stack

| Layer              | Tech                                                  |
| ------------------ | ----------------------------------------------------- |
| Backend            | ASP.NET Core 8 Web API (C#)                           |
| Real-time          | SignalR                                               |
| Database           | SQL Server + EF Core 8 (in-memory option included)    |
| Auth               | JWT Bearer (Access 15 min + Refresh 7 days)           |
| Caching            | `IMemoryCache` (60s TTL, invalidated on writes)       |
| Validation         | FluentValidation                                      |
| Frontend           | React 18 + TypeScript + Vite                          |
| State              | Zustand                                               |
| Maps               | Leaflet.js + react-leaflet (OpenStreetMap tiles)      |
| Styling            | Tailwind CSS                                          |
| Tests              | xUnit + Moq                                           |
| CI/CD              | GitHub Actions, Azure App Service deployment ready    |

---

## Folder structure

```
mediroute/
├── backend/
│   ├── MediRoute.API/            ASP.NET Core 8 Web API + SignalR
│   │   ├── Controllers/          Auth, Hospitals, Beds, Doctors, Ambulances, Admin
│   │   ├── Hubs/                 ResourceHub (real-time)
│   │   ├── Models/               EF entities + enums
│   │   ├── DTOs/                 Request/response contracts
│   │   ├── Data/                 AppDbContext + SeedData (15 hospitals, Delhi NCR)
│   │   ├── Services/             AvailabilityService, HospitalSearchService, AuthService…
│   │   ├── Repositories/         Repository pattern (no DbContext in controllers)
│   │   ├── Validators/           FluentValidation rules
│   │   ├── Middleware/           Global exception handler
│   │   └── Program.cs
│   └── MediRoute.Tests/          xUnit + Moq unit tests
├── frontend/
│   ├── src/
│   │   ├── components/           Map/, Search/, Hospital/, Admin/, shared/
│   │   ├── hooks/                useSignalR, useGeolocation, useHospitalSearch
│   │   ├── pages/                PatientSearch, HospitalDetail, AdminLogin, AdminDashboard
│   │   ├── services/             api.ts (axios + JWT refresh), signalrService.ts
│   │   ├── store/                Zustand stores (auth, hospital)
│   │   └── types/
│   ├── tailwind.config.js
│   └── vite.config.ts
├── .github/workflows/deploy.yml
└── README.md
```

---

## Local setup

### Prerequisites
- .NET SDK **8.0+**
- Node.js **20+** and npm
- (Optional) SQL Server / SQL Server Express — by default the app uses an in-memory database for instant local startup.

### 1) Backend

```bash
cd mediroute/backend
dotnet restore
dotnet run --project MediRoute.API
```

API: <http://localhost:5000> · Swagger: <http://localhost:5000/swagger>
SignalR hub: `http://localhost:5000/hubs/resource`

By default `appsettings.json` has `"UseInMemoryDatabase": true` (seeded automatically). To use real SQL Server, set it to `false` and update `ConnectionStrings:DefaultConnection`, then:

```bash
dotnet ef migrations add Init --project MediRoute.API
dotnet ef database update --project MediRoute.API
```

### 2) Frontend

```bash
cd mediroute/frontend
npm install
npm run dev
```

Open <http://localhost:5173>.

### 3) Seeded login credentials

| Username     | Role           | Hospital |
| ------------ | -------------- | -------- |
| `superadmin` | SuperAdmin     | _all_    |
| `admin1`…`admin15` | HospitalAdmin | 1…15  |

**Password for all:** `Admin@123`

---

## Environment variables

### Backend (`appsettings.json`)

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MediRoute;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JWT": {
    "Secret": "REPLACE_WITH_A_LONG_RANDOM_256_BIT_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "MediRoute",
    "Audience": "MediRouteUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Cache": { "AvailabilityTTLSeconds": 60 },
  "UseInMemoryDatabase": true
}
```

### Frontend (`.env`)

```
VITE_API_BASE_URL=http://localhost:5000
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/resource
```

---

## API documentation summary

### Public
| Method | Route                                                   | Description |
| ------ | ------------------------------------------------------- | ----------- |
| GET    | `/api/hospitals/search?lat=&lng=&service=&specialization=&radius=` | Ranked search (Availability × 0.6 + Proximity × 0.4) |
| GET    | `/api/hospitals/{id}`                                   | Hospital detail incl. departments, beds, doctors, ambulances |
| GET    | `/api/hospitals/{id}/availability`                      | Live (cached) availability summary |
| GET    | `/api/hospitals/{id}/doctors?specialization=`           | Doctors filtered by specialization |
| GET    | `/api/hospitals/nearby?lat=&lng=&radius=`               | Hospitals within radius (km) |
| GET    | `/api/hospitals/{id}/snapshot-history?hours=24`         | Historical resource snapshots |

### Admin (JWT, role `HospitalAdmin` or `SuperAdmin`)
| Method | Route                                  | Description |
| ------ | -------------------------------------- | ----------- |
| PUT    | `/api/beds/{id}/status`                | Update single bed status |
| PUT    | `/api/beds/bulk-update`                | Update many beds at once |
| PUT    | `/api/doctors/{id}/availability`       | Toggle doctor available/unavailable |
| PUT    | `/api/ambulances/{id}/status`          | Update ambulance status |
| PUT    | `/api/ot/{departmentId}/status`        | Activate/deactivate an OT |
| GET    | `/api/admin/dashboard`                 | Full resource summary |
| POST   | `/api/admin/snapshot`                  | Persist a resource snapshot |

### Auth
| Method | Route                | Description |
| ------ | -------------------- | ----------- |
| POST   | `/api/auth/login`    | Returns JWT access + refresh tokens |
| POST   | `/api/auth/refresh`  | Exchange refresh token for new pair  |

### SignalR — `/hubs/resource`
- `JoinHospitalGroup(hospitalId)`
- `LeaveHospitalGroup(hospitalId)`
- Server → client event: `AvailabilityUpdated` with `{ hospitalId, updatedResourceType, availability, timestamp }`

---

## Search scoring

```
Score             = (AvailabilityScore × 0.6) + (ProximityScore × 0.4)
AvailabilityScore = available_for_service / total_for_service              ∈ [0,1]
ProximityScore    = 1 - (distance_km / search_radius_km)                   ∈ [0,1]
Distance          = Haversine(lat1,lng1, lat2,lng2)
```
Hospitals with 0 availability for the requested service are filtered out. Top 20 returned.

## Color coding
- 🟢 Green — availability > 50 %
- 🟡 Yellow — 20 – 50 %
- 🔴 Red — < 20 %
- ⚪ Grey — service unavailable / department closed

---

## Tests

```bash
cd mediroute/backend
dotnet test
```

Covers `HospitalSearchService` (scoring, filtering, ranking), `AvailabilityService` (caching, color logic, invalidation) and `RoutingService` (Haversine).

---

## Screenshots

| Patient search | Hospital detail | Admin dashboard |
| -------------- | --------------- | --------------- |
| _add screenshot_ | _add screenshot_ | _add screenshot_ |

---

## Deployment (Azure App Service)

1. Create an Azure Web App (Linux, .NET 8).
2. Add the publish profile as the GitHub secret `AZURE_WEBAPP_PUBLISH_PROFILE`.
3. Push to `main` — `.github/workflows/deploy.yml` will build, test, and deploy.

Frontend can be hosted on Azure Static Web Apps, Vercel, or Netlify — point `VITE_API_BASE_URL` and `VITE_SIGNALR_HUB_URL` at your deployed API.

---

## License
MIT
