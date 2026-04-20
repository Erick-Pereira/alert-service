using Microsoft.EntityFrameworkCore;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Infrastructure.Persistence.Repositories;
using Simcag.AlertService.Application.Services;
using Simcag.AlertService.Application.Workers;
using Simcag.Shared.Messaging.Configuration;
using Simcag.Shared.Messaging.Contracts;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging.Extensions;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AlertDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// Services
builder.Services.AddScoped<IAlertService, AlertEvaluationService>();

// Controllers & API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks();

// RabbitMQ Configuration
var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ__HOST") ?? "localhost";
var rabbitMqPort = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ__PORT") ?? "5672");
var rabbitMqUserName = Environment.GetEnvironmentVariable("RABBITMQ__USERNAME") ?? "guest";
var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ__PASSWORD") ?? "guest";

var rabbitMqOptions = new RabbitMqOptions
{
    Host = rabbitMqHost,
    Port = rabbitMqPort,
    UserName = rabbitMqUserName,
    Password = rabbitMqPassword,
    VirtualHost = "/"
};

builder.Services.AddRabbitMqMessaging(rabbitMqOptions);
builder.Services.AddRabbitMqEventConsumer<PriceAnalyzedEvent>("price.analyzed");
builder.Services.AddRabbitMqEventPublisher<AlertCreatedEvent>("alerts");

// Background Services
builder.Services.AddHostedService<PriceAnalyzedEventConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();