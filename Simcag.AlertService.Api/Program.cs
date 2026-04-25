using Microsoft.AspNetCore.Mvc;
using Simcag.AlertService.Api.Controllers;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.Services;
using Simcag.AlertService.Infrastructure.Cache;
using Simcag.AlertService.Infrastructure.Messaging;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Configuration;
using Simcag.Shared.Messaging.Extensions;
using Simcag.Shared.Messaging.Contracts;
using RabbitMQ.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
builder.Services.AddHealthChecks();

// Distributed cache (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
    options.InstanceName = "AlertService:";
});

// RabbitMQ Configuration
var rabbitMqOptions = new RabbitMqOptions
{
    Host = Environment.GetEnvironmentVariable("RABBITMQ__HOST") ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ__PORT") ?? "5672"),
    UserName = Environment.GetEnvironmentVariable("RABBITMQ__USERNAME") ?? "guest",
    Password = Environment.GetEnvironmentVariable("RABBITMQ__PASSWORD") ?? "guest",
    VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ__VIRTUALHOST") ?? "/"
};

builder.Services.AddSingleton(rabbitMqOptions);
builder.Services.AddRabbitMqMessaging(rabbitMqOptions);

// Register Event Publishers
builder.Services.AddSingleton<IEventPublisher<AlertTriggeredEvent>, RabbitMqAlertPublisher>();

// Register Application Services
builder.Services.AddScoped<IAlertRuleService, AlertClassificationService>();
builder.Services.AddScoped<IRedisCacheService, RedisAlertDeduplicationCache>();
builder.Services.AddScoped<IAlertService, AlertOrchestrator>();

// Register Workers
builder.Services.AddHostedService<PriceAnalysisEventConsumer>();

// HTTP Client
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
