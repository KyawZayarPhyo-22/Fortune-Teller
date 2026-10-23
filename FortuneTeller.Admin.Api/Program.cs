using FortuneTeller.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDomainServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    Application = "Fortune Teller Admin API",
    Status = "Online",
    Timestamp = DateTime.UtcNow,
    Orchestrator = "Reading Orchestrator active",
    OpenApiDocs = "/openapi/v1.json"
}));

app.Run();
