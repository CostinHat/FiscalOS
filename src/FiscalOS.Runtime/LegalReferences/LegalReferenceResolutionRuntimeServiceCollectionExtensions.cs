using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences.Embedded;
using FiscalOS.Runtime.LegalReferences.Traceability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiscalOS.Runtime.LegalReferences;

public static class LegalReferenceResolutionRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddLegalReferenceResolutionRuntime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<InMemoryLegalReferenceResolutionRepository>();
        services.TryAddSingleton<InMemoryLegalReferenceResolutionAuditRepository>();
        services.TryAddSingleton<InMemoryLegalReferenceResolutionProvenanceRepository>();
        services.TryAddSingleton<InMemoryLegalReferenceResolutionEvidencePackageRepository>();

        services.TryAddSingleton<ILegalReferenceResolutionRepository>(sp =>
            sp.GetRequiredService<InMemoryLegalReferenceResolutionRepository>());
        services.TryAddSingleton<ILegalReferenceResolutionAuditRepository>(sp =>
            sp.GetRequiredService<InMemoryLegalReferenceResolutionAuditRepository>());
        services.TryAddSingleton<ILegalReferenceResolutionProvenanceRepository>(sp =>
            sp.GetRequiredService<InMemoryLegalReferenceResolutionProvenanceRepository>());
        services.TryAddSingleton<ILegalReferenceResolutionEvidencePackageRepository>(sp =>
            sp.GetRequiredService<InMemoryLegalReferenceResolutionEvidencePackageRepository>());

        services.TryAddSingleton<RepositoryLegalReferenceResolutionStage>();
        services.TryAddSingleton<ILegalReferenceResolutionStage>(sp =>
            sp.GetRequiredService<RepositoryLegalReferenceResolutionStage>());
        services.TryAddSingleton<LegalReferenceResolutionPipeline>(sp =>
            new LegalReferenceResolutionPipeline(sp.GetRequiredService<ILegalReferenceResolutionStage>()));
        services.TryAddSingleton<ILegalReferenceResolutionPipeline>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionPipeline>());
        services.TryAddSingleton<LegalReferenceResolutionEngine>(sp =>
            new LegalReferenceResolutionEngine(sp.GetRequiredService<ILegalReferenceResolutionPipeline>()));
        services.TryAddSingleton<ILegalReferenceResolutionEngine>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionEngine>());

        services.TryAddSingleton<PersistResolutionAuditTrailStage>();
        services.TryAddSingleton<ILegalReferenceResolutionAuditStage>(sp =>
            sp.GetRequiredService<PersistResolutionAuditTrailStage>());
        services.TryAddSingleton<LegalReferenceResolutionAuditPipeline>(sp =>
            new LegalReferenceResolutionAuditPipeline(sp.GetRequiredService<ILegalReferenceResolutionAuditStage>()));
        services.TryAddSingleton<ILegalReferenceResolutionAuditPipeline>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionAuditPipeline>());
        services.TryAddSingleton<LegalReferenceResolutionAuditEngine>(sp =>
            new LegalReferenceResolutionAuditEngine(sp.GetRequiredService<ILegalReferenceResolutionAuditPipeline>()));
        services.TryAddSingleton<ILegalReferenceResolutionAuditEngine>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionAuditEngine>());

        services.TryAddSingleton<PersistResolutionProvenanceStage>();
        services.TryAddSingleton<ILegalReferenceResolutionProvenanceStage>(sp =>
            sp.GetRequiredService<PersistResolutionProvenanceStage>());
        services.TryAddSingleton<LegalReferenceResolutionProvenancePipeline>(sp =>
            new LegalReferenceResolutionProvenancePipeline(
                sp.GetRequiredService<ILegalReferenceResolutionProvenanceStage>()));
        services.TryAddSingleton<ILegalReferenceResolutionProvenancePipeline>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionProvenancePipeline>());
        services.TryAddSingleton<LegalReferenceResolutionProvenanceEngine>(sp =>
            new LegalReferenceResolutionProvenanceEngine(
                sp.GetRequiredService<ILegalReferenceResolutionProvenancePipeline>()));
        services.TryAddSingleton<ILegalReferenceResolutionProvenanceEngine>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionProvenanceEngine>());

        services.TryAddSingleton<PersistResolutionEvidencePackageStage>();
        services.TryAddSingleton<ILegalReferenceResolutionEvidencePackageStage>(sp =>
            sp.GetRequiredService<PersistResolutionEvidencePackageStage>());
        services.TryAddSingleton<LegalReferenceResolutionEvidencePackagePipeline>(sp =>
            new LegalReferenceResolutionEvidencePackagePipeline(
                sp.GetRequiredService<ILegalReferenceResolutionEvidencePackageStage>()));
        services.TryAddSingleton<ILegalReferenceResolutionEvidencePackagePipeline>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionEvidencePackagePipeline>());
        services.TryAddSingleton<LegalReferenceResolutionEvidencePackageEngine>(sp =>
            new LegalReferenceResolutionEvidencePackageEngine(
                sp.GetRequiredService<ILegalReferenceResolutionEvidencePackagePipeline>()));
        services.TryAddSingleton<ILegalReferenceResolutionEvidencePackageEngine>(sp =>
            sp.GetRequiredService<LegalReferenceResolutionEvidencePackageEngine>());

        services.TryAddSingleton<ResolutionEvidencePackageComposer>();
        services.TryAddSingleton<LegalReferenceResolutionRuntime>(sp =>
            new LegalReferenceResolutionRuntime(
                sp.GetRequiredService<ILegalReferenceResolutionEngine>(),
                sp.GetRequiredService<ILegalReferenceResolutionAuditEngine>(),
                sp.GetRequiredService<ILegalReferenceResolutionProvenanceEngine>(),
                sp.GetRequiredService<ILegalReferenceResolutionEvidencePackageEngine>(),
                sp.GetRequiredService<ResolutionEvidencePackageComposer>()));
        services.TryAddSingleton<LegalReferenceTraceabilityProjector>();
        services.TryAddSingleton<EmbeddedLegalReferenceFeature>();

        return services;
    }
}
