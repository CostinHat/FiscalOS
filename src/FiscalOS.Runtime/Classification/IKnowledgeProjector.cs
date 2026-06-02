namespace FiscalOS.Runtime.Classification;

public interface IKnowledgeProjector
{
    KnowledgeProjectionResult Project(ClassificationDecision decision);
}
