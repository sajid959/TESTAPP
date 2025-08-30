using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNetEnv;
using DSAGrind.Common.Configuration;

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
        // Get the root directory (traverse up from service directory to find backend folder)
        var currentDir = Directory.GetCurrentDirectory();
        var backendDir = FindBackendDirectory(currentDir);
        
        if (backendDir != null)
        {
            // Load .env file from backend directory
            var envFile = Path.Combine(backendDir, ".env");
            if (File.Exists(envFile))
            {
                Env.Load(envFile);
                Console.WriteLine($"Loaded environment variables from: {envFile}");
            }
            else
            {
                Console.WriteLine($"Environment file not found: {envFile}");
            }

            // Load .env.development file if it exists
            var devEnvFile = Path.Combine(backendDir, ".env.development");
            if (File.Exists(devEnvFile))
            {
                Env.Load(devEnvFile);
                Console.WriteLine($"Loaded development environment variables from: {devEnvFile}");
            }
        }
        else
        {
            // Fallback to current directory (for compatibility)
            var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envFile))
            {
                Env.Load(envFile);
                Console.WriteLine($"Loaded environment variables from current directory: {envFile}");
            }
        }
    }

    private static string? FindBackendDirectory(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        
        while (current != null)
        {
            // Check if current directory is backend or contains backend folder
            if (current.Name.Equals("backend", StringComparison.OrdinalIgnoreCase))
            {
                return current.FullName;
            }
            
            // Check if current directory contains a backend folder
            var backendSubDir = Path.Combine(current.FullName, "backend");
            if (Directory.Exists(backendSubDir))
            {
                return backendSubDir;
            }
            
            current = current.Parent;
        }
        
        return null;
    }

    /// <summary>
    /// Gets an environment variable with a fallback to appsettings configuration
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="key">The configuration key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <param name="logger">Optional logger for fallback notifications</param>
    /// <returns>The configuration value</returns>
    public static string GetConfigurationValue(this IConfiguration configuration, string key, string? defaultValue = null, ILogger? logger = null)
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
            // Log that we're using local configuration instead of environment variable
            logger?.LogInformation("⚠️  Using local configuration value for {Key} from appsettings.json (consider setting environment variable)", key);
            return configValue;
        }

        // Return default value with logging
        if (defaultValue != null)
        {
            logger?.LogWarning("⚠️  Using local fallback value for {Key}: {DefaultValue}", key, defaultValue);
            return defaultValue;
        }

        // Log error and throw exception
        logger?.LogError("❌ Configuration key '{Key}' not found in environment variables or appsettings", key);
        throw new InvalidOperationException($"Configuration key '{key}' not found in environment variables or appsettings");
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

    /// <summary>
    /// Substitutes environment variables in configuration after it's loaded
    /// This replaces ${VAR_NAME} and ${VAR_NAME:default} syntax with actual values
    /// </summary>
    /// <param name="configuration">The configuration to process</param>
    /// <param name="logger">Optional logger for debugging</param>
    public static void SubstituteEnvironmentVariables(this IConfiguration configuration, ILogger? logger = null)
    {
        var configurationRoot = configuration as IConfigurationRoot;
        if (configurationRoot == null) return;

        var substitutedValues = new Dictionary<string, string>();

        foreach (var provider in configurationRoot.Providers)
        {
            var data = GetProviderData(provider);
            if (data == null) continue;

            foreach (var kvp in data.ToList())
            {
                if (kvp.Value != null && kvp.Value.Contains("${"))
                {
                    var substitutedValue = SubstituteVariables(kvp.Value, logger);
                    if (substitutedValue != kvp.Value)
                    {
                        substitutedValues[kvp.Key] = substitutedValue;
                        logger?.LogDebug("Substituted {Key}: {Original} -> {Substituted}", kvp.Key, kvp.Value, substitutedValue);
                    }
                }
            }
        }

        // Apply substituted values
        foreach (var kvp in substitutedValues)
        {
            configuration[kvp.Key] = kvp.Value;
        }
    }

    private static IDictionary<string, string?>? GetProviderData(IConfigurationProvider provider)
    {
        var field = provider.GetType().GetField("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(provider) as IDictionary<string, string?>;
    }

    private static string SubstituteVariables(string value, ILogger? logger = null)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var pattern = @"\$\{([^}:]+)(?::([^}]*))?\}";
        return System.Text.RegularExpressions.Regex.Replace(value, pattern, match =>
        {
            var variableName = match.Groups[1].Value;
            var defaultValue = match.Groups[2].Success ? match.Groups[2].Value : null;
            
            var envValue = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrEmpty(envValue))
            {
                logger?.LogDebug("Found environment variable {Variable} = {Value}", variableName, envValue);
                return envValue;
            }
            
            if (defaultValue != null)
            {
                logger?.LogWarning("Environment variable {Variable} not found, using default: {Default}", variableName, defaultValue);
                return defaultValue;
            }
            
            logger?.LogError("Environment variable {Variable} not found and no default provided", variableName);
            return match.Value; // Return original if no substitution possible
        });
    }
}