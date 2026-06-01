using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed class ClassificationEngine
{
    public Task<ClassificationResult> ClassifyAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new ClassificationResult(
            Category: "Unclassified",
            Explanation: "No classification rules have been executed yet.");

        return Task.FromResult(result);
    }
}