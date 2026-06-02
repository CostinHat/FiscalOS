using System;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Jurisdiction
{
    [Fact]
    public void Jurisdiction_id_exposes_its_value()
    {
        var id = new JurisdictionId("RO");

        Assert.Equal("RO", id.Value);
        Assert.Equal("RO", id.ToString());
    }

    [Fact]
    public void Jurisdiction_id_rejects_empty_value()
    {
        Assert.Throws<ArgumentException>(() => new JurisdictionId("  "));
    }

    [Fact]
    public void Jurisdiction_exposes_id_and_name()
    {
        var jurisdiction = new Jurisdiction(new JurisdictionId("RO"), "Romania");

        Assert.Equal("RO", jurisdiction.Id.Value);
        Assert.Equal("Romania", jurisdiction.Name);
    }

    [Fact]
    public void Jurisdictions_with_the_same_components_are_equal()
    {
        var a = new Jurisdiction(new JurisdictionId("RO"), "Romania");
        var b = new Jurisdiction(new JurisdictionId("RO"), "Romania");

        Assert.Equal(a, b);
    }
}
