using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionEngine : ILegalReferenceResolutionEngine
{
    private readonly ILegalReferenceResolutionPipeline _pipeline;

    public LegalReferenceResolutionEngine(ILegalReferenceResolutionPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        _pipeline = pipeline;
    }

    public async Task<IReadOnlyList<ResolutionResult>> ResolveAsync(
        IReadOnlyList<LegalReference> references,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(references);

        var context = new LegalReferenceResolutionContext(
            references,
            Array.Empty<ResolutionResult>());
        var resolved = await _pipeline.RunAsync(context, cancellationToken);

        return resolved.Results;
    }
}
