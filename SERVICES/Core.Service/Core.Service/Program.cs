using Core.Service.Infrastructure.Data.DbContext;
using Core.Service.Infrastructure.Data.Repositories;
using Core.Service.Infrastructure.UnityOfWork;
using Core.Service.Infrastructure.Adapter;
using Core.Service.Application.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTP/2 support in Docker
builder.WebHost.ConfigureKestrel(options =>
{
    // Enable HTTP/2 over HTTP (without TLS) for Docker containers
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

// Add services to the container
builder.Services.AddGrpc();
builder.Services.AddControllers();

// Entity Framework
builder.Services.AddDbContext<ZapFinanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository pattern and Unit of Work
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddHttpClient<GoogleGeminiService>();
builder.Services.AddScoped<IGoogleGeminiService, GoogleGeminiService>();
builder.Services.AddHttpClient<WhatsAppService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ZapFinanceDbContext>();

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ZapFinanceDbContext>();
    try
    {
        context.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error applying database migrations");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// gRPC services
app.MapGrpcService<UsuarioGrpcService>();
app.MapGrpcService<ReceiptGrpcService>();

// HTTP Controllers
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Default endpoint
app.MapGet("/", () => "ZapFinance Core Service is running!");

// Database test endpoint
app.MapGet("/test-db", async (ZapFinanceDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        var userCount = await context.Usuarios.CountAsync();
        
        return Results.Ok(new 
        { 
            CanConnect = canConnect,
            UserCount = userCount,
            Message = "Database connection successful"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

app.Run();
