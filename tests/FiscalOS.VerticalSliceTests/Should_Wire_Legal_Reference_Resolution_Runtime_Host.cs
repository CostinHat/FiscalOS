using FiscalOS.Api;
using FiscalOS.Runtime.LegalReferences;
using FiscalOS.Runtime.LegalReferences.Embedded;
using FiscalOS.Runtime.LegalReferences.Traceability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Wire_Legal_Reference_Resolution_Runtime_Host
{
    [Fact]
    public void Api_host_container_resolves_legal_reference_resolution_runtime()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddFiscalOSApi();

        using var provider = builder.Services.BuildServiceProvider();

        Assert.IsType<LegalReferenceResolutionRuntime>(
            provider.GetRequiredService<LegalReferenceResolutionRuntime>());
    }

    [Fact]
    public void Api_host_container_resolves_embedded_traceability_boundary_without_mapping_endpoint()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddFiscalOSApi();

        var app = builder.Build();

        Assert.IsType<LegalReferenceTraceabilityProjector>(
            app.Services.GetRequiredService<LegalReferenceTraceabilityProjector>());
        Assert.IsType<EmbeddedLegalReferenceFeature>(
            app.Services.GetRequiredService<EmbeddedLegalReferenceFeature>());
        Assert.Empty(app.Services.GetRequiredService<EndpointDataSource>().Endpoints);
    }
}
