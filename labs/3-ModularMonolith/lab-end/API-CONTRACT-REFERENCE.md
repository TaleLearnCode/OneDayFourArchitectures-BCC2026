# The Circuit — Complete API Contract Reference

**Version:** 1.0  
**Designer:** Letty (Frontend/Integration Engineer)  
**Date:** 2026-05-05  
**Status:** Lab-start Scaffold — Ready for Implementation  

---

## Quick Summary

The Circuit API exposes race event management through four module boundaries:
- **Events** — Race scheduling and management
- **Participants** — Racer registration and profiles
- **Results** — Race results and leaderboards
- **Penalties** — Race penalties and time adjustments (lab task)

All endpoints return immutable `record` DTOs serialized with `System.Text.Json` in `camelCase`.

---

## 1. API Endpoint Reference

### 1.1 Events Endpoints

#### GET /api/events
**Response:** `200 OK` with `List<EventDto>`

Lists all race events (completed, in-progress, scheduled, cancelled).

**Example Response:**
```json
[
  {
    "eventId": 1,
    "eventName": "Spring Quarter Finals",
    "scheduledDate": "2026-06-15T10:00:00Z",
    "venueId": 101,
    "status": "Scheduled"
  }
]
```

---

#### GET /api/events/upcoming
**Response:** `200 OK` with `List<EventDto>`

Lists upcoming (scheduled or in-progress) events only.

---

#### GET /api/events/{eventId}
**Parameters:** `eventId` (int, route)  
**Response:** `200 OK` with `EventDto`, or `404 NotFound`

Fetches a single race event by ID.

**Example Request:**
```
GET /api/events/1
```

**Example Response:**
```json
{
  "eventId": 1,
  "eventName": "Spring Quarter Finals",
  "scheduledDate": "2026-06-15T10:00:00Z",
  "venueId": 101,
  "status": "Scheduled"
}
```

---

### 1.2 Participants Endpoints

#### GET /api/participants
**Response:** `200 OK` with `List<ParticipantDto>`

Lists all registered racers.

**Example Response:**
```json
[
  {
    "racerId": 42,
    "fullName": "Letty Ortiz",
    "licenseNumber": "LIC-2026-00042",
    "teamName": "Team Toretto",
    "isActive": true
  }
]
```

---

#### GET /api/participants/{racerId}
**Parameters:** `racerId` (int, route)  
**Response:** `200 OK` with `ParticipantDto`, or `404 NotFound`

Fetches a single racer by ID.

---

#### GET /api/participants/events/{eventId}
**Parameters:** `eventId` (int, route)  
**Response:** `200 OK` with `List<ParticipantDto>`

Lists racers registered for a specific event.

---

### 1.3 Results Endpoints

#### GET /api/results/events/{eventId}
**Parameters:** `eventId` (int, route)  
**Response:** `200 OK` with `List<RaceResultDto>`

Fetches race results for a specific event (leaderboard).
Results are sorted by finish position.

**✓ Cross-Module Composition Pattern:**
Results controller calls `IParticipantService.GetRacerAsync()` for each result to populate `ParticipantName`.

**Example Response:**
```json
[
  {
    "resultId": 501,
    "eventId": 1,
    "racerId": 42,
    "finishPosition": 2,
    "lapTimeMs": 125430,
    "adjustedTimeMs": 127930,
    "points": 180,
    "participantName": "Letty Ortiz"
  }
]
```

---

#### GET /api/results/racers/{racerId}
**Parameters:** `racerId` (int, route)  
**Response:** `200 OK` with `List<RaceResultDto>`

Fetches all results for a specific racer (career history).

---

#### GET /api/results/{resultId}
**Parameters:** `resultId` (int, route)  
**Response:** `200 OK` with `RaceResultDto`, or `404 NotFound`

Fetches a single race result by ID.

---

### 1.4 Penalties Endpoints (Lab Task)

#### GET /api/penalties
**Response:** `200 OK` with `List<PenaltyDto>`

Lists all penalties (admin view).

---

#### POST /api/penalties
**Request Body:** `CreatePenaltyRequest`  
**Response:** `201 Created` with `PenaltyDto`, or `400 BadRequest`

**✓ PRIMARY LAB ENDPOINT**

Creates a new penalty for a racer in an event.

**Request Body:**
```json
{
  "eventId": 1,
  "racerId": 42,
  "penaltyReason": "CourseCut",
  "penaltyCostMs": 2500
}
```

**Expected Response (201 Created):**
```json
{
  "penaltyId": 701,
  "eventId": 1,
  "racerId": 42,
  "penaltyReason": "CourseCut",
  "penaltyCostMs": 2500,
  "appliedDate": "2026-06-15T11:30:00Z",
  "status": "Issued"
}
```

**Architectural Teaching Point:**
When this endpoint is called:
1. PenaltyService persists the penalty to the Penalties module's database
2. PenaltyService calls `IResultsService.ApplyPenaltyAsync(eventId, racerId, penaltyCostMs)`
3. Results module updates the racer's adjusted time
4. Subsequent GET /api/results calls reflect the penalty

**Key: Penalties module never directly touches Results database. It calls through IResultsService interface.**

---

#### GET /api/penalties/{penaltyId}
**Parameters:** `penaltyId` (int, route)  
**Response:** `200 OK` with `PenaltyDto`, or `404 NotFound`

Fetches a single penalty by ID.

---

#### GET /api/penalties/events/{eventId}
**Parameters:** `eventId` (int, route)  
**Response:** `200 OK` with `List<PenaltyDto>`

Lists all penalties issued in a specific event.

---

#### GET /api/penalties/racers/{racerId}
**Parameters:** `racerId` (int, route)  
**Response:** `200 OK` with `List<PenaltyDto>`

Lists all penalties issued to a specific racer across all events.

---

### 1.5 Health Check Endpoint

#### GET /health
**Response:** `200 OK` with status object

Liveness probe. No module interaction; always returns success if API is running.

**Example Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-06-15T11:30:00Z"
}
```

---

## 2. DTO Reference

### Response DTOs (Immutable Records)

#### EventDto
```csharp
public record EventDto(
    int EventId,
    string EventName,
    DateTime ScheduledDate,
    int VenueId,
    string Status  // "Scheduled", "InProgress", "Completed", "Cancelled"
);
```

#### ParticipantDto
```csharp
public record ParticipantDto(
    int RacerId,
    string FullName,
    string LicenseNumber,
    string TeamName,
    bool IsActive
);
```

#### RaceResultDto
```csharp
public record RaceResultDto(
    int ResultId,
    int EventId,
    int RacerId,
    int FinishPosition,
    int LapTimeMs,
    int AdjustedTimeMs,
    int Points,
    string ParticipantName  // Enriched by Results controller
);
```

#### PenaltyDto
```csharp
public record PenaltyDto(
    int PenaltyId,
    int EventId,
    int RacerId,
    string PenaltyReason,        // "Speeding", "CourseCut", "Contact", "Conduct"
    int PenaltyCostMs,
    DateTime AppliedDate,
    string Status                // "Issued", "Appealed", "Dismissed"
);
```

### Request DTOs

#### CreatePenaltyRequest
```csharp
public record CreatePenaltyRequest(
    int EventId,
    int RacerId,
    string PenaltyReason,     // Must match PenaltyType enum
    int PenaltyCostMs
);
```

---

## 3. HTTP Status Codes

| Code | Meaning | Used By |
|------|---------|---------|
| `200 OK` | Successful GET request | All GET endpoints |
| `201 Created` | Successful POST request | POST /api/penalties |
| `400 BadRequest` | Invalid request parameters | POST /api/penalties (validation failure) |
| `404 NotFound` | Resource not found | GET with ID (resource doesn't exist) |
| `500 InternalServerError` | Unexpected server error | Any endpoint (on exception) |

---

## 4. Cross-Module Communication Pattern

### The Architecture in Action

**Scenario:** Client calls `GET /api/results/events/1`

**Flow:**

1. **API Layer (Host)** — ResultsController receives request
   ```csharp
   public ResultsController(IResultsService resultsService, IParticipantService participantService)
   ```
   Both services are dependency-injected.

2. **Results Module** — Fetches race data
   ```csharp
   var results = await _resultsService.GetResultsForEventAsync(eventId);
   ```
   Results module's DbContext has only `race_results` table. No foreign key to `racers`.

3. **Results → Participants Boundary Crossing** — Enriches with participant data
   ```csharp
   foreach (var result in results)
   {
       var racer = await _participantService.GetRacerAsync(result.RacerId);
       enrichedResults.Add(result with { ParticipantName = racer?.FullName ?? "Unknown" });
   }
   ```
   - IParticipantService is the **contract** (defined in TheCircuit.SharedKernel)
   - ParticipantService (concrete implementation) is **internal** to Participants module
   - The call is in-process; no network overhead

4. **API Response** — Returns enriched results to client
   ```json
   [
     {
       "resultId": 501,
       "eventId": 1,
       "racerId": 42,
       "finishPosition": 2,
       "lapTimeMs": 125430,
       "adjustedTimeMs": 127930,
       "points": 180,
       "participantName": "Letty Ortiz"
     }
   ]
   ```

### Why This Pattern Matters

| Aspect | Benefit |
|--------|---------|
| **No Direct Table Access** | Results never reaches into Participants' database. Data isolation is enforced. |
| **Interface as Contract** | IParticipantService is the formal boundary. Modules collaborate through agreed APIs. |
| **Compiler Enforcement** | Results cannot reference Participants directly. Build fails if you try. |
| **Same Process, Different Domains** | No network calls; method invocation. But architecturally, Results and Participants are separate. |
| **Teaching Bridge** | This exact pattern, with network calls instead of method calls, is how microservices work. |

---

## 5. Serialization & Configuration

### System.Text.Json Configuration

```csharp
// Program.cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

**Effect:**
- Request JSON: `eventId` or `EventId` are accepted (case-insensitive)
- Response JSON: Always uses `camelCase` (`eventId`, `participantName`, etc.)

### Record Type Serialization

All DTOs are immutable `record` types:
```csharp
public record EventDto(int EventId, string EventName, DateTime ScheduledDate, int VenueId, string Status);
```

**Why records:**
- Immutability: DTOs cannot be modified in transit
- Value equality: Two EventDto instances with same field values are equal
- Serializable: System.Text.Json handles them natively
- Compact: Less code than class + property declarations

---

## 6. Error Handling & Validation

### Input Validation

Participants should implement validation in their endpoints:

```csharp
[HttpPost]
public async Task<ActionResult<PenaltyDto>> CreatePenalty([FromBody] CreatePenaltyRequest request)
{
    if (request.EventId <= 0 || request.RacerId <= 0)
        return BadRequest("EventId and RacerId must be positive");

    if (!Enum.TryParse<PenaltyType>(request.PenaltyReason, out _))
        return BadRequest("Invalid PenaltyReason");

    // Process...
}
```

### 404 Not Found

Return 404 when a resource doesn't exist:

```csharp
var eventDto = await _eventService.GetEventAsync(eventId);
if (eventDto == null)
    return NotFound();
return Ok(eventDto);
```

### 500 Internal Server Error

Let ASP.NET Core framework handle unhandled exceptions → 500 response.

---

## 7. Endpoint Summary Table

| Method | Endpoint | Response Type | Participant Lab Task? |
|--------|----------|---------------|----------------------|
| GET | /api/events | List<EventDto> | Pre-built |
| GET | /api/events/upcoming | List<EventDto> | Pre-built |
| GET | /api/events/{eventId} | EventDto | Pre-built |
| GET | /api/participants | List<ParticipantDto> | Pre-built |
| GET | /api/participants/{racerId} | ParticipantDto | Pre-built |
| GET | /api/participants/events/{eventId} | List<ParticipantDto> | Pre-built |
| GET | /api/results/events/{eventId} | List<RaceResultDto> | Pre-built (composition) |
| GET | /api/results/racers/{racerId} | List<RaceResultDto> | Pre-built (composition) |
| GET | /api/results/{resultId} | RaceResultDto | Pre-built |
| GET | /api/penalties | List<PenaltyDto> | **Implement** |
| POST | /api/penalties | PenaltyDto | **Implement** |
| GET | /api/penalties/{penaltyId} | PenaltyDto | **Implement** |
| GET | /api/penalties/events/{eventId} | List<PenaltyDto> | **Implement** |
| GET | /api/penalties/racers/{racerId} | List<PenaltyDto> | **Implement** |
| GET | /health | Object | Pre-built |

---

## 8. Lab Task Checklist — Participants

### Endpoint Implementation (POST /api/penalties)

- [ ] Parse `CreatePenaltyRequest` from request body
- [ ] Validate `eventId > 0` and `racerId > 0`
- [ ] Validate `penaltyReason` is a valid PenaltyType
- [ ] Call `_penaltyService.IssuePenaltyAsync()`
- [ ] Return `201 Created` with the penalty object
- [ ] Include `Location` header with resource URL

### PenaltyService Implementation

- [ ] Inject `IResultsService` into constructor
- [ ] In `IssuePenaltyAsync()`:
  - [ ] Create Penalty entity
  - [ ] Persist to PenaltiesDbContext
  - [ ] Call `_resultsService.ApplyPenaltyAsync()` to update results
  - [ ] Return PenaltyDto

### Acceptance Criteria

- [ ] Endpoint accepts CreatePenaltyRequest
- [ ] Penalty is saved to database
- [ ] IResultsService.ApplyPenaltyAsync() is called
- [ ] GET /api/results/events/{eventId} shows updated adjusted times
- [ ] No errors on subsequent requests
- [ ] Code compiles without warnings

---

## 9. Teaching Points for Facilitators

When walking through the code:

1. **Open Sections/3-ModularMonolith/Lab-start/TheCircuit.Api/Controllers/ResultsController.cs**
   - Point to the constructor: "Results controller depends on IParticipantService"
   - Show GetResultsForEvent(): "We make a method call to Participants, but we never reference Participants directly"
   - Highlight comment: "This is the module boundary crossing via interface"

2. **Show the project references in .csproj files**
   - TheCircuit.Api references Events, Participants, Results modules
   - TheCircuit.Results references only TheCircuit.SharedKernel
   - "Results cannot reference Participants. The compiler enforces this."

3. **Compare to monolith (Module 1)**
   - "In Module 1, one database, one LayerController could call LayerService and access any table"
   - "Here, each module owns its data. Collaboration happens through interfaces."

4. **Connect to microservices (Module 4 preview)**
   - "This exact pattern — module A calls module B through an interface — works when module B is a separate service"
   - "Instead of method calls, you'd have HTTP calls to module B's API"
   - "The architecture is already there. We just swap the transport mechanism."

---

## Summary

The Circuit API demonstrates:
✅ RESTful resource-oriented design  
✅ Immutable DTOs (record types)  
✅ Cross-module data composition through interfaces  
✅ Module isolation (internal classes, interface boundaries)  
✅ Lab hook (Penalties module implementation)  

This design is the bridge between N-Tier (layers, all public) and Modular Monolith (domains, internal enforcement, interface boundaries).
