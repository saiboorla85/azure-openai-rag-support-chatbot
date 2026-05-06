using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using PropertySupport.RagApi.Configuration;

namespace PropertySupport.RagApi.Services;

public sealed class KernelBuilderService : IKernelBuilderService
{
    private readonly AzureOpenAIOptions _options;
    private readonly ILoggerFactory _loggerFactory;

    public KernelBuilderService(IOptions<AzureOpenAIOptions> options, ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _loggerFactory = loggerFactory;
    }

    public Kernel BuildKernel()
    {
        ValidateConfiguration();

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_loggerFactory);

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: _options.DeploymentName,
            endpoint: _options.Endpoint,
            apiKey: _options.ApiKey);

        return builder.Build();
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
            throw new InvalidOperationException("AzureOpenAI:Endpoint is missing.");

        if (string.IsNullOrWhiteSpace(_options.DeploymentName))
            throw new InvalidOperationException("AzureOpenAI:DeploymentName is missing.");

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("AzureOpenAI:ApiKey is missing.");
    }
}
