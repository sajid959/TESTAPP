using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace DSAGrind.Common.Configuration;

/// <summary>
/// Configuration provider that substitutes environment variables in configuration values
/// Supports ${VAR_NAME} and ${VAR_NAME:default_value} syntax
/// </summary>
public class VariableSubstitutionConfigurationProvider : ConfigurationProvider
{
    private readonly IConfigurationProvider _underlyingProvider;
    private static readonly Regex VariablePattern = new(@"\$\{([^}:]+)(?::([^}]*))?\}", RegexOptions.Compiled);

    public VariableSubstitutionConfigurationProvider(IConfigurationProvider underlyingProvider)
    {
        _underlyingProvider = underlyingProvider;
    }

    public override void Load()
    {
        _underlyingProvider.Load();
        
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var item in _underlyingProvider.GetChildKeys(new string[0], null))
        {
            ProcessConfigurationItem("", item, data);
        }
        
        Data = data;
    }

    private void ProcessConfigurationItem(string prefix, string key, Dictionary<string, string?> data)
    {
        var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
        
        if (_underlyingProvider.TryGet(fullKey, out var value) && value != null)
        {
            // Substitute variables in the value
            var substitutedValue = SubstituteVariables(value);
            data[fullKey] = substitutedValue;
        }
        
        // Process child keys
        var childKeys = _underlyingProvider.GetChildKeys(new[] { fullKey }, null);
        foreach (var childKey in childKeys)
        {
            ProcessConfigurationItem(fullKey, childKey, data);
        }
    }

    private static string SubstituteVariables(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return VariablePattern.Replace(value, match =>
        {
            var variableName = match.Groups[1].Value;
            var defaultValue = match.Groups[2].Success ? match.Groups[2].Value : null;
            
            // First try to get from environment variables
            var envValue = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }
            
            // If no environment variable found, use default value if provided
            if (defaultValue != null)
            {
                return defaultValue;
            }
            
            // If no default value, return the original placeholder
            return match.Value;
        });
    }

    public override bool TryGet(string key, out string? value)
    {
        return Data.TryGetValue(key, out value);
    }

    public override void Set(string key, string? value)
    {
        Data[key] = value;
    }

    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return _underlyingProvider.GetChildKeys(earlierKeys, parentPath);
    }
}