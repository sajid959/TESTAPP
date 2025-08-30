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
        // Substitute environment variables in configuration automatically
        configuration.SubstituteEnvironmentVariables();
        
        // Configure settings
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        // KAFKA MIGRATION TO RABBITMQ - Commented out Kafka settings
        // services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.Configure<RabbitMQSettings>(configuration.GetSection(RabbitMQSettings.SectionName));
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<AISettings>(configuration.GetSection(AISettings.SectionName));
        services.Configure<QdrantSettings>(configuration.GetSection(QdrantSettings.SectionName));

        // Add MongoDB with robust SSL/TLS configuration
        services.AddSingleton<IMongoClient>(sp =>
        {
            try
            {
                var settings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>();
                var connectionString = settings!.ConnectionString;
                
                // Try different MongoDB driver approaches for SSL
                var mongoUrl = new MongoUrl(connectionString);
                var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);
                
                // Disable all SSL certificate validation for cross-platform compatibility
                mongoClientSettings.UseTls = true;
                mongoClientSettings.AllowInsecureTls = true;
                mongoClientSettings.SslSettings = new SslSettings
                {
                    EnabledSslProtocols = System.Security.Authentication.SslProtocols.None,
                    CheckCertificateRevocation = false,
                    ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                
                // Aggressive timeout settings
                mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(30);
                mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
                mongoClientSettings.SocketTimeout = TimeSpan.FromSeconds(60);
                mongoClientSettings.WaitQueueTimeout = TimeSpan.FromSeconds(30);
                
                // Retry and pool settings
                mongoClientSettings.RetryWrites = true;
                mongoClientSettings.RetryReads = true;
                mongoClientSettings.MaxConnectionPoolSize = 50;
                mongoClientSettings.MinConnectionPoolSize = 5;
                
                mongoClientSettings.ApplicationName = "DSAGrind";
                
                return new MongoClient(mongoClientSettings);
            }
            catch (Exception ex)
            {
                // If MongoDB connection fails, log error and return a placeholder
                Console.WriteLine($"MongoDB connection failed: {ex.Message}");
                // Create a basic client that will fail gracefully
                return new MongoClient("mongodb://localhost:27017");
            }
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
                sp.GetRequiredService<IMongoDatabase>(),
                "users"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Problem>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Problem>(
                sp.GetRequiredService<IMongoDatabase>(),
                "problems"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Submission>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Submission>(
                sp.GetRequiredService<IMongoDatabase>(),
                "submissions"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Category>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Category>(
                sp.GetRequiredService<IMongoDatabase>(),
                "categories"));

        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Payment>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Payment>(
                sp.GetRequiredService<IMongoDatabase>(),
                "payments"));
        
        services.AddScoped<IMongoRepository<DSAGrind.Models.Entities.Subscription>>(sp =>
            new MongoRepository<DSAGrind.Models.Entities.Subscription>(
                sp.GetRequiredService<IMongoDatabase>(),
                "subscriptions"));
        // Add common services
        services.AddSingleton<IJwtService, JwtService>();
        // KAFKA MIGRATION TO RABBITMQ - Commented out Kafka service registration
        // services.AddSingleton<IKafkaService, KafkaService>();
        services.AddSingleton<RabbitMQService>();
        services.AddSingleton<IKafkaService>(sp => sp.GetRequiredService<RabbitMQService>());
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<IRedisService, RedisService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAIProviderService, PerplexityAIService>();
        services.AddSingleton<QdrantClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
            return new QdrantClient(config.Url, config.GrpcPort, false, config.ApiKey);
        });

        // Add HttpClient
        services.AddHttpClient();

        // Add health checks
        services.AddHealthChecks();

        return services;
    }
}