using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


var keyVaultName = app.Configuration.GetSection("KeyVaultName");
var kvUri = "https://" + keyVaultName.Value + ".vault.azure.net";

var userAssignedClientId = app.Configuration.GetSection("UserAssignedClientId");

var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId.Value}));

var secret = await client.GetSecretAsync("TestSecret");


app.MapGet("/key", () =>
{
    return secret.Value.Value;
})
.WithName("GetKey");

app.MapGet("/test", () =>
{
    return "test";
})
.WithName("Test");

app.Run();
