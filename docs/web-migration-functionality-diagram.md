# Web Migration Functionality Diagram

## General MVC Flow

```mermaid
flowchart LR
    User["User"] --> View["Razor View (.cshtml)"]
    View --> Controller["MVC Controller"]
    Controller --> ViewModel["ViewModel / local page cache"]
    Controller --> Client["HospitalManagement.Web API client"]
    Client --> API["Common.API controller"]
    API --> Service["API service"]
    Service --> Repository["Common.Data repository"]
    Repository --> DbContext["EFHospitalDbContext"]
    DbContext --> Database["SQL Server / LocalDB"]
```

## Authentication Flow

```mermaid
sequenceDiagram
    participant User
    participant View as Login Razor View
    participant Controller as AuthenticationController
    participant Client as AuthenticationApiClient
    participant Api as Common.API AuthController
    participant Session as Web Session

    User->>View: Enter username/password
    View->>Controller: POST AuthenticationView
    Controller->>Client: LoginAsync(username, password)
    Client->>Api: POST /api/auth/login
    Api-->>Client: JWT, username, role
    Client-->>Controller: AuthResponseDto
    Controller->>Session: Store JWT, username, role
    Controller-->>User: Redirect to MVC home/dashboard
```

## Protected API Flow

```mermaid
sequenceDiagram
    participant Page as Web Feature Page
    participant Controller as MVC Controller
    participant Client as Web API Client
    participant Api as Common.API [Authorize] endpoint
    participant Service as API Service
    participant Repo as Repository
    participant Db as Database

    Page->>Controller: GET/POST feature action
    Controller->>Client: Call backend operation
    Client->>Client: Read JWT from Web Session
    Client->>Api: HTTP request with Bearer token
    Api->>Api: Validate JWT and role
    Api->>Service: Execute business operation
    Service->>Repo: Read/write data
    Repo->>Db: EF Core query/update
    Db-->>Repo: Result
    Repo-->>Service: Entity/DTO
    Service-->>Api: Result
    Api-->>Client: JSON response
    Client-->>Controller: Typed model
    Controller-->>Page: Updated ViewModel
```

## Patient Migration Flow

```mermaid
flowchart LR
    PatientPage["Patient Details page"] --> PatientController["HospitalManagement.Web PatientController"]
    AdminPage["Admin patient list"] --> PatientController
    PatientController --> PatientVm["PatientProfileViewModel"]
    PatientController --> PatientClient["PatientApiClient"]
    PatientController --> BillingClient["BillingApiClient"]
    PatientClient --> PatientApi["Common.API PatientController"]
    BillingClient --> BillingApi["Common.API BillingController"]
    PatientApi --> PatientService["PatientService"]
    BillingApi --> BillingService["BillingService"]
    PatientService --> PatientRepos["Patient / History / Record repositories"]
    BillingService --> BillingRepos["History / Record / Prescription / Transplant repositories"]
    PatientRepos --> Db["HospitalManagementDbEF"]
    BillingRepos --> Db
```

## ER Workflow Migration Flow

```mermaid
flowchart LR
    RegistrationPage["Registration page"] --> RegistrationController["RegistrationController"]
    TriagePage["Triage page"] --> TriageController["TriageController"]
    QueuePage["Queue page"] --> QueueController["QueueController"]
    RoomAssignmentPage["Room Assignment page"] --> RoomAssignmentController["RoomAssignmentController"]
    RoomManagementPage["Room Management page"] --> RoomManagementController["RoomManagementController"]
    ExaminationPage["Examination page"] --> ExaminationController["ExaminationController"]
    TransferPage["Transfer page"] --> TransferController["TransferController"]

    RegistrationController --> ErClient["ErWorkflowApiClient"]
    TriageController --> ErClient
    QueueController --> ErClient
    RoomAssignmentController --> ErClient
    RoomManagementController --> ErClient
    ExaminationController --> ErClient
    TransferController --> ErClient

    TriageController --> StaffRules["ErStaffService nurse/doctor rules"]
    ExaminationController --> StaffRules

    ErClient --> VisitsApi["Common.API ERVisitsController"]
    ErClient --> TriageApi["Common.API TriageController / TriageParametersController"]
    ErClient --> RoomsApi["Common.API ERRoomsController"]
    ErClient --> ExamApi["Common.API ExaminationController"]
    ErClient --> TransferApi["Common.API TransferLogController"]

    VisitsApi --> ErServices["ER services"]
    TriageApi --> ErServices
    RoomsApi --> ErServices
    ExamApi --> ErServices
    TransferApi --> ErServices
    ErServices --> ErRepositories["Common.Data ER repositories"]
    ErRepositories --> Db["HospitalManagementDbEF"]
```

## Concrete Patient And ER Page Flow

```mermaid
sequenceDiagram
    participant User
    participant Page as MVC Razor Page
    participant Controller as Feature Controller
    participant Client as ErWorkflowApiClient / PatientApiClient
    participant Api as Common.API protected endpoint
    participant Data as Common.Data

    User->>Page: Select visit/patient and submit action
    Page->>Controller: GET or POST MVC action
    Controller->>Client: Build local ViewModel and call API client
    Client->>Client: Attach Bearer token from session
    Client->>Api: HTTP request to protected API
    Api->>Data: Service/repository operation
    Data-->>Api: Entity or DTO
    Api-->>Client: JSON result
    Client-->>Controller: Typed model
    Controller-->>Page: Render refreshed local page cache
```

## Current Migration Map

| Old feature | MVC target | Backend/API path |
| --- | --- | --- |
| `PatientView` / `PatientViewModel` | `PatientController`, `Views/Patient/Details.cshtml`, `PatientProfileViewModel` | `api/patients/*`, `api/billing/*` |
| Patient admin list/create/update/archive | `AdminController`, `Views/Admin/*`, `CreatePatientViewModel`, `EditPatientViewModel` | `api/patients/*`, `api/allergies` |
| Authentication | `AuthenticationController`, `Views/Authentication/AuthenticationView.cshtml` | `api/auth/login` |
| `PatientRegistrationViewModel` | `RegistrationController`, `Views/Registration/Index.cshtml`, `RegistrationViewModel` | `api/patients`, `api/patients/exists/{cnp}`, `api/er-visits` |
| `TriageViewModel` | `TriageController`, `Views/Triage/Index.cshtml`, `TriageViewModel`, `ErStaffService` | `api/er-visits`, `api/triages`, `api/triage-parameters` |
| `QueueViewModel` | `QueueController`, `Views/Queue/Index.cshtml`, `QueueViewModel` | `api/er-visits`, `api/triages` |
| `RoomAssignmentViewModel` | `RoomAssignmentController`, `Views/RoomAssignment/Index.cshtml`, `RoomAssignmentViewModel` | `api/er-visits/auto-assign-room`, `api/er-visits/{visitId}/assign-room/{roomId}`, `api/er-rooms` |
| `RoomManagementViewModel` | `RoomManagementController`, `Views/RoomManagement/Index.cshtml`, `RoomManagementViewModel` | `api/er-rooms/status/{status}`, `api/er-rooms/{id}/visit-details`, `api/er-rooms/{id}/mark-cleaning`, `api/er-rooms/{id}/mark-available` |
| `ExaminationViewModel` | `ExaminationController`, `Views/Examination/Index.cshtml`, `Views/Examination/Summary.cshtml`, `ExaminationViewModel` | `api/examinations/*`, `api/er-visits`, `api/triages`, `api/triage-parameters` |
| `TransferLogViewModel` | `TransferController`, `Views/Transfer/Index.cshtml`, `TransferViewModel` | `api/transfer-logs/*`, `api/er-visits/{visitId}/transfer`, `api/er-visits/{visitId}/retry-transfer`, `api/er-visits/{visitId}/close` |
| `ImportView`, `NurseView`, `StateManagementView`, `TimelineView`, standalone `RoomView` | Not migrated in this slice because matching old view/viewmodel files or supported backend endpoints were not found | N/A |
