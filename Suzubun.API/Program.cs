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

// 1. Cấu hình CORS cực kỳ thoải mái để tránh lỗi Preflight trên IIS/Somee
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Configuration.AddJsonFile("appsettings.Secret.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddEndpointsApiExplorer();

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
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] {} } });
});

builder.Services.Configure<AppOptions>(builder.Configuration);
builder.Services.Configure<SupabaseConfig>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var supabaseUrl = builder.Configuration["ConnectionStrings:SupabaseUrl"] ?? "";
var supabaseKey = builder.Configuration["ConnectionStrings:SupabaseKey"] ?? "";
var supabaseAnonKey = builder.Configuration["ConnectionStrings:SupabaseAnonKey"];

builder.Services.AddScoped<Supabase.Client>(provider => 
    new Supabase.Client(supabaseUrl, supabaseAnonKey, new Supabase.SupabaseOptions { AutoRefreshToken = true, AutoConnectRealtime = true }));

builder.Services.AddKeyedSingleton<Supabase.Client>("AdminClient", (provider, key) => 
    new Supabase.Client(supabaseUrl, supabaseKey, new Supabase.SupabaseOptions { AutoRefreshToken = true }));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IJapaneseService, JapaneseService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Nếu Somee chặn outbound, Authority sẽ fail. 
        // Trong trường hợp đó, bạn nên dùng TokenValidationParameters với Key cứng (Symmetric)
        options.Authority = $"{supabaseUrl}/auth/v1";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { $"{supabaseUrl}/auth/v1", "supabase" },
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateLifetime = true
        };
    });

var app = builder.Build();

// Thêm Endpoint Ping để test
app.MapGet("/api/ping", () => Results.Ok(new { message = "API Suzubun is alive!", time = DateTime.Now }));

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Suzubun API v1");
    c.RoutePrefix = "swagger";
});

// Thứ tự Middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
