using System;
using System.Linq;
using FiscalOS.Api;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Evaluation;
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
    public void Custom_classification_rule_registration_is_included_in_default_registry()
    {
        var services = new ServiceCollection();
        services.AddClassificationRule<CustomClassificationRule>();
        services.AddClassificationRuntime();

        using var provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<RuleRegistry>();
        var ruleTypes = registry.GetRules().Select(rule => rule.GetType()).ToArray();

        Assert.Contains(typeof(CustomClassificationRule), ruleTypes);
        Assert.Contains(typeof(MicroenterpriseClassificationRule), ruleTypes);
        Assert.Contains(typeof(VatPayerClassificationRule), ruleTypes);
        Assert.DoesNotContain(typeof(AlwaysPassRule), ruleTypes);
    }

    [Fact]
    public void Custom_classification_rule_registration_is_idempotent_by_rule_implementation()
    {
        var services = new ServiceCollection();
        services.AddClassificationRule<CustomClassificationRule>();
        services.AddClassificationRule<CustomClassificationRule>();
        services.AddClassificationRuntime();

        using var provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<RuleRegistry>();

        Assert.Equal(
            1,
            registry.GetRules().Count(rule => rule.GetType() == typeof(CustomClassificationRule)));
    }

    [Fact]
    public void Rule_registry_override_is_honored_before_default_registration()
    {
        var customRegistry = new DefaultRuleRegistry(
            new ClassificationRule[] { new CustomClassificationRule() });
        var services = new ServiceCollection();
        services.AddSingleton<RuleRegistry>(customRegistry);
        services.AddClassificationRuntime();

        using var provider = services.BuildServiceProvider();

        Assert.Same(customRegistry, provider.GetRequiredService<RuleRegistry>());
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

    private sealed class CustomClassificationRule : ClassificationRule
    {
        public string RuleId => "CUSTOM_CLASSIFICATION_RULE";

        public string Description => "Custom classification rule for DI registration tests.";

        public int Priority => 50;

        public RuleEvaluationResult Evaluate(ClassificationContext context) =>
            new(RuleId, false, "Custom rule evaluated.");
    }
}
