using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("blobs");

var api = builder.AddProject<LeoFlix_Api>("api")
    .WithReference(blobs)
    .WaitFor(blobs);

builder.AddNpmApp("reactvite", "../../../client-leoflix", "dev")
    .WithReference(api)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT", targetPort: 5173)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();