using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using log4net;
using log4net.Config;
using MassTransit;
using System.Reflection;

using Subasta.Application.Handlers;
using Subasta.Application.Validations;

using Subasta.Domain.Events;
using Subasta.Domain.Repositories;

using Subasta.Infrastructure.Configurations;
using Subasta.Infrastructure.Consumer;
using Subasta.Infrastructure.Interfaces;
using Subasta.Infrastructure.Persistence.Repository.MongoRead;
using Subasta.Infrastructure.Persistence.Repository.MongoWrite;
using MongoDB.Bson.Serialization;
using Subasta.Infrastructure.StateMachine;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using Hangfire;
using Hangfire.MemoryStorage;
using Subasta.Infrastructure.Queries.QueryHandlers;
using RestSharp;
using Subasta.Infrastructure.Services;

var loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(loggerRepository, new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRestClient>(new RestClient());
builder.Services.AddSingleton<ICronJobService, CronJobService>();

// Registrar configuración de MongoDB
builder.Services.AddSingleton<MongoWriteDbConfig>();
builder.Services.AddSingleton<MongoReadDbConfig>();

// Registrar configuración de Log4Net
builder.Services.AddSingleton(LogManager.GetLogger(typeof(Program)));

// REGISTRA EL REPOSITORIO ANTES DE MediatR
builder.Services.AddScoped<IAuctionRepository, MongoWriteAuctionRepository>();
builder.Services.AddScoped<IReadAuctionRepository, MongoReadAuctionRepository>();
builder.Services.AddScoped<IReadPrizeClaimRepository, MongoReadPrizeClaimRepository>();
builder.Services.AddScoped<IPrizeClaimRepository, MongoWritePrizeClaimRepository>();

// REGISTRA MediatR PARA TODOS LOS HANDLERS
var list = new List<System.Reflection.Assembly>();

list.Add(typeof(CreateAuctionCommandHandler).Assembly);
list.Add(typeof(StartAuctionCommandHandler).Assembly);
list.Add(typeof(PdfGeneratorService).Assembly);
list.Add(typeof(ExcelGeneratorService).Assembly);
list.Add(typeof(CreatePrizeClaimCommandHandler).Assembly);

list.Add(typeof(GetAuctionByIdQueryHandler).Assembly);
list.Add(typeof(GetAuctionsByStatusQueryHandler).Assembly);
list.Add(typeof(GetProductAuctionsQueryHandler).Assembly);
list.Add(typeof(GetUserAuctionsQueryHandler).Assembly);
list.Add(typeof(GetPrizeClaimByUserAndPrizeClaimQueryHandler).Assembly);
list.Add(typeof(GetAuctionsInRangeQueryHandler).Assembly);

list.Add(typeof(SendNotificationConsumer).Assembly);

foreach (var item in list.Distinct())
{
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(item));
   
}


builder.Services.AddValidatorsFromAssemblyContaining<CreateAuctionDtoValidation>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddMassTransit(busConfigurator =>
{

    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.AddConsumers(typeof(Program).Assembly);
    busConfigurator.AddConsumer<CreateAuctionConsumer>();
    busConfigurator.AddConsumer<AuctionStatusChangedConsumer>();
    busConfigurator.AddConsumer<DeleteAuctionConsumer>();
    busConfigurator.AddConsumer<UpdateAuctionConsumer>();
    busConfigurator.AddConsumer<CreatePrizeClaimConsumer>();

    busConfigurator.AddSagaStateMachine<AuctionStatusSaga, AuctionStatusSagaData>()
    .MongoDbRepository(r =>
    {
        r.Connection = Environment.GetEnvironmentVariable("MONGODB_CNN_WRITE");
        r.DatabaseName = Environment.GetEnvironmentVariable("MONGODB_MASSTRANSIT_NAME");
        r.CollectionName = "auctionStatusSaga";
    });

    BsonClassMap.RegisterClassMap<AuctionStatusSagaData>(cm =>
    {
        cm.AutoMap();
        cm.MapIdProperty(x => x.CorrelationId)
            .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
    });
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(Environment.GetEnvironmentVariable("RABBIT_URL")), h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBIT_USERNAME"));
            h.Password(Environment.GetEnvironmentVariable("RABBIT_PASSWORD"));
        });

        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE"), e =>
        {
            e.ConfigureConsumer<CreateAuctionConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_STATUS_CHANGE"), e =>
        {
            e.ConfigureConsumer<AuctionStatusChangedConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_DELETE"), e =>
        {
            e.ConfigureConsumer<DeleteAuctionConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_UPDATE"), e =>
        {
            e.ConfigureConsumer<UpdateAuctionConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_PRIZE_CLAIM_QUEUE"), e =>
        {
            e.ConfigureConsumer<CreatePrizeClaimConsumer>(context);
        });

        configurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        configurator.ConfigureEndpoints(context);
    });
});
EndpointConvention.Map<AuctionCreatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE")));
EndpointConvention.Map<AuctionStatusChangedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_STATUS_CHANGE")));
EndpointConvention.Map<AuctionDeletedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_DELETE")));
EndpointConvention.Map<AuctionUpdatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_UPDATE")));
EndpointConvention.Map<PrizeClaimCreatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_PRIZE_CLAIM_QUEUE")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() 
            .AllowAnyMethod() 
            .AllowAnyHeader();
    });
});
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
