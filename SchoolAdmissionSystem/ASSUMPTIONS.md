# Assumptions and design decisions

1. A student is identified by a 17-digit birth registration number entered on the public UI. No account/password flow was specified; the UI sends it as `X-Student-Id`.
2. A class/shift is one admission unit with its own independent seat capacity.
3. The admin interface is a demo administrative surface without authentication because authentication/roles were not specified. In production, protect it with authorization and audit logging.
4. Creating a hold reserves one seat for `HOLD_DURATION_SECONDS` (default 600). Expired holds are treated as expired on every read/write and therefore immediately release capacity; no client/API call is needed to release them.
5. Only one active HELD or CONFIRMED application per student across the system is allowed. A student can cancel an unconfirmed hold and apply again. Confirmed applications cannot be cancelled through this endpoint.
6. Payment is simulated: clicking “Pay admission fee” confirms the hold. No real payment gateway or fee amount was specified.
7. A hold can be confirmed only by the student whose ID owns it, and only before expiry. Repeated confirmation returns the already-confirmed state without incrementing counts.
8. Capacity safety is enforced inside a PostgreSQL transaction with row-level locking on the class row. This serializes concurrent hold creation for the same class and prevents held + confirmed seats exceeding total seats across multiple API instances.
9. Database migrations are applied automatically at startup for this take-home app. PostgreSQL uses a named Docker volume so data survives container recreation and `docker compose down`.
10. The API returns standard 4xx responses for invalid input, missing resources, conflicts, and ownership failures. Unexpected errors are handled by a global exception handler and return a generic 500 response without leaking internals.
11. All timestamps are stored as UTC. The frontend refreshes status periodically and derives EXPIRED from the API response.
