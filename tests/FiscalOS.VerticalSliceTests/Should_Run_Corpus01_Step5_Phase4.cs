using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Corpus01_Step5_Phase4
{
    [Fact] public void Valid_legal_valid_time_is_explicit() { var r=LegalValidTime.Create(new DateOnly(2026,1,1),new DateOnly(2026,12,31)); Assert.Equal(SemanticCurationError.None,r.Error); Assert.Equal(new DateOnly(2026,1,1),r.Value!.ValidFrom); }
    [Fact] public void Invalid_legal_valid_range_is_typed_error() { Assert.Equal(SemanticCurationError.InvalidLegalValidRange,LegalValidTime.Create(new DateOnly(2026,2,1),new DateOnly(2026,1,1)).Error); Assert.Equal(SemanticCurationError.MissingLegalValidFrom,LegalValidTime.Create(null,null).Error); }
    [Fact] public void Legal_valid_time_is_not_inferred_from_knowledge_time() { var r=LegalValidTime.Create(new DateOnly(2020,1,1),null); Assert.Equal(SemanticCurationError.None,r.Error); Assert.NotEqual(DateOnly.FromDateTime(DateTime.UtcNow),r.Value!.ValidFrom); }
    [Fact] public void Legal_valid_time_is_independent_value() { var a=new LegalValidTime(new DateOnly(2026,1,1)); var b=new LegalValidTime(new DateOnly(2026,1,1)); Assert.Equal(a,b); }
}
