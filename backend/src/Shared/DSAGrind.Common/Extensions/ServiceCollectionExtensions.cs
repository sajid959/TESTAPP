using DSAGrind.Common.Configuration;
using DSAGrind.Common.Repositories;
using DSAGrind.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Qdrant.Client;

namespace DSAGrind.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure settings
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<AISettings>(configuration.GetSection(AISettings.SectionName));
        services.Configure<QdrantSettings>(configuration.GetSection(QdrantSettings.SectionName));

        // Add MongoDB
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>();
            return new MongoClient(settings!.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var settings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>();
            return client.GetDatabase(settings!.DatabaseName);
        });

        // Add common repositories
        //services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.User>, MongoRepository<DSAGrind.Models.Entities.User>>();
        //services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Problem>, MongoRepository<DSAGrind.Models.Entities.Problem>>();
        //services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Submission>, MongoRepository<DSAGrind.Models.Entities.Submission>>();
        //services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Category>, MongoRepository<DSAGrind.Models.Entities.Category>>();
        //services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Subscription>, MongoRepository<DSAGrind.Models.Entities.Subscription>>();
        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.User>>(sp =>
    new MongoRepository<DSAGrind.Models.Entities.User>(
        sp.GetRequiredService<IOptions<MongoDbSettings>>(),
        "users"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Problem>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Problem>(
                sp.GetRequiredService<IOptions<MongoDbSettings>>(),
                "problems"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Submission>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Submission>(
                sp.GetRequiredService<IOptions<MongoDbSettings>>(),
                "submissions"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Category>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Category>(
                sp.GetRequiredService<IOptions<MongoDbSettings>>(),
                "categories"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Payment>>(sp =>
    new MongoRepository<DSAGrind.Models.Entities.Payment>(
        sp.GetRequiredService<IOptions<MongoDbSettings>>(),
        "payments"));
        
        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Subscription>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Subscription>(
                sp.GetRequiredService<IOptions<MongoDbSettings>>(),
                "subscriptions"));
        // Add common services
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IKafkaService, KafkaService>();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<IRedisService, RedisService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAIProviderService, PerplexityAIService>();
        services.AddSingleton<QdrantClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
            return new QdrantClient(config.Url, config.GrpcPort,false ,config.ApiKey);
        });

        // Add HttpClient
        services.AddHttpClient();

        // Add health checks
        services.AddHealthChecks();

        return services;
    }
}