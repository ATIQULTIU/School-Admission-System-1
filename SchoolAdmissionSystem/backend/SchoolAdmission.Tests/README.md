# Automated tests
Run with .NET 8 SDK:
`dotnet test backend/SchoolAdmission.Tests/SchoolAdmission.Tests.csproj`

The included fast regression tests cover capacity under concurrent reservation attempts, idempotent confirmation, and expiry. For a full provider-level integration test, run a disposable PostgreSQL database and issue concurrent POST requests to `/classes/{id}/holds`; the API's PostgreSQL `FOR UPDATE` class-row lock is the production capacity guard.
