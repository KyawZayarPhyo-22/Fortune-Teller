using FortuneTeller.Admin.App.Components;
using FortuneTeller.Admin.App.Services;
using FortuneTeller.Domain;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 15 * 1024 * 1024; // 15 MB payload support for camera frames
    });

builder.Services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
}).AddHubOptions(options =>
{
    options.MaximumReceiveMessageSize = 15 * 1024 * 1024;
});

// Add Domain & Client Services
builder.Services.AddDomainServices();
builder.Services.AddScoped<FortuneStateService>();
builder.Services.AddScoped<FortuneClientService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".tflite"] = "application/octet-stream";
provider.Mappings[".binarypb"] = "application/octet-stream";
provider.Mappings[".data"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
