using ChatApp.Api.Middleware;
using ChatApp.Application.Mappings;
using ChatApp.Application.Validators;
using ChatApp.Infrastructure.Persistence.DbContext;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ----- Cấu hình Serilog ban đầu (trước khi builder.Build()) -----
// Đọc cấu hình từ appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateBootstrapLogger(); // Dùng tạm logger này cho đến khi Host được build

var builder = WebApplication.CreateBuilder(args);

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

// --- Swagger/OpenAPI (Thường đã có sẵn) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ChatApp API", Version = "v1" });
    // Cấu hình JWT cho Swagger sẽ thêm sau
});

// --- CORS Policy (Cho phép Angular dev server) ---
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
// ----- Kết thúc DbContext -----

// ----- Đăng ký các Interface và Implementation -----
// Ví dụ:
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// builder.Services.AddScoped<ITokenService, TokenService>();
// builder.Services.AddScoped<IUserService, UserService>();


var app = builder.Build();

// ----- Configure the HTTP request pipeline -----

// Sử dụng Global Error Handling Middleware (sẽ tạo sau)
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

app.UseCors("AllowAngularDevClient"); // Đặt CORS ở đây, trước Auth

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
// app.MapHub<ChatHub>("/chathub");

Log.Information("Starting ChatApp API...");
app.Run();
