using Microsoft.Extensions.Configuration;

namespace DSAGrind.Common.Configuration;

/// <summary>
/// Configuration source that wraps another source to provide variable substitution
/// </summary>
public class VariableSubstitutionConfigurationSource : IConfigurationSource
{
    private readonly IConfigurationSource _underlyingSource;

    public VariableSubstitutionConfigurationSource(IConfigurationSource underlyingSource)
    {
        _underlyingSource = underlyingSource;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var underlyingProvider = _underlyingSource.Build(builder);
        return new VariableSubstitutionConfigurationProvider(underlyingProvider);
    }
}