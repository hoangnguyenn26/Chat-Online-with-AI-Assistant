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

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Đọc cấu hình GoogleAuthSettings
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuthSettings"));
var googleAuthSettings = builder.Configuration.GetSection("GoogleAuthSettings").Get<GoogleAuthSettings>()
                       ?? throw new InvalidOperationException("GoogleAuthSettings is not configured correctly in user secrets or appsettings.");


// Cấu hình Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Không cần set ở đây nếu Challenge chỉ định scheme
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{

})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options => // Cấu hình Google Provider
{
    options.ClientId = googleAuthSettings.ClientId;
    options.ClientSecret = googleAuthSettings.ClientSecret;
});


builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(SampleValidator).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ChatApp API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient",
        b => b.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatApp API v1"));
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCors("AllowAngularDevClient");

app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

Log.Information("Starting ChatApp API...");
app.Run();