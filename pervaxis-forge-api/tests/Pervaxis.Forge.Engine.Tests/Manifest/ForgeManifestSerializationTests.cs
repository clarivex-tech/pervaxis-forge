using System.Text.Json;
using FluentAssertions;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Manifest;

public class ForgeManifestSerializationTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void SerializeAndDeserialize_RoundTripsCompleteManifest()
    {
        var manifest = new ForgeManifest
        {
            Product = "Pervaxis Forge",
            VerticalSlug = "clarivolt",
            ServiceName = "intake-service",
            ServiceType = ServiceType.RestApi,
            ComponentPrefix = "clv",
            CloudProvider = "AWS",
            Database = new DatabaseConfig
            {
                Engine = "postgresql",
                Name = "intake_db",
                Username = "forge_user",
                Password = "secret",
                Host = "localhost",
                Port = 5432,
                SslMode = "Require",
            },
            Queue = new QueueConfig
            {
                Provider = "SQS",
                Name = "intake-queue",
                Enabled = true,
                MaxRetries = 5,
            },
            Api = new ApiConfig
            {
                BasePath = "/api/v1/intake",
                SwaggerEnabled = true,
                CorsEnabled = true,
            },
            Angular = new AngularConfig
            {
                WorkspaceName = "clarivolt-workspace",
                ProjectName = "intake-shell",
                RoutePrefix = "intake",
            },
            Metadata = new ManifestMetadata
            {
                Author = "platform",
                Description = "Manifest used for engine serialization tests.",
                Version = "1.0.0",
            },
        };

        var json = JsonSerializer.Serialize(manifest, Options);
        var roundTripped = JsonSerializer.Deserialize<ForgeManifest>(json, Options);

        roundTripped.Should().BeEquivalentTo(manifest);
    }

    [Fact]
    public void Serialize_UsesWebDefaults()
    {
        var manifest = new ForgeManifest
        {
            Product = "Pervaxis Forge",
            VerticalSlug = "clarivolt",
            ServiceName = "billing-service",
            ServiceType = ServiceType.Worker,
            ComponentPrefix = "clv",
            CloudProvider = "AWS",
        };

        var json = JsonSerializer.Serialize(manifest, Options);

        json.Should().Contain("\"product\"");
        json.Should().Contain("\"verticalSlug\"");
        json.Should().Contain("\"serviceType\":2");
    }
}
