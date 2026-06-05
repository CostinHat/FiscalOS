using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class RepositoryLegalReferenceResolutionStage : ILegalReferenceResolutionStage
{
    private readonly ILegalReferenceResolutionRepository _repository;

    public RepositoryLegalReferenceResolutionStage(ILegalReferenceResolutionRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        _repository = repository;
    }

    public string Name => "repository-resolution";

    public async Task<LegalReferenceResolutionContext> ExecuteAsync(
        LegalReferenceResolutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var results = new List<ResolutionResult>(context.Results);
        foreach (var query in context.Queries)
        {
            var stored = await _repository.GetAsync(query, cancellationToken);
            results.Add(stored ?? Unresolved(query));
        }

        return context with { Results = results };
    }

    private static ResolutionResult Unresolved(LegalReference query)
    {
        return new ResolutionResult(
            ResolutionStatus.Unresolved,
            Array.Empty<ResolutionCandidate>(),
            new UnresolvedReference(query, "No stored resolution found."));
    }
}
