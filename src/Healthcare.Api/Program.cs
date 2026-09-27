using Healthcare.Api.Workers;
using Healthcare.Api.Options;
using OpenTelemetry.Trace;
using Healthcare.Api.Data;
using Healthcare.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<Healthcare.Api.Options.AzureStorageOptions>(
    builder.Configuration.GetSection("AzureStorage"));

var useAzureStorage =
    builder.Configuration.GetValue<bool>("AzureStorage:Enabled");

if (useAzureStorage)
{
    builder.Services.AddScoped<
        Healthcare.Api.Storage.IFileStorageService,
        Healthcare.Api.Storage.AzureBlobStorageService>();
}
else
{
    builder.Services.AddScoped<
        Healthcare.Api.Storage.IFileStorageService,
        Healthcare.Api.Storage.LocalFileStorageService>();
}

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<Healthcare.Api.Data.HealthcareDbContext>(
        "database",
        tags: new[] { "ready" });

builder.Services.AddSingleton<DocumentProcessingQueue>();
builder.Services.AddHostedService<DocumentProcessingWorker>();

builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection("Ollama"));

builder.Services.Configure<RagOptions>(
    builder.Configuration.GetSection("Rag"));

builder.Services.Configure<ChunkingOptions>(
    builder.Configuration.GetSection("Chunking"));

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });


builder.Services.AddControllers();

builder.Services.AddDbContext<HealthcareDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DocumentService>();
builder.Services.Configure<DocumentIntelligenceOptions>(
    builder.Configuration.GetSection("DocumentIntelligence"));

var useDocumentIntelligence =
    builder.Configuration.GetValue<bool>("DocumentIntelligence:Enabled");

if (useDocumentIntelligence)
{
    builder.Services.AddScoped<
        IDocumentTextExtractor,
        AzureDocumentIntelligenceService>();
}
else
{
    builder.Services.AddScoped<
        IDocumentTextExtractor,
        OcrService>();
}
builder.Services.AddScoped<DocumentClassificationService>();
builder.Services.AddScoped<StructuredExtractionService>();
builder.Services.AddScoped<DocumentValidationService>();
builder.Services.AddScoped<DocumentChunkingService>();
builder.Services.AddScoped<DocumentRetrievalService>();
builder.Services.AddHttpClient<EmbeddingService>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>()
        .Value;

    client.BaseAddress =
        new Uri(options.BaseUrl);

    client.Timeout =
        TimeSpan.FromSeconds(
            options.TimeoutSeconds);
});
builder.Services.AddScoped<EmbeddingBackfillService>();
builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection("AzureOpenAI"));

var useAzureOpenAI =
    builder.Configuration.GetValue<bool>("AzureOpenAI:Enabled");

if (useAzureOpenAI)
{
    builder.Services.AddScoped<
        IChatService,
        AzureOpenAIChatService>();

    builder.Services.AddScoped<
        IEmbeddingService,
        AzureOpenAIEmbeddingService>();
}
else
{
    builder.Services.AddScoped<IChatService>(
        serviceProvider =>
            serviceProvider.GetRequiredService<OllamaService>());

    builder.Services.AddScoped<IEmbeddingService>(
        serviceProvider =>
            serviceProvider.GetRequiredService<EmbeddingService>());
}

builder.Services.AddScoped<RagService>();

builder.Services.AddHttpClient<OllamaService>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>()
        .Value;

    client.BaseAddress =
        new Uri(options.BaseUrl);

    client.Timeout =
        TimeSpan.FromSeconds(
            options.TimeoutSeconds);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services
    .AddHealthChecks()
    .AddCheck<Healthcare.Api.Health.OllamaHealthCheck>(
        "ollama",
        tags: new[] { "ready" });
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<Healthcare.Api.Data.HealthcareDbContext>();

    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Healthcare.Api.Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<Healthcare.Api.Middleware.GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.MapHealthChecks(
    "/health/live",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

app.MapHealthChecks(
    "/health/ready",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check =>
            check.Tags.Contains("ready")
    });
app.Run();















public partial class Program { }















