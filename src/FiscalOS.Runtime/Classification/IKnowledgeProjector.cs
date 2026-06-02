using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public interface IKnowledgeProjector
{
    KnowledgeProjectionResult Project(ClassificationResult result);
}
