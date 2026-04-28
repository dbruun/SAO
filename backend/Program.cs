using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS – allow all origins for local dev
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Identity Verification API", Version = "v1" }));

// Register document extraction – use Azure if configured, otherwise fall back to mock
var azureEndpoint = builder.Configuration["AzureDocumentIntelligence:Endpoint"];
var azureApiKey   = builder.Configuration["AzureDocumentIntelligence:ApiKey"];

if (!string.IsNullOrWhiteSpace(azureEndpoint) && !string.IsNullOrWhiteSpace(azureApiKey))
{
    builder.Services.AddSingleton(_ =>
        new DocumentAnalysisClient(new Uri(azureEndpoint), new AzureKeyCredential(azureApiKey)));
    builder.Services.AddScoped<IDocumentExtractionService, AzureDocumentIntelligenceExtractionService>();
}
else
{
    builder.Services.AddScoped<IDocumentExtractionService, MockDocumentExtractionService>();
}

builder.Services.AddScoped<IAddressValidationService, StubAddressValidationService>();
builder.Services.AddScoped<ILivenessService, StubLivenessService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IBackendRecordsService, MockBackendRecordsService>();
builder.Services.AddScoped<IDocumentScanService, DocumentScanService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
