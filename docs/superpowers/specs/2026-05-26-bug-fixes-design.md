# Bug Fix Plan — Attencial QA Findings

**Date:** 2026-05-26  
**Scope:** Functional bugs only (not security, not code quality)  
**Branch:** khayyam-241871

---

## Fixes (7 bugs)

### 1. Sync-over-async deadlock in FaceService constructor

**File:** `Attencial.API/Services/FaceService.cs:34`  
**Problem:** `EnsureCollectionExistsAsync().GetAwaiter().GetResult()` blocks the calling thread on startup. If the synchronization context is captured, this deadlocks.  
**Fix:** Remove the blocking call from the constructor. Move collection initialization to a lazy async pattern — check/create the collection on first API call. Add a `_collectionEnsured` flag with a semaphore.

### 2. No database transactions on multi-step writes

**Files:**
- `Attencial.API/Controllers/AttendanceController.cs:97-119` — CreateSession writes `AttendanceSession`, saves, then writes `OnlineAttendanceToken`. If the second SaveChangesAsync fails, the session row is orphaned.
- `Attencial.API/Controllers/EnrollmentController.cs:233-244` — EnrollStudent updates the student, then adds 3 FaceVector rows. If face vector inserts fail, student status is already "Trained" with no vectors.

**Fix:** Wrap both multi-step operations in `await _context.Database.BeginTransactionAsync()` + `CommitAsync()` with try/catch rollback.

### 3. Rate limiting race condition

**File:** `Attencial.API/Services/AttendanceService.cs:221-235`  
**Problem:** `IncrementCacheKeyAsync` reads the current count, increments it in memory, then writes back. Two concurrent requests can both read N, both set N+1, losing a count.  
**Fix:** `IDistributedCache` has no atomic increment. Use a static `ConcurrentDictionary<string, SemaphoreSlim>` to serialize increments per cache key. Each key gets its own semaphore (fine-grained locking). Keep TTL behavior intact.

### 4. Registration auto-creates Student only

**File:** `Attencial.API/Controllers/AuthController.cs:79-90`  
**Problem:** `Register()` only checks for `"Student"` role to auto-create a profile. Professors registering get no profile and must separately call `/api/seed/create-professor-profile`.  
**Fix:** Mirror the Student logic — when registering as `"Professor"`, create a `Professor` row with `FullName` and a default `Department` (or take it from the registration request, or leave empty). Add `FullName` to `RegisterRequest` validation for Professor role, and optionally `Department`.

### 5. "Remember me" checkbox is dead UI

**File:** `Attencial.Client/Pages/Login.razor:49-52` and code section  
**Problem:** The checkbox is captured in the `rememberMe` field but never sent to the login API and never persisted.  
**Fix:** Remove the checkbox entirely — it's misleading. JWT tokens always expire in 60 minutes by design, and there's no refresh token mechanism to support "remember me."

### 6. Dead "Forgot password" link

**File:** `Attencial.Client/Pages/Login.razor:73`  
**Problem:** `<a href="login#">` links to the current page with a fragment, doing nothing.  
**Fix:** Replace the dead link with a `@onclick` handler that shows a toast/alert: "Password reset is not yet available. Please contact your administrator."

### 7. Token truncation without length guard

**File:** `Attencial.API/Controllers/AttendanceController.cs:91-95`  
**Problem:** `tokenString[..64]` uses C# range operator on the Base64 result. A 48-byte input produces 64 Base64 chars after replacement, so it's _always_ exactly 64. But if the math ever changes or RandomNumberGenerator returns fewer bytes, this throws `ArgumentOutOfRangeException`.  
**Fix:** Guard with `Math.Min(tokenString.Length, 64)` or just drop the truncation since 48 bytes → 64 chars is deterministic.

---

## Files touched

| File | Fix |
|------|-----|
| `Services/FaceService.cs` | Lazy async collection init instead of blocking .GetResult() |
| `Controllers/AttendanceController.cs` | Transaction wrapper on CreateSession; length guard on token |
| `Controllers/EnrollmentController.cs` | Transaction wrapper on EnrollStudent |
| `Services/AttendanceService.cs` | Semaphore around rate limit increment |
| `Controllers/AuthController.cs` | Auto-create Professor profile on register |
| `Pages/Login.razor` | Remove "Remember me" checkbox; replace dead forgot-password link |

## What's NOT in scope

- Security credential management (appsettings.json secrets)
- Authorization role fixes (admin vs professor)
- Tests (no test project exists yet)
- Logging (Console.WriteLine → ILogger migration)
- Code quality (seed controller relocation, enum usage, etc.)
