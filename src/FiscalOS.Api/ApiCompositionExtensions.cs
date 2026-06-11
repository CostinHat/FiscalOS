using FiscalOS.Runtime.LegalReferences;
using Microsoft.Extensions.DependencyInjection;

namespace FiscalOS.Api;

public static class ApiCompositionExtensions
{
    public static IServiceCollection AddFiscalOSApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLegalReferenceResolutionRuntime();

        return services;
    }
}
