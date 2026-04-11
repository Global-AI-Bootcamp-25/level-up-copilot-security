# Code Review: Demo Security Findings
**Date**: 2026-04-11
**Ready for Production**: No
**Critical Issues**: 4

## Scope
- Frontend: Client-side script execution risk in XSS demo view
- Backend: Auth secret handling, API data exposure, and cookie hardening
- Supplemental evidence reviewed: docs/code-review/latest-clientapp-npm-audit.md and docs/code-review/latest-clientapp-npm-audit.json

## Priority 1 (Must Fix) ⛔

### 1) XSS via dynamic code execution
- File: ClientApp/src/views/XssView.vue
- Risk: User input is embedded into JavaScript and executed with `eval(...)`.
- OWASP: A03 Injection

**Why it is dangerous**
`eval` runs a string as code in the browser origin of the app. If untrusted input reaches that string, attackers can execute arbitrary JavaScript and steal cookies, tokens, or perform actions as the victim.

**Secure approach**
- Never evaluate user input as code.
- Treat user input as data only.
- Keep Vue default escaping (`{{ value }}`) and avoid `v-html` for untrusted content.

### 2) Hard-coded authentication credential
- File: MyApp.API/Authentication/BasicAuthenticationHandler.cs
- Risk: Shared secret in source code (`ExpectedAuthValue`) can be leaked in commits, logs, screenshots, or package artifacts.
- OWASP: A02 Cryptographic Failures / Secrets Management

**Why it is dangerous**
Once committed, secrets are effectively public to everyone with repo or artifact access, including historical clones and backups.

**Secure approach**
- Store secrets outside source code (User Secrets in development, environment variables, or a vault in production).
- Validate secret presence at startup.
- Use constant-time comparison for secret checks.

### 3) Sensitive data exposure in API response
- File: MyApp.API/Controllers/InsecureController.cs (`GET /my-values`)
- Risk: Response includes JWT-like tokens and encoded credential material.
- OWASP: A01 Broken Access Control / A02 Cryptographic Failures / A09 Security Logging and Monitoring Failures (through leakage)

**Why it is dangerous**
Any caller can copy and replay tokens or extract PII/credentials if endpoint remains anonymous.

**Secure approach**
- Data minimization: only return fields needed by the caller.
- Classify and block secrets/credentials/PII from response models.
- Require authorization for non-public data.

### 4) Insecure cookie configuration
- File: MyApp.API/Controllers/InsecureController.cs (`POST /cookies`)
- Risk: `InsecureCookie` lacks `HttpOnly`, `Secure`, and `SameSite` controls.
- OWASP: A05 Security Misconfiguration

**Why it is dangerous**
- Missing `HttpOnly`: JavaScript can read cookie during XSS.
- Missing `Secure`: cookie can be sent over plaintext HTTP.
- Missing `SameSite`: browser may include cookie in cross-site requests (CSRF risk).

**Secure approach**
- Default all sensitive cookies to `HttpOnly=true`, `Secure=true`, `SameSite=Lax` (or `Strict` when possible).
- Use `SameSite=None` only when cross-site is required, and always with `Secure=true`.

## Recommended Secure Implementations

### Vue: remove eval and keep user input as plain text
```ts
const handleSubmit = () => {
  const value = inputString.value.trim();
  if (!value) return;

  submittedInputs.value.push(value); // Vue escapes in template interpolation
  inputString.value = '';
};
```

### ASP.NET Core: secret from configuration + fixed-time comparison
```csharp
using System;
using System.Security.Cryptography;
using System.Text;

private readonly IConfiguration _configuration;

public BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock,
    IConfiguration configuration) : base(options, logger, encoder, clock)
{
    _configuration = configuration;
}

protected override Task<AuthenticateResult> HandleAuthenticateAsync()
{
    if (!Request.Headers.TryGetValue("Authorization", out var header))
        return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

    var provided = Encoding.UTF8.GetBytes(header.ToString());
    var expectedValue = _configuration["Authentication:StaticToken"];
    if (string.IsNullOrWhiteSpace(expectedValue))
        return Task.FromResult(AuthenticateResult.Fail("Auth secret not configured"));

    var expected = Encoding.UTF8.GetBytes(expectedValue);
    var valid = provided.Length == expected.Length &&
                CryptographicOperations.FixedTimeEquals(provided, expected);

    if (!valid)
        return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header Value"));

    // build principal as before...
    return Task.FromResult(AuthenticateResult.Success(ticket));
}
```

### API response: return only safe DTO fields
```csharp
public class PublicValueDto
{
    public string Label { get; set; }
}

[HttpGet]
[Route("/my-values")]
[Authorize]
public ActionResult<IEnumerable<PublicValueDto>> Get()
{
    return Ok(new[]
    {
        new PublicValueDto { Label = "Sample public value" },
        new PublicValueDto { Label = "Non-sensitive metadata" }
    });
}
```

### Cookie hardening: enforce secure defaults
```csharp
Response.Cookies.Append(
    "SessionCookie",
    sessionToken,
    new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTime.UtcNow.AddMinutes(30)
    });
```

## Notes
- This repository is a workshop/demo with intentionally insecure examples; only harden these paths when the goal is remediation rather than demonstration.
- Frontend npm audit also shows dependency risk (`moment`, `vite`, `rollup`, `minimatch`, `picomatch`) and should be tracked as separate dependency remediation work.