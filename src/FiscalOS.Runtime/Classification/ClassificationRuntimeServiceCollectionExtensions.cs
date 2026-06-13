using FiscalOS.Runtime.Classification.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiscalOS.Runtime.Classification;

public static class ClassificationRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddClassificationRuntime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ClassificationRule, MicroenterpriseClassificationRule>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ClassificationRule, VatPayerClassificationRule>());

        services.TryAddSingleton<RuleRegistry>(sp =>
            new DefaultRuleRegistry(sp.GetServices<ClassificationRule>()));
        services.TryAddSingleton<ILegalBasisResolver, LegalKnowledgeLegalBasisResolver>();
        services.TryAddSingleton<ClassificationEngine>();

        return services;
    }
}
