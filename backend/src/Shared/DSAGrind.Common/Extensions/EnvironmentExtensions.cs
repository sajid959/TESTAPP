using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DotNetEnv;

namespace DSAGrind.Common.Extensions;

public static class EnvironmentExtensions
{
    /// <summary>
    /// Loads environment variables from .env file if it exists and adds them to configuration
    /// This method should be called early in the application startup process
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddEnvironmentVariables(this IConfigurationBuilder builder, IHostEnvironment? environment = null)
    {
        // Load .env file if it exists (development)
        var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envFile))
        {
            Env.Load(envFile);
        }

        // Load environment-specific .env file
        if (environment != null)
        {
            var envSpecificFile = Path.Combine(Directory.GetCurrentDirectory(), $".env.{environment.EnvironmentName.ToLower()}");
            if (File.Exists(envSpecificFile))
            {
                Env.Load(envSpecificFile);
            }
        }

        // Add environment variables to configuration (this will override appsettings values)
        builder.AddEnvironmentVariables();

        return builder;
    }

    /// <summary>
    /// Loads environment variables from .env files (should be called in Program.cs)
    /// </summary>
    public static void LoadEnvFile()
    {
        // Load .env file if it exists (development)
        var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envFile))
        {
            Env.Load(envFile);
        }

        // Load .env.development file if it exists
        var devEnvFile = Path.Combine(Directory.GetCurrentDirectory(), ".env.development");
        if (File.Exists(devEnvFile))
        {
            Env.Load(devEnvFile);
        }
    }

    /// <summary>
    /// Gets an environment variable with a fallback to appsettings configuration
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="key">The configuration key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>The configuration value</returns>
    public static string GetConfigurationValue(this IConfiguration configuration, string key, string? defaultValue = null)
    {
        // First check environment variables
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(envValue))
        {
            return envValue;
        }

        // Then check configuration (appsettings.json)
        var configValue = configuration[key];
        if (!string.IsNullOrEmpty(configValue))
        {
            return configValue;
        }

        // Return default value or throw exception
        return defaultValue ?? throw new InvalidOperationException($"Configuration key '{key}' not found in environment variables or appsettings");
    }

    /// <summary>
    /// Validates that all required environment variables are present
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="requiredKeys">Array of required configuration keys</param>
    /// <exception cref="InvalidOperationException">Thrown when required keys are missing</exception>
    public static void ValidateRequiredConfiguration(this IConfiguration configuration, params string[] requiredKeys)
    {
        var missingKeys = new List<string>();

        foreach (var key in requiredKeys)
        {
            var value = Environment.GetEnvironmentVariable(key) ?? configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                missingKeys.Add(key);
            }
        }

        if (missingKeys.Any())
        {
            throw new InvalidOperationException(
                $"Required configuration keys are missing: {string.Join(", ", missingKeys)}. " +
                "Please set these as environment variables or in appsettings.json");
        }
    }
}