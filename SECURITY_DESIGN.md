# DSAGrind Platform - Security Design Documentation

## 📋 Document Information
- **Version**: 1.0
- **Created**: 2024
- **System**: DSAGrind Competitive Programming Platform
- **Focus**: Comprehensive Security Architecture and Implementation
- **Security Framework**: Zero Trust Architecture with Defense in Depth

---

## 🎯 Security Overview

### Security Philosophy
DSAGrind implements a **Zero Trust Security Model** with multiple layers of protection:
- **Assume Breach**: Every component validates and verifies
- **Least Privilege**: Minimal access rights for users and services
- **Defense in Depth**: Multiple security layers at every level
- **Continuous Monitoring**: Real-time threat detection and response
- **Privacy by Design**: Data protection built into every feature

### Security Domains
```
Security Architecture:
├── Identity & Access Management (IAM)
├── Application Security
├── Data Protection & Privacy
├── Infrastructure Security
├── Code Execution Security
├── Network Security
├── Monitoring & Incident Response
└── Compliance & Governance
```

---

## 🔐 Identity & Access Management

### Authentication Architecture

#### Multi-Factor Authentication (MFA)
```csharp
public class MultiFactorAuthenticationService
{
    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        // Step 1: Primary authentication (password)
        var primaryAuth = await ValidatePrimaryCredentials(request.EmailOrUsername, request.Password);
        if (!primaryAuth.IsValid)
            return AuthenticationResult.Failed("Invalid credentials");

        // Step 2: Check if MFA is required
        var user = primaryAuth.User;
        if (user.IsMfaEnabled || IsHighRiskLogin(request))
        {
            var mfaChallenge = await CreateMfaChallenge(user);
            return AuthenticationResult.RequiresMfa(mfaChallenge);
        }

        // Step 3: Risk-based authentication
        var riskAssessment = await AssessLoginRisk(user, request);
        if (riskAssessment.RiskLevel == RiskLevel.High)
        {
            await TriggerAdditionalVerification(user, request);
            return AuthenticationResult.RequiresAdditionalVerification();
        }

        // Step 4: Generate secure tokens
        var tokens = await GenerateSecureTokens(user, request);
        await LogSuccessfulLogin(user, request);

        return AuthenticationResult.Success(user, tokens);
    }

    private async Task<MfaChallenge> CreateMfaChallenge(User user)
    {
        var availableMethods = new List<MfaMethod>();

        // TOTP (Time-based One-Time Password)
        if (user.MfaSettings.TotpEnabled)
            availableMethods.Add(MfaMethod.Totp);

        // SMS (for backup)
        if (user.MfaSettings.SmsEnabled && !string.IsNullOrEmpty(user.PhoneNumber))
            availableMethods.Add(MfaMethod.Sms);

        // Hardware Security Keys (FIDO2/WebAuthn)
        if (user.MfaSettings.SecurityKeys.Any())
            availableMethods.Add(MfaMethod.SecurityKey);

        // Backup codes
        if (user.MfaSettings.BackupCodes.Any(c => !c.IsUsed))
            availableMethods.Add(MfaMethod.BackupCode);

        return new MfaChallenge
        {
            ChallengeId = Guid.NewGuid().ToString(),
            AvailableMethods = availableMethods,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
    }
}

// Risk Assessment Engine
public class LoginRiskAssessment
{
    public async Task<RiskAssessment> AssessLoginRisk(User user, LoginRequest request)
    {
        var factors = new List<RiskFactor>();

        // Geographic risk
        var location = await _geoLocationService.GetLocationAsync(request.IpAddress);
        if (!user.LoginHistory.Any(h => h.Country == location.Country))
            factors.Add(new RiskFactor("new_country", RiskLevel.Medium));

        // Device fingerprinting
        var deviceId = await _deviceFingerprintService.GetDeviceIdAsync(request.UserAgent, request.Fingerprint);
        if (!user.KnownDevices.Any(d => d.DeviceId == deviceId))
            factors.Add(new RiskFactor("new_device", RiskLevel.Medium));

        // Time-based analysis
        var currentHour = DateTime.UtcNow.Hour;
        var userTypicalHours = user.LoginHistory.Select(h => h.Timestamp.Hour).ToList();
        if (!userTypicalHours.Contains(currentHour) && userTypicalHours.Any())
            factors.Add(new RiskFactor("unusual_time", RiskLevel.Low));

        // Velocity checks
        var recentLogins = user.LoginHistory.Where(h => h.Timestamp > DateTime.UtcNow.AddMinutes(-5));
        if (recentLogins.Count() > 3)
            factors.Add(new RiskFactor("rapid_attempts", RiskLevel.High));

        // IP reputation
        var ipReputation = await _threatIntelligenceService.CheckIpReputationAsync(request.IpAddress);
        if (ipReputation.IsMalicious)
            factors.Add(new RiskFactor("malicious_ip", RiskLevel.High));

        return new RiskAssessment
        {
            RiskLevel = CalculateOverallRisk(factors),
            Factors = factors,
            Confidence = 0.85f
        };
    }
}
```

#### JWT Token Security
```csharp
public class SecureJwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ISecretRotationService _secretRotationService;
    private readonly ITokenBlacklistService _blacklistService;

    public async Task<TokenResponse> GenerateTokensAsync(User user, DeviceInfo deviceInfo)
    {
        // Use rotating secrets for enhanced security
        var currentSecret = await _secretRotationService.GetCurrentSecretAsync();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(currentSecret.Value));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Generate unique JTI (JWT ID) for token tracking
        var jti = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("username", user.Username),
            new Claim("role", user.Role.ToString()),
            new Claim("subscription", user.Subscription.Type.ToString()),
            new Claim("device_id", deviceInfo.DeviceId),
            new Claim("session_id", deviceInfo.SessionId)
        };

        // Short-lived access token (15 minutes)
        var accessToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        // Longer-lived refresh token (30 days) with additional security
        var refreshToken = await GenerateSecureRefreshToken(user.Id, deviceInfo, jti);

        // Store token metadata for security monitoring
        await StoreTokenMetadata(new TokenMetadata
        {
            Jti = jti,
            UserId = user.Id,
            DeviceId = deviceInfo.DeviceId,
            IpAddress = deviceInfo.IpAddress,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = accessToken.ValidTo,
            TokenType = "access"
        });

        return new TokenResponse
        {
            AccessToken = _tokenHandler.WriteToken(accessToken),
            RefreshToken = refreshToken,
            ExpiresAt = accessToken.ValidTo,
            TokenType = "Bearer"
        };
    }

    public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
    {
        try
        {
            // Check token blacklist first
            var jti = ExtractJti(token);
            if (await _blacklistService.IsBlacklistedAsync(jti))
                throw new SecurityTokenValidationException("Token has been revoked");

            // Get current and previous secrets for key rotation
            var secrets = await _secretRotationService.GetValidSecretsAsync();
            SecurityToken validatedToken = null;
            ClaimsPrincipal principal = null;

            foreach (var secret in secrets)
            {
                try
                {
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret.Value));
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = true,
                        ValidIssuer = _jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        RequireExpirationTime = true
                    };

                    principal = _tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                    break; // Successfully validated
                }
                catch (SecurityTokenException)
                {
                    continue; // Try next secret
                }
            }

            if (principal == null)
                throw new SecurityTokenValidationException("Invalid token signature");

            // Additional security checks
            await PerformAdditionalTokenValidation(principal, validatedToken as JwtSecurityToken);

            return principal;
        }
        catch (Exception ex)
        {
            await LogSecurityEvent("token_validation_failed", new { token = MaskToken(token), error = ex.Message });
            throw;
        }
    }

    private async Task PerformAdditionalTokenValidation(ClaimsPrincipal principal, JwtSecurityToken jwt)
    {
        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var deviceId = principal.FindFirst("device_id")?.Value;

        // Check if token is still valid in our database
        var tokenMetadata = await GetTokenMetadata(jti);
        if (tokenMetadata == null || tokenMetadata.IsRevoked)
            throw new SecurityTokenValidationException("Token has been revoked");

        // Validate device binding
        if (tokenMetadata.DeviceId != deviceId)
            throw new SecurityTokenValidationException("Token device mismatch");

        // Check for concurrent session limits
        var userActiveSessions = await GetActiveSessionCount(tokenMetadata.UserId);
        if (userActiveSessions > _securitySettings.MaxConcurrentSessions)
        {
            await RevokeOldestSession(tokenMetadata.UserId);
        }
    }
}
```

### Authorization Framework

#### Role-Based Access Control (RBAC)
```csharp
public enum UserRole
{
    User = 1,
    Premium = 2,
    Moderator = 4,
    ContentCreator = 8,
    Admin = 16,
    SuperAdmin = 32
}

public enum Permission
{
    // Problem permissions
    ViewBasicProblems = 1,
    ViewPremiumProblems = 2,
    CreateProblems = 4,
    EditProblems = 8,
    DeleteProblems = 16,
    
    // Submission permissions
    SubmitSolutions = 32,
    ViewOwnSubmissions = 64,
    ViewAllSubmissions = 128,
    
    // Contest permissions
    ParticipateContests = 256,
    CreateContests = 512,
    ModerateContests = 1024,
    
    // User management
    ViewUserProfiles = 2048,
    EditUserProfiles = 4096,
    BanUsers = 8192,
    
    // Analytics and reporting
    ViewBasicAnalytics = 16384,
    ViewAdvancedAnalytics = 32768,
    ViewSystemAnalytics = 65536
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute, IAuthorizationRequirement
{
    public Permission[] Permissions { get; }
    public bool RequireAll { get; set; } = false;

    public RequirePermissionAttribute(params Permission[] permissions)
    {
        Permissions = permissions;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<RequirePermissionAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequirePermissionAttribute requirement)
    {
        var user = context.User;
        if (!user.Identity.IsAuthenticated)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var userRole = Enum.Parse<UserRole>(user.FindFirst("role")?.Value ?? "User");
        var userPermissions = GetPermissionsForRole(userRole);

        var hasPermission = requirement.RequireAll
            ? requirement.Permissions.All(p => userPermissions.HasFlag(p))
            : requirement.Permissions.Any(p => userPermissions.HasFlag(p));

        if (hasPermission)
            context.Succeed(requirement);
        else
            context.Fail();

        return Task.CompletedTask;
    }

    private Permission GetPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.User => Permission.ViewBasicProblems | Permission.SubmitSolutions | Permission.ViewOwnSubmissions | Permission.ViewUserProfiles | Permission.ViewBasicAnalytics,
            
            UserRole.Premium => GetPermissionsForRole(UserRole.User) | Permission.ViewPremiumProblems | Permission.ParticipateContests,
            
            UserRole.Moderator => GetPermissionsForRole(UserRole.Premium) | Permission.ModerateContests | Permission.ViewAllSubmissions | Permission.EditUserProfiles,
            
            UserRole.ContentCreator => GetPermissionsForRole(UserRole.Premium) | Permission.CreateProblems | Permission.EditProblems | Permission.CreateContests,
            
            UserRole.Admin => GetPermissionsForRole(UserRole.ContentCreator) | GetPermissionsForRole(UserRole.Moderator) | Permission.DeleteProblems | Permission.BanUsers | Permission.ViewAdvancedAnalytics,
            
            UserRole.SuperAdmin => (Permission)(-1), // All permissions
            
            _ => Permission.ViewBasicProblems
        };
    }
}

// Usage in controllers
[ApiController]
[Route("api/[controller]")]
public class ProblemsController : ControllerBase
{
    [HttpGet("{id}")]
    [RequirePermission(Permission.ViewBasicProblems)]
    public async Task<IActionResult> GetProblem(string id)
    {
        var problem = await _problemService.GetByIdAsync(id);
        
        // Additional permission check for premium problems
        if (problem.IsPremium)
        {
            var userRole = Enum.Parse<UserRole>(User.FindFirst("role")?.Value ?? "User");
            var permissions = GetPermissionsForRole(userRole);
            
            if (!permissions.HasFlag(Permission.ViewPremiumProblems))
                return Forbid("Premium subscription required");
        }
        
        return Ok(problem);
    }

    [HttpPost]
    [RequirePermission(Permission.CreateProblems)]
    public async Task<IActionResult> CreateProblem([FromBody] CreateProblemRequest request)
    {
        // Implementation
    }
}
```

---

## 🛡️ Application Security

### Input Validation & Sanitization

#### Comprehensive Input Validation
```csharp
public class SecurityValidationService
{
    private readonly ILogger<SecurityValidationService> _logger;
    private readonly IConfiguration _configuration;

    public ValidationResult ValidateUserInput(string input, InputType type, ValidationContext context)
    {
        var result = new ValidationResult();

        // Basic sanitization
        input = SanitizeInput(input, type);

        // Length validation
        if (!ValidateLength(input, type))
        {
            result.AddError("Input length exceeds allowed limits");
            return result;
        }

        // XSS prevention
        if (ContainsPotentialXss(input))
        {
            result.AddError("Input contains potentially malicious content");
            await LogSecurityThreat("xss_attempt", context, input);
            return result;
        }

        // SQL injection prevention
        if (ContainsPotentialSqlInjection(input))
        {
            result.AddError("Input contains potentially malicious SQL patterns");
            await LogSecurityThreat("sql_injection_attempt", context, input);
            return result;
        }

        // Code injection prevention (for code submissions)
        if (type == InputType.Code && ContainsPotentialCodeInjection(input))
        {
            result.AddError("Code contains potentially dangerous operations");
            await LogSecurityThreat("code_injection_attempt", context, input);
            return result;
        }

        // Custom validation rules based on input type
        switch (type)
        {
            case InputType.Email:
                result = ValidateEmail(input);
                break;
            case InputType.Username:
                result = ValidateUsername(input);
                break;
            case InputType.Code:
                result = ValidateCodeSubmission(input, context);
                break;
            case InputType.ProblemDescription:
                result = ValidateProblemDescription(input);
                break;
        }

        return result;
    }

    private string SanitizeInput(string input, InputType type)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return type switch
        {
            InputType.Html => AntiXss.HtmlEncode(input),
            InputType.PlainText => input.Trim(),
            InputType.Code => input, // Don't sanitize code submissions
            InputType.Email => input.Trim().ToLowerInvariant(),
            InputType.Username => Regex.Replace(input.Trim(), @"[^\w\d_.-]", ""),
            _ => AntiXss.HtmlEncode(input.Trim())
        };
    }

    private bool ContainsPotentialXss(string input)
    {
        var xssPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"onload\s*=",
            @"onerror\s*=",
            @"onclick\s*=",
            @"<iframe\b",
            @"<object\b",
            @"<embed\b"
        };

        return xssPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline));
    }

    private bool ContainsPotentialSqlInjection(string input)
    {
        var sqlPatterns = new[]
        {
            @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|UPDATE|UNION( +ALL)?)\b)",
            @"(\b(AND|OR)\b.*(=|>|<|\bLIKE\b).*(\b(AND|OR)\b|\-\-|\#))",
            @"(\bEXEC\b(\s|\+)+(S|X)P\w+)",
            @"(\bSP_\w+)"
        };

        return sqlPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline));
    }

    private bool ContainsPotentialCodeInjection(string input)
    {
        var dangerousPatterns = new[]
        {
            @"\bexec\s*\(",           // exec() function calls
            @"\beval\s*\(",           // eval() function calls
            @"\bsystem\s*\(",         // system() calls
            @"\bshell_exec\s*\(",     // shell execution
            @"\bpassthru\s*\(",       // passthru calls
            @"\bfile_get_contents\s*\(",  // file access
            @"\bfopen\s*\(",          // file operations
            @"\b__import__\s*\(",     // Python import
            @"\bos\.",                // OS module access
            @"\bsubprocess\.",        // subprocess module
            @"\bRuntime\.getRuntime", // Java runtime
            @"\bProcess\.",           // Process class
            @"\bProcessBuilder\.",    // ProcessBuilder
            @"import\s+os",           // OS import
            @"from\s+os\s+import",    // OS import variations
        };

        return dangerousPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline));
    }

    private ValidationResult ValidateCodeSubmission(string code, ValidationContext context)
    {
        var result = new ValidationResult();

        // Check code length limits
        if (code.Length > _configuration.GetValue<int>("Security:MaxCodeLength", 10000))
        {
            result.AddError("Code submission exceeds maximum length");
            return result;
        }

        // Language-specific validation
        var language = context.Language?.ToLowerInvariant();
        switch (language)
        {
            case "csharp":
                result = ValidateCSharpCode(code);
                break;
            case "python":
                result = ValidatePythonCode(code);
                break;
            case "java":
                result = ValidateJavaCode(code);
                break;
            case "cpp":
                result = ValidateCppCode(code);
                break;
        }

        return result;
    }

    private ValidationResult ValidateCSharpCode(string code)
    {
        var result = new ValidationResult();
        var dangerousNamespaces = new[]
        {
            "System.IO",
            "System.Net",
            "System.Diagnostics",
            "System.Management",
            "System.Runtime.InteropServices",
            "System.Security",
            "System.Web"
        };

        foreach (var ns in dangerousNamespaces)
        {
            if (code.Contains($"using {ns}") || code.Contains($"System.{ns.Split('.').Last()}"))
            {
                result.AddError($"Usage of {ns} namespace is not allowed");
            }
        }

        return result;
    }
}
```

### Rate Limiting & DDoS Protection

#### Advanced Rate Limiting
```csharp
public class AdvancedRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<AdvancedRateLimitingService> _logger;

    public async Task<RateLimitResult> CheckRateLimitAsync(RateLimitRequest request)
    {
        var rules = await GetRateLimitRules(request.Endpoint, request.UserRole);
        var results = new List<RateLimitCheckResult>();

        foreach (var rule in rules)
        {
            var result = await CheckIndividualRule(request, rule);
            results.Add(result);

            if (!result.Allowed)
            {
                await LogRateLimitViolation(request, rule, result);
                return new RateLimitResult
                {
                    Allowed = false,
                    RetryAfter = result.RetryAfter,
                    Rule = rule,
                    RemainingRequests = 0
                };
            }
        }

        var mostRestrictive = results.OrderBy(r => r.RemainingRequests).First();
        return new RateLimitResult
        {
            Allowed = true,
            RemainingRequests = mostRestrictive.RemainingRequests,
            ResetTime = mostRestrictive.ResetTime
        };
    }

    private async Task<RateLimitCheckResult> CheckIndividualRule(RateLimitRequest request, RateLimitRule rule)
    {
        var key = GenerateRateLimitKey(request, rule);
        var window = GetTimeWindow(rule.WindowSize);
        var currentCount = await GetCurrentCount(key, window);

        if (currentCount >= rule.MaxRequests)
        {
            return new RateLimitCheckResult
            {
                Allowed = false,
                RetryAfter = window.End - DateTime.UtcNow,
                RemainingRequests = 0,
                ResetTime = window.End
            };
        }

        // Increment counter
        await IncrementCounter(key, window);

        return new RateLimitCheckResult
        {
            Allowed = true,
            RemainingRequests = rule.MaxRequests - currentCount - 1,
            ResetTime = window.End
        };
    }

    private string GenerateRateLimitKey(RateLimitRequest request, RateLimitRule rule)
    {
        return rule.Scope switch
        {
            RateLimitScope.Global => $"rate_limit:global:{rule.Endpoint}:{rule.WindowSize}",
            RateLimitScope.PerUser => $"rate_limit:user:{request.UserId}:{rule.Endpoint}:{rule.WindowSize}",
            RateLimitScope.PerIP => $"rate_limit:ip:{request.IpAddress}:{rule.Endpoint}:{rule.WindowSize}",
            RateLimitScope.PerUserAndIP => $"rate_limit:user_ip:{request.UserId}:{request.IpAddress}:{rule.Endpoint}:{rule.WindowSize}",
            _ => throw new ArgumentException($"Unknown rate limit scope: {rule.Scope}")
        };
    }

    // Sliding window implementation
    private async Task<int> GetCurrentCount(string key, TimeWindow window)
    {
        var script = @"
            local key = KEYS[1]
            local window_start = ARGV[1]
            local window_end = ARGV[2]
            
            -- Remove old entries
            redis.call('ZREMRANGEBYSCORE', key, '-inf', window_start)
            
            -- Count current entries
            return redis.call('ZCARD', key)
        ";

        var result = await _cache.ExecuteScriptAsync(script, 
            new[] { key }, 
            new[] { window.Start.Ticks.ToString(), window.End.Ticks.ToString() });

        return (int)result;
    }

    private async Task IncrementCounter(string key, TimeWindow window)
    {
        var script = @"
            local key = KEYS[1]
            local timestamp = ARGV[1]
            local ttl = ARGV[2]
            
            -- Add current timestamp
            redis.call('ZADD', key, timestamp, timestamp)
            
            -- Set expiration
            redis.call('EXPIRE', key, ttl)
        ";

        await _cache.ExecuteScriptAsync(script,
            new[] { key },
            new[] { DateTime.UtcNow.Ticks.ToString(), window.Duration.TotalSeconds.ToString() });
    }
}

// DDoS Protection Middleware
public class DDoSProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DDoSProtectionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIP = GetClientIP(context);
        var requestFingerprint = GenerateRequestFingerprint(context);

        // Check for suspicious patterns
        if (await IsSuspiciousTraffic(clientIP, requestFingerprint))
        {
            await HandleSuspiciousRequest(context, clientIP);
            return;
        }

        // Track request patterns
        await TrackRequest(clientIP, requestFingerprint);

        await _next(context);
    }

    private async Task<bool> IsSuspiciousTraffic(string clientIP, string fingerprint)
    {
        var checks = new[]
        {
            CheckRequestFrequency(clientIP),
            CheckRequestPatterns(clientIP, fingerprint),
            CheckRequestSize(fingerprint),
            CheckGeographicAnomaly(clientIP),
            CheckUserAgentPatterns(fingerprint)
        };

        var suspiciousCount = (await Task.WhenAll(checks)).Count(x => x);
        return suspiciousCount >= 2; // Multiple suspicious indicators
    }

    private async Task<bool> CheckRequestFrequency(string clientIP)
    {
        var key = $"request_freq:{clientIP}";
        var count = await GetRequestCount(key, TimeSpan.FromMinutes(1));
        
        // More than 100 requests per minute is suspicious
        return count > 100;
    }

    private async Task<bool> CheckRequestPatterns(string clientIP, string fingerprint)
    {
        // Check for repeating patterns that might indicate automated attacks
        var recentFingerprints = await GetRecentFingerprints(clientIP, TimeSpan.FromMinutes(5));
        var uniqueFingerprints = recentFingerprints.Distinct().Count();
        var totalFingerprints = recentFingerprints.Count();

        // If more than 80% of requests have identical fingerprints, it's suspicious
        return totalFingerprints > 10 && (double)uniqueFingerprints / totalFingerprints < 0.2;
    }

    private async Task HandleSuspiciousRequest(HttpContext context, string clientIP)
    {
        await LogSecurityEvent("ddos_protection_triggered", new { 
            clientIP, 
            userAgent = context.Request.Headers["User-Agent"].ToString(),
            path = context.Request.Path,
            method = context.Request.Method
        });

        // Implement progressive response:
        // 1. Rate limiting
        // 2. CAPTCHA challenge
        // 3. Temporary IP blocking
        // 4. Permanent blocking for repeated offenses

        var blockDuration = await GetBlockDuration(clientIP);
        if (blockDuration > TimeSpan.Zero)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("Retry-After", blockDuration.TotalSeconds.ToString());
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }

        // Challenge with CAPTCHA
        await ChallengeWithCaptcha(context);
    }
}
```

---

## 🔒 Code Execution Security

### Sandboxed Execution Environment

#### Secure Docker Container Configuration
```csharp
public class SecureDockerExecutionService : ICodeExecutionService
{
    private readonly DockerClient _dockerClient;
    private readonly ISecurityAuditService _auditService;

    public async Task<ExecutionResult> ExecuteCodeSecurelyAsync(CodeExecutionRequest request)
    {
        // Pre-execution security checks
        await ValidateCodeSecurity(request);
        
        var containerId = string.Empty;
        try
        {
            // Create highly restricted container
            var containerConfig = CreateSecureContainerConfig(request);
            var container = await _dockerClient.Containers.CreateContainerAsync(containerConfig);
            containerId = container.ID;

            // Set up security monitoring
            var monitoringTask = MonitorContainerSecurity(containerId);

            // Execute with strict timeout
            var executionTask = ExecuteInContainer(containerId, request);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(request.TimeLimit));

            var completedTask = await Task.WhenAny(executionTask, timeoutTask, monitoringTask);

            if (completedTask == timeoutTask)
            {
                await _auditService.LogSecurityEvent("execution_timeout", new { containerId, request.UserId });
                throw new TimeoutException("Code execution timed out");
            }

            if (completedTask == monitoringTask)
            {
                await _auditService.LogSecurityEvent("security_violation_detected", new { containerId, request.UserId });
                throw new SecurityException("Security violation detected during execution");
            }

            return await executionTask;
        }
        finally
        {
            // Always cleanup, even if execution fails
            await CleanupContainer(containerId);
        }
    }

    private CreateContainerParameters CreateSecureContainerConfig(CodeExecutionRequest request)
    {
        var config = new CreateContainerParameters
        {
            Image = GetSecureImageForLanguage(request.Language),
            
            // Security: No network access
            NetworkMode = "none",
            
            // Security: Run as non-privileged user
            User = "runner:runner", // UID 1000:1000
            
            // Security: Read-only root filesystem
            HostConfig = new HostConfig
            {
                ReadonlyRootfs = true,
                
                // Resource limits
                Memory = request.MemoryLimit * 1024 * 1024, // Convert MB to bytes
                MemorySwap = request.MemoryLimit * 1024 * 1024, // No swap
                CpuQuota = 50000, // 50% of one CPU
                CpuPeriod = 100000,
                
                // Process limits
                PidsLimit = 64,
                
                // Security options
                SecurityOpt = new[]
                {
                    "no-new-privileges", // Prevent privilege escalation
                    "seccomp=unconfined" // Use custom seccomp profile
                },
                
                // Capabilities - drop all, add only essential ones
                CapDrop = new[] { "ALL" },
                CapAdd = new[] { "DAC_OVERRIDE" }, // Minimal required capabilities
                
                // Temporary filesystems (writable areas)
                Tmpfs = new Dictionary<string, string>
                {
                    { "/tmp", "rw,noexec,nosuid,size=50m" },
                    { "/var/tmp", "rw,noexec,nosuid,size=10m" },
                    { "/home/runner", "rw,noexec,nosuid,size=100m" }
                },
                
                // Resource monitoring
                OomKillDisable = false, // Allow OOM killer
                
                // Ulimits for additional resource control
                Ulimits = new List<Ulimit>
                {
                    new() { Name = "nproc", Soft = 32, Hard = 32 },     // Process limit
                    new() { Name = "nofile", Soft = 64, Hard = 64 },   // File descriptor limit
                    new() { Name = "fsize", Soft = 10485760, Hard = 10485760 }, // 10MB file size limit
                    new() { Name = "cpu", Soft = 5, Hard = 5 }         // 5 seconds CPU time
                }
            },
            
            // Environment variables (minimal)
            Env = new[]
            {
                "HOME=/home/runner",
                "USER=runner",
                "PATH=/usr/local/bin:/usr/bin:/bin",
                $"TIMEOUT={request.TimeLimit}",
                $"LANG={request.Language}"
            },
            
            // Working directory
            WorkingDir = "/home/runner",
            
            // Command to execute
            Cmd = GetSecureExecutionCommand(request),
            
            // Labels for tracking
            Labels = new Dictionary<string, string>
            {
                { "dsagrind.execution.user", request.UserId },
                { "dsagrind.execution.problem", request.ProblemId },
                { "dsagrind.execution.language", request.Language },
                { "dsagrind.security.level", "high" }
            }
        };

        return config;
    }

    private async Task<ExecutionResult> ExecuteInContainer(string containerId, CodeExecutionRequest request)
    {
        // Start the container
        await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

        // Wait for completion with timeout
        var result = await _dockerClient.Containers.WaitContainerAsync(containerId, CancellationToken.None);

        // Get execution statistics
        var stats = await _dockerClient.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters { OneShot = true });

        // Get logs (stdout/stderr)
        var logs = await _dockerClient.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Timestamps = false
        });

        var (stdout, stderr) = ParseDockerLogs(logs);

        return new ExecutionResult
        {
            ExitCode = (int)result.StatusCode,
            Output = stdout,
            ErrorOutput = stderr,
            ExecutionTime = TimeSpan.FromMilliseconds(stats.Read.Subtract(stats.PreRead).TotalMilliseconds),
            MemoryUsed = stats.MemoryStats.Usage,
            Success = result.StatusCode == 0 && string.IsNullOrEmpty(stderr)
        };
    }

    private async Task MonitorContainerSecurity(string containerId)
    {
        var monitoringInterval = TimeSpan.FromSeconds(1);
        
        while (true)
        {
            try
            {
                var container = await _dockerClient.Containers.InspectContainerAsync(containerId);
                
                // Check if container is still running
                if (!container.State.Running)
                    break;

                // Monitor resource usage
                var stats = await _dockerClient.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters { OneShot = true });
                
                // Check for suspicious behavior
                await CheckForSuspiciousBehavior(containerId, stats);
                
                await Task.Delay(monitoringInterval);
            }
            catch (DockerContainerNotFoundException)
            {
                break; // Container was removed
            }
        }
    }

    private async Task CheckForSuspiciousBehavior(string containerId, ContainerStatsResponse stats)
    {
        // Check for excessive CPU usage
        var cpuUsage = CalculateCpuUsage(stats);
        if (cpuUsage > 0.9) // More than 90% CPU usage
        {
            await _auditService.LogSecurityEvent("excessive_cpu_usage", new { containerId, cpuUsage });
        }

        // Check for memory usage spikes
        var memoryUsage = (double)stats.MemoryStats.Usage / stats.MemoryStats.Limit;
        if (memoryUsage > 0.95) // More than 95% memory usage
        {
            await _auditService.LogSecurityEvent("excessive_memory_usage", new { containerId, memoryUsage });
        }

        // Check for too many processes
        if (stats.PidsStats.Current > 32)
        {
            await _auditService.LogSecurityEvent("too_many_processes", new { containerId, processCount = stats.PidsStats.Current });
        }
    }

    private string GetSecureImageForLanguage(string language)
    {
        // Use custom-built, security-hardened images
        return language.ToLowerInvariant() switch
        {
            "csharp" => "dsagrind/dotnet-secure:8.0-runtime",
            "python" => "dsagrind/python-secure:3.11-slim",
            "java" => "dsagrind/openjdk-secure:17-jre",
            "cpp" => "dsagrind/gcc-secure:latest",
            "javascript" => "dsagrind/node-secure:18-alpine",
            _ => throw new NotSupportedException($"Language {language} is not supported")
        };
    }

    private List<string> GetSecureExecutionCommand(CodeExecutionRequest request)
    {
        // All execution goes through a security wrapper script
        return new List<string>
        {
            "/usr/local/bin/secure-runner.sh",
            request.Language,
            Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Code)),
            Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Input ?? string.Empty))
        };
    }
}

// Security wrapper script (secure-runner.sh) content:
/*
#!/bin/bash
set -euo pipefail

LANGUAGE="$1"
CODE_B64="$2"  
INPUT_B64="$3"

# Decode inputs
CODE=$(echo "$CODE_B64" | base64 -d)
INPUT=$(echo "$INPUT_B64" | base64 -d)

# Set strict ulimits
ulimit -v 131072    # 128MB virtual memory
ulimit -f 10240     # 10MB file size
ulimit -u 32        # 32 processes
ulimit -t 5         # 5 seconds CPU time

# Create temporary working directory
WORK_DIR=$(mktemp -d)
cd "$WORK_DIR"

# Language-specific execution with additional security
case "$LANGUAGE" in
    "csharp")
        echo "$CODE" > Program.cs
        # Compile with security flags
        dotnet new console -f net8.0 --no-restore
        cp Program.cs .
        dotnet run --no-restore < <(echo "$INPUT")
        ;;
    "python")
        echo "$CODE" > solution.py
        # Run with restricted modules
        python3 -B -S -s solution.py < <(echo "$INPUT")
        ;;
    "java")
        echo "$CODE" > Solution.java
        javac Solution.java
        java -Djava.security.manager -Djava.security.policy==/dev/null Solution < <(echo "$INPUT")
        ;;
    *)
        echo "Unsupported language: $LANGUAGE" >&2
        exit 1
        ;;
esac
*/
```

### Static Code Analysis
```csharp
public class StaticCodeAnalysisService
{
    public async Task<SecurityAnalysisResult> AnalyzeCodeSecurity(string code, string language)
    {
        var result = new SecurityAnalysisResult();
        
        switch (language.ToLowerInvariant())
        {
            case "csharp":
                result = await AnalyzeCSharpSecurity(code);
                break;
            case "python":
                result = await AnalyzePythonSecurity(code);
                break;
            case "java":
                result = await AnalyzeJavaSecurity(code);
                break;
        }

        return result;
    }

    private async Task<SecurityAnalysisResult> AnalyzeCSharpSecurity(string code)
    {
        var result = new SecurityAnalysisResult();
        var issues = new List<SecurityIssue>();

        // Parse the code using Roslyn
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = await syntaxTree.GetRootAsync();

        // Check for dangerous using statements
        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
        foreach (var usingDirective in usingDirectives)
        {
            var namespaceName = usingDirective.Name.ToString();
            if (IsDangerousNamespace(namespaceName))
            {
                issues.Add(new SecurityIssue
                {
                    Type = SecurityIssueType.DangerousNamespace,
                    Severity = SecuritySeverity.High,
                    Message = $"Usage of potentially dangerous namespace: {namespaceName}",
                    Line = usingDirective.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

        // Check for reflection usage
        var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();
        foreach (var identifier in identifiers)
        {
            if (IsReflectionMethod(identifier.Identifier.ValueText))
            {
                issues.Add(new SecurityIssue
                {
                    Type = SecurityIssueType.ReflectionUsage,
                    Severity = SecuritySeverity.Medium,
                    Message = $"Reflection usage detected: {identifier.Identifier.ValueText}",
                    Line = identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

        // Check for unsafe code blocks
        var unsafeStatements = root.DescendantNodes().OfType<UnsafeStatementSyntax>();
        foreach (var unsafeStatement in unsafeStatements)
        {
            issues.Add(new SecurityIssue
            {
                Type = SecurityIssueType.UnsafeCode,
                Severity = SecuritySeverity.High,
                Message = "Unsafe code block detected",
                Line = unsafeStatement.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            });
        }

        result.Issues = issues;
        result.OverallSeverity = issues.Any() ? issues.Max(i => i.Severity) : SecuritySeverity.None;
        
        return result;
    }

    private bool IsDangerousNamespace(string namespaceName)
    {
        var dangerousNamespaces = new[]
        {
            "System.IO",
            "System.Net",
            "System.Diagnostics", 
            "System.Management",
            "System.Runtime.InteropServices",
            "System.Security.Principal",
            "System.Web",
            "Microsoft.Win32"
        };

        return dangerousNamespaces.Any(ns => namespaceName.StartsWith(ns));
    }

    private bool IsReflectionMethod(string methodName)
    {
        var reflectionMethods = new[]
        {
            "GetType",
            "typeof",
            "Activator.CreateInstance",
            "Assembly.Load",
            "Assembly.LoadFrom",
            "Type.GetType",
            "MethodInfo.Invoke"
        };

        return reflectionMethods.Contains(methodName);
    }
}
```

---

## 💾 Data Protection & Privacy

### Encryption Strategy

#### Data Encryption at Rest
```csharp
public class DataEncryptionService : IDataEncryptionService
{
    private readonly IKeyManagementService _keyService;
    private readonly ILogger<DataEncryptionService> _logger;

    public async Task<string> EncryptSensitiveDataAsync(string data, string dataType, string userId)
    {
        try
        {
            // Get user-specific encryption key
            var encryptionKey = await _keyService.GetUserEncryptionKeyAsync(userId);
            
            // Add data type salt for additional security
            var salt = await _keyService.GetDataTypeSaltAsync(dataType);
            
            using var aes = Aes.Create();
            aes.Key = encryptionKey;
            aes.IV = GenerateRandomIV();
            aes.Mode = CipherMode.GCM; // Galois/Counter Mode for authenticated encryption
            
            using var encryptor = aes.CreateEncryptor();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var saltedData = CombineBytes(salt, dataBytes);
            
            var encryptedData = encryptor.TransformFinalBlock(saltedData, 0, saltedData.Length);
            
            // Combine IV + encrypted data + authentication tag
            var result = CombineBytes(aes.IV, encryptedData);
            
            await LogEncryptionEvent(userId, dataType, "encrypted");
            
            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Failed to encrypt data for user {UserId}", userId);
            throw new EncryptionException("Failed to encrypt sensitive data", ex);
        }
    }

    public async Task<string> DecryptSensitiveDataAsync(string encryptedData, string dataType, string userId)
    {
        try
        {
            var encryptionKey = await _keyService.GetUserEncryptionKeyAsync(userId);
            var salt = await _keyService.GetDataTypeSaltAsync(dataType);
            
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            
            using var aes = Aes.Create();
            aes.Key = encryptionKey;
            aes.Mode = CipherMode.GCM;
            
            // Extract IV and encrypted data
            var iv = new byte[16];
            var ciphertext = new byte[encryptedBytes.Length - 16];
            Array.Copy(encryptedBytes, 0, iv, 0, 16);
            Array.Copy(encryptedBytes, 16, ciphertext, 0, ciphertext.Length);
            
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            
            // Remove salt
            var dataBytes = new byte[decryptedBytes.Length - salt.Length];
            Array.Copy(decryptedBytes, salt.Length, dataBytes, 0, dataBytes.Length);
            
            await LogEncryptionEvent(userId, dataType, "decrypted");
            
            return Encoding.UTF8.GetString(dataBytes);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Failed to decrypt data for user {UserId}", userId);
            throw new DecryptionException("Failed to decrypt sensitive data", ex);
        }
    }
}

// Key Management Service
public class KeyManagementService : IKeyManagementService
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;

    public async Task<byte[]> GetUserEncryptionKeyAsync(string userId)
    {
        var cacheKey = $"encryption_key:{userId}";
        var cachedKey = await _cache.GetAsync(cacheKey);
        
        if (cachedKey != null)
            return cachedKey;

        // Generate user-specific key using PBKDF2
        var masterKey = _configuration["Encryption:MasterKey"];
        var userSalt = Encoding.UTF8.GetBytes($"user:{userId}");
        
        using var pbkdf2 = new Rfc2898DeriveBytes(masterKey, userSalt, 100000, HashAlgorithmName.SHA256);
        var userKey = pbkdf2.GetBytes(32); // 256-bit key
        
        // Cache the key for performance (with short TTL for security)
        await _cache.SetAsync(cacheKey, userKey, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });
        
        return userKey;
    }

    public async Task RotateEncryptionKeysAsync()
    {
        // Implement key rotation strategy
        var users = await GetAllUsersAsync();
        
        foreach (var user in users)
        {
            try
            {
                await RotateUserKeyAsync(user.Id);
            }
            catch (Exception ex)
            {
                await LogKeyRotationError(user.Id, ex);
            }
        }
    }

    private async Task RotateUserKeyAsync(string userId)
    {
        // Get current encrypted data
        var encryptedData = await GetUserEncryptedDataAsync(userId);
        
        // Decrypt with old key
        var oldKey = await GetUserEncryptionKeyAsync(userId);
        var decryptedData = await DecryptAllUserDataAsync(encryptedData, oldKey);
        
        // Generate new key
        var newKey = GenerateNewUserKey(userId);
        await StoreNewUserKeyAsync(userId, newKey);
        
        // Re-encrypt with new key
        var reencryptedData = await EncryptAllUserDataAsync(decryptedData, newKey);
        await StoreReencryptedDataAsync(userId, reencryptedData);
        
        // Clear old key from cache
        await _cache.RemoveAsync($"encryption_key:{userId}");
        
        await LogKeyRotationSuccess(userId);
    }
}
```

### Privacy Compliance (GDPR/CCPA)

#### Data Privacy Management
```csharp
public class DataPrivacyService : IDataPrivacyService
{
    public async Task<DataPortabilityResult> ExportUserDataAsync(string userId, ExportFormat format)
    {
        var userData = await CollectAllUserDataAsync(userId);
        
        // Validate user consent and legal basis
        await ValidateExportRequest(userId);
        
        var exportData = new UserDataExport
        {
            UserId = userId,
            ExportDate = DateTime.UtcNow,
            Format = format,
            Data = new Dictionary<string, object>
            {
                ["profile"] = userData.Profile,
                ["submissions"] = userData.Submissions.Select(s => new
                {
                    s.Id,
                    s.ProblemId,
                    s.Code,
                    s.Language,
                    s.Status,
                    s.SubmittedAt,
                    s.ExecutionMetrics
                }),
                ["analytics"] = userData.Analytics,
                ["preferences"] = userData.Preferences,
                ["subscription"] = userData.Subscription
            }
        };

        // Remove sensitive internal data
        exportData = await SanitizeExportData(exportData);
        
        var exportFile = await GenerateExportFile(exportData, format);
        
        await LogDataExport(userId, format);
        
        return new DataPortabilityResult
        {
            ExportId = Guid.NewGuid().ToString(),
            FileName = $"dsagrind_data_export_{userId}_{DateTime.UtcNow:yyyyMMdd}.{format.ToString().ToLowerInvariant()}",
            FileContent = exportFile,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<DataDeletionResult> DeleteUserDataAsync(string userId, DataDeletionRequest request)
    {
        // Validate deletion request
        await ValidateDeletionRequest(userId, request);
        
        var deletionPlan = await CreateDataDeletionPlan(userId, request);
        
        try
        {
            // Phase 1: Soft delete (mark as deleted but keep for legal retention)
            await SoftDeleteUserData(userId, deletionPlan);
            
            // Phase 2: Anonymize data that needs to be retained for analytics
            await AnonymizeRetainedData(userId, deletionPlan);
            
            // Phase 3: Hard delete after retention period (scheduled job)
            await ScheduleHardDeletion(userId, deletionPlan);
            
            await LogDataDeletion(userId, deletionPlan);
            
            return new DataDeletionResult
            {
                DeletionId = Guid.NewGuid().ToString(),
                Status = DeletionStatus.Completed,
                DeletedCategories = deletionPlan.CategoriesToDelete,
                RetainedCategories = deletionPlan.CategoriesToRetain,
                HardDeletionScheduledDate = DateTime.UtcNow.AddDays(deletionPlan.RetentionDays)
            };
        }
        catch (Exception ex)
        {
            await LogDataDeletionError(userId, ex);
            throw new DataDeletionException("Failed to delete user data", ex);
        }
    }

    private async Task<DataDeletionPlan> CreateDataDeletionPlan(string userId, DataDeletionRequest request)
    {
        var plan = new DataDeletionPlan { UserId = userId };
        
        // Determine what can be deleted immediately vs what needs retention
        var userAccount = await GetUserAccountAsync(userId);
        
        // Check legal retention requirements
        if (userAccount.HasActiveSubscription)
        {
            plan.CategoriesToRetain.Add("payment_records");
            plan.RetentionReasons.Add("active_subscription");
        }
        
        if (userAccount.HasRecentDisputes)
        {
            plan.CategoriesToRetain.Add("transaction_history");
            plan.RetentionReasons.Add("dispute_resolution");
        }
        
        // Check business retention requirements
        if (request.DeleteSubmissions == false)
        {
            plan.CategoriesToRetain.Add("submissions");
            plan.RetentionReasons.Add("user_preference");
        }
        
        // Default categories to delete
        plan.CategoriesToDelete.AddRange(new[]
        {
            "personal_information",
            "preferences",
            "analytics_data",
            "session_data",
            "device_information"
        });
        
        // Remove overlaps
        plan.CategoriesToDelete.RemoveAll(c => plan.CategoriesToRetain.Contains(c));
        
        return plan;
    }

    private async Task AnonymizeRetainedData(string userId, DataDeletionPlan plan)
    {
        foreach (var category in plan.CategoriesToRetain)
        {
            switch (category)
            {
                case "submissions":
                    await AnonymizeSubmissions(userId);
                    break;
                case "payment_records":
                    await AnonymizePaymentRecords(userId);
                    break;
                case "analytics_data":
                    await AnonymizeAnalyticsData(userId);
                    break;
            }
        }
    }

    private async Task AnonymizeSubmissions(string userId)
    {
        var anonymousId = $"anon_{Guid.NewGuid():N}";
        
        // Replace user ID with anonymous ID in submissions
        var update = Builders<Submission>.Update
            .Set(s => s.UserId, anonymousId)
            .Set(s => s.IsAnonymized, true)
            .Set(s => s.AnonymizedAt, DateTime.UtcNow);
            
        await _submissionRepository.UpdateManyAsync(
            s => s.UserId == userId,
            update
        );
    }

    public async Task<ConsentManagementResult> UpdateDataProcessingConsentAsync(string userId, ConsentUpdateRequest request)
    {
        var currentConsent = await GetUserConsentAsync(userId);
        
        var newConsent = new UserConsent
        {
            UserId = userId,
            MarketingEmails = request.MarketingEmails ?? currentConsent.MarketingEmails,
            AnalyticsTracking = request.AnalyticsTracking ?? currentConsent.AnalyticsTracking,
            PersonalizedContent = request.PersonalizedContent ?? currentConsent.PersonalizedContent,
            ThirdPartySharing = request.ThirdPartySharing ?? currentConsent.ThirdPartySharing,
            UpdatedAt = DateTime.UtcNow,
            ConsentVersion = await GetLatestConsentVersionAsync()
        };
        
        await StoreUserConsentAsync(newConsent);
        
        // Apply consent changes immediately
        await ApplyConsentChanges(userId, currentConsent, newConsent);
        
        return new ConsentManagementResult
        {
            Success = true,
            UpdatedConsent = newConsent,
            ChangesApplied = await GetConsentChanges(currentConsent, newConsent)
        };
    }

    private async Task ApplyConsentChanges(string userId, UserConsent oldConsent, UserConsent newConsent)
    {
        // Stop marketing emails if consent withdrawn
        if (oldConsent.MarketingEmails && !newConsent.MarketingEmails)
        {
            await _emailService.UnsubscribeFromMarketingAsync(userId);
        }
        
        // Stop analytics tracking if consent withdrawn
        if (oldConsent.AnalyticsTracking && !newConsent.AnalyticsTracking)
        {
            await _analyticsService.StopTrackingUserAsync(userId);
            await AnonymizeExistingAnalyticsDataAsync(userId);
        }
        
        // Remove personalized data if consent withdrawn
        if (oldConsent.PersonalizedContent && !newConsent.PersonalizedContent)
        {
            await _personalizationService.ClearUserDataAsync(userId);
        }
    }
}
```

This comprehensive security design provides multiple layers of protection covering all aspects of the DSAGrind platform, from user authentication to code execution security, ensuring the highest standards of security and privacy protection.