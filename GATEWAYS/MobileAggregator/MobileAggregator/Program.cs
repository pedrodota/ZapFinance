using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MobileAggregator.Application.UseCases.IUseCase;
using MobileAggregator.Application.UseCases.UseCase;
using MobileAggregator.Application.UseCases.UploadReceiptUseCase;
using ZapFinance.ProtoServer.Core;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// gRPC Client
builder.Services.AddSingleton(provider =>
{
    var coreServiceUrl = builder.Configuration.GetConnectionString("CoreService") ?? "https://localhost:7001";
    
    // Configure for HTTP/2 over HTTP (non-TLS) when using Docker containers
    var httpHandler = new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    
    return GrpcChannel.ForAddress(coreServiceUrl, new GrpcChannelOptions
    {
        HttpHandler = httpHandler,
        // For HTTP (non-HTTPS) URLs, we need to explicitly allow unencrypted HTTP/2
        UnsafeUseInsecureChannelCallCredentials = coreServiceUrl.StartsWith("http://")
    });
});

// gRPC typed clients
builder.Services.AddSingleton(provider =>
{
    var channel = provider.GetRequiredService<GrpcChannel>();
    return new ReceiptService.ReceiptServiceClient(channel);
});

// Use Cases
builder.Services.AddScoped<IAuthUseCase, AuthUseCase>();
builder.Services.AddScoped<IUserUseCase, UserUseCase>();
builder.Services.AddScoped<IUploadReceiptUseCase, UploadReceiptUseCase>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default-secret-key"))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobile", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowMobile");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => "ZapFinance Mobile Aggregator is running!");

app.Run();
