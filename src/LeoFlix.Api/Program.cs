using LeoFlix.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.CustomSchemaIds(t => t.FullName?.Replace("+", ".")));

builder.AddAzureBlobClient("blobs");

builder.Services
    .AddCustomCors()
    .AddFileForm()
    .AddEndpoints()
    .AddFeatures();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpoints();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.Run();