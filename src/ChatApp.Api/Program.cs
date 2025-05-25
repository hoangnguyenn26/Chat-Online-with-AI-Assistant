using ChatApp.Api.Middleware;
using ChatApp.Application.Mappings;
using ChatApp.Application.Settings;
using ChatApp.Application.Validators;
using ChatApp.Infrastructure.Persistence.DbContext;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Đọc cấu hình từ appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuthSettings"));
var googleAuthSettings = builder.Configuration.GetSection("GoogleAuthSettings").Get<GoogleAuthSettings>()
                       ?? throw new InvalidOperationException("GoogleAuthSettings is not configured correctly in user secrets or appsettings.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/auth/login-google";
    options.AccessDeniedPath = "/auth/access-denied";
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = googleAuthSettings.ClientId;
    options.ClientSecret = googleAuthSettings.ClientSecret;
});

// ----- Sử dụng Serilog cho Host -----
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());


// ----- Add services to the container -----
builder.Services.AddControllers();

// --- AutoMapper ---
// Quét Assembly của Application Layer để tìm các Profile
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// --- FluentValidation ---
// Quét Assembly của Application Layer để tìm các Validator
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(SampleValidator).Assembly);

// --- Swagger/OpenAPI---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ChatApp API", Version = "v1" });
});

// --- CORS Policy ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient",
        b => b.WithOrigins("http://localhost:4200") // URL của Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // Cần cho SignalR với credentials
});


// ----- Bắt đầu cấu hình DbContext (sẽ thêm DbSet sau) -----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
//,
//         sqlServerOptionsAction: sqlOptions =>
//         {
//             // sqlOptions.EnableRetryOnFailure( // Tạm thời comment out để tránh lỗi với transaction thủ công
//             //     maxRetryCount: 5,
//             //     maxRetryDelay: TimeSpan.FromSeconds(30),
//             //     errorNumbersToAdd: null);
//         }
));


// ----- Đăng ký các Interface và Implementation -----
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// builder.Services.AddScoped<ITokenService, TokenService>();
// builder.Services.AddScoped<IUserService, UserService>();


var app = builder.Build();

// ----- Configure the HTTP request pipeline -----

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatApp API v1"));
    // app.UseDeveloperExceptionPage(); // ErrorHandlingMiddleware sẽ xử lý
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseCors("AllowAngularDevClient");

app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
// app.MapHub<ChatHub>("/chathub");

Log.Information("Starting ChatApp API...");
app.Run();
