using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Dataverse.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Dataverse.Client;

public static class ServiceInjection
{
    /// <summary>
    /// Adds Dataverse client services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddDataverseClient(
        this IServiceCollection services,
        Action<DataverseClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Configure and validate options
        services.Configure(configureOptions);
        services.AddSingleton<IValidateOptions<DataverseClientOptions>, DataverseClientOptionsValidator>();

        // Register dependencies first
        services.AddSingleton(provider =>
        {
            DataverseClientOptions options = provider.GetRequiredService<IOptions<DataverseClientOptions>>().Value;
            return new ServiceClient(options.GetEffectiveConnectionString());
        });

        // Register Dataverse services
        services.AddSingleton<IBatchProcessor, BatchProcessor>();
        services.AddSingleton<IDataverseClient, DataverseClient>();
        services.AddSingleton<IDataverseMetadataClient, DataverseMetadataClient>();

        return services;
    }

    /// <summary>
    /// Adds Dataverse client services using configuration section.
    /// </summary>
    public static IServiceCollection AddDataverseClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Dataverse") => services.AddDataverseClient(options =>
                                                  {
                                                      IConfigurationSection section = configuration.GetSection(sectionName);
                                                      section.Bind(options);
                                                  });
}

/// <summary>
/// Validates the configuration options for the Dataverse client.
/// </summary>
public class DataverseClientOptionsValidator : IValidateOptions<DataverseClientOptions>
{
    public ValidateOptionsResult Validate(string? name, DataverseClientOptions options)
    {
        List<string> errors = options.GetValidationErrors();
        return errors.Count != 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
