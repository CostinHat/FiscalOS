using System;
using System.Linq;
using FiscalOS.Api;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Register_Classification_Runtime
{
    [Fact]
    public void Service_collection_extension_resolves_classification_runtime_components()
    {
        var services = new ServiceCollection();
        services.AddClassificationRuntime();

        using var provider = services.BuildServiceProvider();

        Assert.IsType<ClassificationEngine>(provider.GetRequiredService<ClassificationEngine>());
        Assert.IsType<DefaultRuleRegistry>(provider.GetRequiredService<RuleRegistry>());
        Assert.IsType<LegalKnowledgeLegalBasisResolver>(provider.GetRequiredService<ILegalBasisResolver>());
    }

    [Fact]
    public void Default_rule_registry_contains_curated_runtime_rules()
    {
        var services = new ServiceCollection();
        services.AddClassificationRuntime();

        using var provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<RuleRegistry>();
        var ruleTypes = registry.GetRules().Select(rule => rule.GetType()).ToArray();

        Assert.Contains(typeof(MicroenterpriseClassificationRule), ruleTypes);
        Assert.Contains(typeof(VatPayerClassificationRule), ruleTypes);
        Assert.DoesNotContain(typeof(AlwaysPassRule), ruleTypes);
    }

    [Fact]
    public void Legal_basis_resolver_override_is_honored_before_default_registration()
    {
        var customResolver = new CustomLegalBasisResolver();
        var services = new ServiceCollection();
        services.AddSingleton<ILegalBasisResolver>(customResolver);
        services.AddClassificationRuntime();

        using var provider = services.BuildServiceProvider();

        Assert.Same(customResolver, provider.GetRequiredService<ILegalBasisResolver>());
    }

    [Fact]
    public void Api_composition_wires_classification_runtime()
    {
        var services = new ServiceCollection();
        services.AddFiscalOSApi();

        using var provider = services.BuildServiceProvider();

        Assert.IsType<ClassificationEngine>(provider.GetRequiredService<ClassificationEngine>());
        Assert.IsType<DefaultRuleRegistry>(provider.GetRequiredService<RuleRegistry>());
        Assert.IsType<LegalKnowledgeLegalBasisResolver>(provider.GetRequiredService<ILegalBasisResolver>());
    }

    private sealed class CustomLegalBasisResolver : ILegalBasisResolver
    {
        public DecisionLegalBasis Resolve(IReadOnlyList<LegalCitation> consideredCitations) =>
            new(consideredCitations, new ConflictResolutionResult(Array.Empty<LegalCitation>()));
    }
}
