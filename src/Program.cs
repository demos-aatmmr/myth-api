using Microsoft.EntityFrameworkCore;
using MythApi.Gods.Interfaces;
using MythApi.Gods.DBRepositories;
using MythApi.Common.Database;
using MythApi.Endpoints.v1;
using MythApi.Mythologies.DBRepositories;
using MythApi.Mythologies.Interfaces;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var keyVaultName = builder.Configuration["MYTH_KeyVaultName"];
var uri = new Uri($"https://{keyVaultName}.vault.azure.net/");
var credential = new DefaultAzureCredential();

builder.Configuration.AddAzureKeyVault(uri, credential);

var connectionString = $"Host={builder.Configuration["dbHost"]};Database={builder.Configuration["dbName"]};Username={builder.Configuration["adminUsername"]};Password={builder.Configuration["adminPassword"]}";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services
    .AddScoped<IGodRepository, GodRepository>()
    .AddScoped<IMythologyRepository, MythologyRepository>();

var app = builder.Build();

app.RegisterGodEndpoints();
app.RegisterMythologiesEndpoints();
app.UseSwagger();
app.UseSwaggerUI();


app.Run();
