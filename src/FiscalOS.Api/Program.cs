using FiscalOS.Api.Endpoints;
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.MapMicroenterpriseEligibilityEndpoint();

app.MapGet("/", () => "FiscalOS API");

app.Run();