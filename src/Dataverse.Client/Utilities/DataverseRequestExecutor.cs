using Dataverse.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Dataverse.Client.Utilities;

/// <summary>
/// Utility class for executing Dataverse requests with retry logic and error handling.
/// </summary>
public static class DataverseRequestExecutor
{
    /// <summary>
    /// Executes a Dataverse request with retry logic for handling transient failures.
    /// </summary>
    /// <typeparam name="TResponse">The expected response type</typeparam>
    /// <param name="serviceClient">The Dataverse service client</param>
    /// <param name="request">The organization request to execute</param>
    /// <param name="options">Client configuration options</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationContext">Context description for logging (e.g., "Request", "Batch request")</param>
    /// <returns>The typed response</returns>
    public static async Task<TResponse> ExecuteWithRetryAsync<TResponse>(
        ServiceClient serviceClient,
        OrganizationRequest request,
        DataverseClientOptions options,
        ILogger logger,
        string operationContext = "Request")
        where TResponse : OrganizationResponse
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        int maxRetries = options.RetryAttempts;
        int currentAttempt = 0;

        while (currentAttempt <= maxRetries)
        {
            try
            {
                OrganizationResponse? response = await serviceClient.ExecuteAsync(request);
                return (TResponse)response;
            }
            catch (Exception ex) when (currentAttempt < maxRetries && DataverseUtilities.IsTransientException(ex))
            {
                currentAttempt++;
                int retryDelay = DataverseUtilities.CalculateRetryDelay(currentAttempt, options.RetryDelayMs);

                logger.LogWarning(ex,
                    "{OperationContext} failed with transient error on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}ms",
                    operationContext, currentAttempt, maxRetries + 1, retryDelay);

                await Task.Delay(retryDelay);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "{OperationContext} failed on attempt {Attempt}/{MaxAttempts} with non-transient error",
                    operationContext, currentAttempt + 1, maxRetries + 1);
                throw;
            }
        }

        throw new DataverseException(
            DataverseConstants.ErrorCodes.Timeout,
            $"{operationContext} failed after {maxRetries + 1} attempts");
    }
}
