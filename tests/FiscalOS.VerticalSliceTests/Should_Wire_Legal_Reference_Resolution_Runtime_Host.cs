using FiscalOS.Api;
using FiscalOS.Runtime.LegalReferences;
using Microsoft.AspNetCore.Builder;
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
}
