# Production Audit & Refactor — Design Spec

## Scope
Full codebase audit and refactor of Attencial (.NET 10 solution: API, Blazor WASM Client, Shared).

## Phases

### Phase 1: Security & Critical Fixes
- Replace hardcoded secrets in appsettings.Production.json with env var references
- Fix FaceService blocking async (.GetAwaiter().GetResult() → async factory pattern)
- Fix swallowed exceptions (add ILogger, meaningful error handling)
- Remove Console.WriteLine debug logging → ILogger

### Phase 2: Dead Code & Cleanup
- Remove: TestController, empty ApiEndpoints, unused FacultyAbuseLog from DbContext
- Clean decompiled client code: strip [CompilerGenerated], unused usings, Runtime.CompilerServices
- Remove artifacts: client-backup/, decompiled-client/, restore_razor.py, stitch-export/

### Phase 3: Structural Improvements
- Replace magic strings with constants/enums (roles, statuses)
- Extract inline DTOs to Shared project
- Add AsNoTracking() to read-only queries
- Fix N+1 query in StudentController.GetMyAttendance()
- Standardize controller routes
- Remove unused SendGrid package reference

### Phase 4: Async/Performance
- Fix pseudo-async repository methods
- Clean up _Imports.cs
- Add CancellationToken support where beneficial

### Phase 5: Documentation
- XML docs on public interfaces/services
- Class-level responsibility comments
- Business rule documentation (attendance pipeline, rate limiting)
