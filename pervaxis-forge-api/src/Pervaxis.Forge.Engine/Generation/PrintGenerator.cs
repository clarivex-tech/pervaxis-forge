using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Templating;
using Pervaxis.Forge.Engine.Validation;

namespace Pervaxis.Forge.Engine.Generation;

public sealed class PrintGenerator
{
    private readonly FileGenerator fileGenerator;
    private readonly ZipPackager zipPackager;

    public PrintGenerator(FileGenerator? fileGenerator = null, ZipPackager? zipPackager = null)
    {
        this.fileGenerator = fileGenerator ?? new FileGenerator();
        this.zipPackager = zipPackager ?? new ZipPackager();
    }

    public async Task<byte[]> GenerateAsync(ForgeManifest manifest, string cloudProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(cloudProvider);

        var validator = new ManifestValidator();
        var validationResult = validator.Validate(manifest);
        if (!validationResult.IsValid)
            throw new InvalidOperationException(string.Join("; ", validationResult.Errors));

        var templateRoot = ResolveTemplateRoot(manifest.ServiceType);
        var model = TemplateModelBuilder.Build(manifest, cloudProvider);
        var files = await fileGenerator.GenerateAsync(templateRoot, model, cancellationToken);
        return zipPackager.Package(files);
    }

    private static string ResolveTemplateRoot(ServiceType serviceType) => serviceType switch
    {
        ServiceType.RestApi => "Templates/rest-api",
        ServiceType.AngularShell => "Templates/angular-shell",
        ServiceType.AngularMfe => "Templates/angular-microfrontend",
        ServiceType.GraphQL => "Templates/graphql",
        ServiceType.Grpc => "Templates/grpc",
        _ => throw new InvalidOperationException($"Unsupported service type: {serviceType}"),
    };
}
