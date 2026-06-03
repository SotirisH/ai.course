# Best Security Practices for .NET Code Authoring

> The key words **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** in this document are to be interpreted as described in [RFC 2119](https://www.rfc-editor.org/rfc/rfc2119).

---

## General Best Practices
- **Use HTTPS**: Call `UseHttpsRedirection()`  in `Program.cs` unless `if (app.Environment.IsDevelopment())`. 
- **Authentication and Authorization**: Apply `[Authorize]` attributes to all non-public endpoints and controllers. 
- **Input Validation**: Validate all incoming request data using model validation or FluentValidation; reject invalid requests with 400 Bad Request. Ensure validation errors follow RFC 7807 Problem Details format.
- **Rate Limiting**: Implement rate limiting using .NET 10's built-in `AddRateLimiter()` with fixed/sliding window policies in `Program.cs`. Example:
  ```csharp
  builder.Services.AddRateLimiter(options =>
  {
      options.AddFixedWindowLimiter("fixed", config =>
      {
          config.Window = TimeSpan.FromMinutes(1);
          config.PermitLimit = 100;
      });
  });
  app.UseRateLimiter();
  ```
- **Logging and Monitoring**: Use `ILogger` for structured logging; ensure no sensitive data (passwords, tokens, PII) is logged. Use Serilog's masking features if needed. *Note: Current `ExceptionHandlingMiddleware` logs `exception.Message` which may contain sensitive data; audit logged content.*
- **Error Handling**: Return generic error messages using RFC 7807 Problem Details (built into .NET 10 via `UseProblemDetails()`); never expose stack traces or internal details to clients. *Note: Current project uses custom error response format; migrate to Problem Details for standards compliance.*
- **Security Headers**: Implement security headers via middleware. Avoid deprecated `UseXXssProtection()` (X-XSS-Protection header is obsolete, modern browsers ignore it). Use:
  - `UseHsts()` for HTTP Strict Transport Security
  - `UseXContentTypeOptions()` to prevent MIME-type sniffing
  - `UseReferrerPolicy()` to control Referer header
  - `UseCsp()` for Content Security Policy
  Example:
  ```csharp
  app.UseHsts();
  app.UseXContentTypeOptions();
  app.UseReferrerPolicy(ReferrerPolicy.StrictOriginWhenCrossOrigin);
  app.UseCsp(options => options.DefaultSources(s => s.Self()));
  ```

---

## Best Practices for Blazor WebAssembly Projects
- **Secure API Calls**: Include valid authentication tokens in all API call headers once authentication is implemented.
- **Token Storage**: For client-side Blazor WebAssembly, store JWTs in memory only; never in `localStorage` or `sessionStorage`. `HttpOnly`, `SameSite=Strict` cookies are not applicable to client-side WASM (they are restricted to server-side/hybrid Blazor scenarios).
- **Data Protection**: Encrypt sensitive data before persisting to browser storage; avoid storing sensitive data in browser storage entirely if possible.
- **CORS Configuration**: Configure CORS policies with specific origins, methods, and headers; never use `AllowAnyOrigin()`, `AllowAnyMethod()`, or `AllowAnyHeader()` in production. 

---

## Application Configuration
- **Environment Variables**: Access sensitive settings via `IConfiguration` or `Environment.GetEnvironmentVariable()`; never hardcode. 
- **`.env` File Hygiene**: Exclude `.env` files from source control via `.gitignore`; maintain `.env.example`. 
- **Secrets Management**: For production, use Azure Key Vault (`Azure.Extensions.AspNetCore.Configuration.Secrets`) or AWS Secrets Manager; avoid third-party secret loaders like `dotenv.net` in production.
- **Configuration Management**: If there is a `appsettings.Production.json` or similar files, ensure that they don't contain any secrets. The key can be there but the values must be empty.. 
- **Blazor WASM Configuration**: Avoid storing any secrets in `wwwroot/appsettings.json` (client-accessible); all sensitive settings must come from the API.

---

## Authentication & JWT Hardening
- **Token Expiry**: Configure access token expiry to 15-60 minutes; use refresh tokens for longer sessions.
- **Refresh Tokens**: Implement refresh token rotation and revocation; store refresh tokens securely in the database with expiry and revocation tracking.
- **Claim Validation**: Validate `iss`, `aud`, `exp`, `nbf` claims during JWT validation. In .NET 10, use `TokenValidationParameters`:
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
          options.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              // ... configure issuer, audience, signing key
          };
      });
  ```
- **Signing Algorithm**: Use `RS256` or `ES256` for JWT signing; never accept `none` algorithm. Set `RequireSignedTokens = true`.
- **Token Transmission**: Transmit tokens only via `Authorization: Bearer` header; never in query strings, cookies without proper flags, or request bodies.
- **Token Revocation**: Implement a token blacklist or use short-lived access tokens with secure refresh token rotation.

---

## Swagger / OpenAPI Exposure
- **Production Restriction**: Wrap `UseSwagger()` and `UseSwaggerUI()` in `if (env.IsDevelopment())` checks.
- **Authentication on Swagger**: Protect Swagger UI with `[Authorize]` in non-production environments. Use `SwaggerUI` with OAuth or API key if needed.
- **Sensitive Schema Exposure**: Exclude internal properties (e.g., entity keys, internal DTO fields) from Swagger using `[JsonIgnore]` or Swagger schema filters.
- **Swagger Endpoint Security**: Consider disabling Swagger entirely in production or restricting access via IP allowlisting or authentication. Example:
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }
  ```

---

## Audit Logging
- **Mutation Logging**: Log create/update/delete operations with: operation type, resource ID, actor ID (from `HttpContext.User`), UTC timestamp, and source IP. Use structured logging for easy querying.
- **Sensitive Data**: Exclude passwords, tokens, PII from logs; use logging framework masking features (e.g., Serilog's `Destructuring` or `IDestructuringPolicy`).
- **Log Integrity**: Configure logging providers to write to append-only stores (e.g., Azure Monitor, Seq with proper retention); avoid application-level log modification.
- **Log Sanitization**: Never log full request bodies, headers (except safe ones), or connection strings. Sanitize data before logging:
  ```csharp
  _logger.LogInformation("Created application {ApplicationId} by {ActorId}", id, User.GetUserId());
  ```
- **Audit Trail Storage**: Store audit logs separately from application logs; consider a dedicated audit table in PostgreSQL with write-only permissions.

---

## Dependency Security
- **Version Pinning**: Explicitly specify versions in `.csproj` (e.g., `<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />`). Avoid floating versions like `*` or `x.y.*`.
- **Vulnerability Scanning**: Run `dotnet list package --vulnerable` before commits; fix critical/high CVEs immediately. Also use `dotnet outdated` tool for version checks.
- **No Vulnerable Packages**: Do not introduce packages with known CVEs; update existing ones promptly. Use `validate_cves` tool or GitHub's Dependabot alerts.
- **Minimal Dependencies**: Only include necessary packages; avoid large frameworks or utilities that are not essential to reduce attack surface.
- **Latest Updates**: Always opt for the latest versions; The current version policy is minimum N-1. If there are outdated packages, update them as soon as possible to benefit from security patches and improvements.
- **Regular Updates**: Schedule regular dependency updates (e.g., monthly) to stay current with security patches; The current version policy is minimum N-1.
- **Minimal Dependencies**: Audit and remove unused `PackageReference` entries regularly. Use `dotnet list package --outdated` to identify stale packages.
- **Dependabot Configuration**: Add `.github/dependabot.yml` to automate dependency updates and vulnerability alerts. Example:
  ```yaml
  version: 2
  updates:
    - package-ecosystem: "nuget"
      directory: "/"
      schedule:
        interval: "weekly"
      open-pull-requests-limit: 10
  ```
- **Third-Party Package Risks**: Avoid packages like `dotenv.net` in production; prefer native .NET configuration providers. Audit packages for malicious code before use.

---

## Database Calls (SQL + PostgreSQL)
- **Parameterized Queries**: Use EF Core LINQ or `NpgsqlParameter` objects; never concatenate user input into SQL strings.
- **Database Permissions**: Configure db user with least-privilege permissions (`SELECT`, `INSERT`, `UPDATE`, `DELETE` on needed tables only); avoid superuser privileges.
- **Encryption**: For PostgreSQL, use `pgcrypto` extension for column-level encryption of sensitive data; enforce SSL/TLS in connection strings with `SSL Mode=Require;Trust Server Certificate=false`.
- **Migration Safety**: Review migration `Up()`/`Down()` methods for destructive operations (e.g., `DropColumn`, `DropTable`); never call `MigrateAsync()` in production startup.
- **Connection String Security**: Store connection strings in environment variables or secret managers; never hardcode. Use `Pooling=true;MinPoolSize=1;MaxPoolSize=100` for connection pool hardening.
- **SQL Injection Prevention**: Avoid dynamic SQL generation; if unavoidable, use `EF.Functions.Like()` or parameterized stored procedures via `FromSqlRaw()` with parameters.

---

## Antiforgery Protection
- **API Antiforgery**: For APIs that accept cookie-based authentication, enable antiforgery via `AddAntiforgery()` and `[ValidateAntiforgeryToken]`. For JWT-only APIs, antiforgery is typically not required.
- **Blazor WASM Antiforgery**: Blazor WebAssembly handles antiforgery tokens automatically for forms; ensure server endpoints accept the `RequestVerificationToken` header if using cookie auth.
- **CSRF Prevention**: For cookie-authenticated APIs, require CORS with credentials only from trusted origins; use `WithExposedHeaders()` sparingly.

---

## Blazor WebAssembly Security Notes
- **`.env` in WASM**: Never load `.env` files in client-side Blazor WASM; environment configuration must come from the API or server-rendered initial state.
- **API Surface Reduction**: Expose only necessary endpoints to the Blazor WASM client; use API versioning and deprecation policies.
- **Download Protection**: Mark `wwwroot` files (except `index.html`, `favicon.ico`, and static assets) with appropriate `Cache-Control` and `Content-Security-Policy` headers to prevent unauthorized access.


## References
- [OWASP Top 10 for .NET](https://owasp.org/www-project-top-ten/)
- [RFC 2119 - Key words for use in RFCs](https://www.rfc-editor.org/rfc/rfc2119)
- [RFC 7807 - Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc7807)
- [.NET 10 Security Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [PostgreSQL Security Best Practices](https://www.postgresql.org/docs/current/security.html)
- [Blazor WebAssembly Security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/)
