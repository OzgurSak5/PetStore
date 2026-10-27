using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Interfaces;
using PetStore.Domain.Services;
using PetStore.Infrastructure.Data;
using PetStore.Infrastructure.Repositories;
using PetStore.Api.Middleware;
using PetStore.Domain.Settings;
using PetStore.Infrastructure.Storage;
using PetStore.Api.BackgroundServices;
using PetStore.Infrastructure.Security;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PetStore.Api.Swagger;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PetStoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

var rateLimitSettings = builder.Configuration.GetSection("RateLimit").Get<RateLimitSettings>()
    ?? new RateLimitSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Run: dotnet user-secrets set \"Jwt:Key\" \"<key>\" --project PetStore.Api");
}

if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes (HS256 requirement).");
}

builder.Services.Configure<OrderSettings>(
    builder.Configuration.GetSection("OrderSettings"));

builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorageSettings"));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimit"));

builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IBreedRepository, BreedRepository>();
builder.Services.AddScoped<IBreedService, BreedService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IPetPhotoRepository, PetPhotoRepository>();
builder.Services.AddScoped<IPetPhotoService, PetPhotoService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ILoginAttemptLimiter, LoginAttemptLimiter>();
builder.Services.AddHostedService<ReservationCleanupService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            NameClaimType = "sub",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login-ip", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = rateLimitSettings.IpPermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitSettings.IpWindowMinutes),
                SegmentsPerWindow = 5,
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? (int)retryAfter.TotalSeconds
            : rateLimitSettings.IpWindowMinutes * 60;

        context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");

        logger.LogWarning("Request blocked by IP rate limit. IP: {Ip}",
            context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        await Results.Problem(
            title: "Too Many Requests",
            detail: "Too many requests. Please try again later.",
            statusCode: StatusCodes.Status429TooManyRequests
        ).ExecuteAsync(context.HttpContext);
    };
});

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Paste the access token here (without the word Bearer)."
    });

    options.DocumentFilter<BearerSecurityDocumentFilter>();
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PetStoreDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DatabaseSeeder.SeedAsync(context, passwordHasher);
}

app.Run();