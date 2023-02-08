using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var keyVaultName = builder.Configuration.GetValue<string>("KeyVault:Vault");
var thumbrint = builder.Configuration.GetValue<string>("KeyVault:Thumbprint");
var tenantId = builder.Configuration.GetValue<string>("TenantId");
var clientId = builder.Configuration.GetValue<string>("ClientId");

var kvUri = "https://" + keyVaultName + ".vault.azure.net";

// usign cert method
using var x509Store = new X509Store(StoreLocation.CurrentUser);

x509Store.Open(OpenFlags.ReadOnly);

var x509Certificate = x509Store.Certificates.Find(X509FindType.FindByThumbprint, thumbrint, false)
    .OfType<X509Certificate2>()
    .Single();

builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new ClientCertificateCredential(tenantId, clientId, x509Certificate));


var secretClient = new SecretClient(new Uri(kvUri), new ClientCertificateCredential(tenantId, clientId, x509Certificate));
var secret = await secretClient.GetSecretAsync("TestingKeyVault-AppConfig--AppSecret");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// used for managed identity method
var userAssignedClientId = app.Configuration.GetSection("UserAssignedClientId");
var managedIdentityClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId.Value}));
var managedIdentitySecret = await managedIdentityClient.GetSecretAsync("TestSecret");

app.MapGet("/key", () =>
{
    return secret;
});

app.MapGet("/managed-identity-key", () =>
{
    return managedIdentitySecret;
});

app.Run();
