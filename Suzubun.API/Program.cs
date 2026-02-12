using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Supabase;
using Suzubun.Service.Interfaces;
using Suzubun.Service.Models;
using Suzubun.Service.Services;
using Suzubun.API.Validators;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load secrets
builder.Configuration.AddJsonFile("appsettings.Secret.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
// ... (Swagger config remains same)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Suzubun API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Bind Configuration
builder.Services.Configure<AppOptions>(builder.Configuration);
builder.Services.Configure<SupabaseConfig>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Supabase Configuration
var supabaseUrl = builder.Configuration["ConnectionStrings:SupabaseUrl"] ?? throw new InvalidOperationException("SupabaseUrl is missing.");
var supabaseKey = builder.Configuration["ConnectionStrings:SupabaseKey"] ?? throw new InvalidOperationException("SupabaseKey is missing.");
var supabaseAnonKey = builder.Configuration["ConnectionStrings:SupabaseAnonKey"];

// Client for User-facing operations (respects RLS)
builder.Services.AddScoped<Supabase.Client>(provider => 
{
    return new Supabase.Client(supabaseUrl, supabaseAnonKey, new Supabase.SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true
    });
});

// Administrative Client (bypasses RLS for system tasks)
builder.Services.AddKeyedSingleton<Supabase.Client>("AdminClient", (provider, key) => 
{
    return new Supabase.Client(supabaseUrl, supabaseKey, new Supabase.SupabaseOptions
    {
        AutoRefreshToken = true
    });
});

// Register Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IJapaneseService, JapaneseService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();

// JWT Authentication Configuration - Using Authority for automatic JWK discovery
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{supabaseUrl}/auth/v1";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { $"{supabaseUrl}/auth/v1", "supabase" }, // Chấp nhận cả URL và định danh "supabase"
            ValidateAudience = true,
            ValidAudience = "authenticated", // Audience mặc định cho user đã đăng nhập
            ValidateLifetime = true
        };
    });

var app = builder.Build();

// Enable Swagger in all environments (including Production/Somee)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Suzubun API v1");
    c.RoutePrefix = "swagger"; // Giữ nguyên /swagger/index.html
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
