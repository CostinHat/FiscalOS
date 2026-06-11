using FiscalOS.Api;
using FiscalOS.Api.Endpoints;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFiscalOSApi();

var app = builder.Build();
app.MapMicroenterpriseEligibilityEndpoint();

app.MapGet("/", () => "FiscalOS API");

app.Run();
