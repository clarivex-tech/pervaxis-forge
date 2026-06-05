using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.NuGet;
using Pervaxis.Forge.Engine.Templating;
using Pervaxis.Forge.Engine.Validation;

namespace Pervaxis.Forge.Engine.Generation;

public sealed class PrintGenerator
{
    private static readonly string GenesisBasePackageId = "Pervaxis.Genesis.Base";

    private readonly FileGenerator fileGenerator;
    private readonly ZipPackager zipPackager;
    private readonly INuGetVersionResolver? nuGetVersionResolver;

    public PrintGenerator(INuGetVersionResolver nuGetVersionResolver)
        : this(null, null, nuGetVersionResolver)
    {
    }

    public PrintGenerator(
        FileGenerator? fileGenerator = null,
        ZipPackager? zipPackager = null,
        INuGetVersionResolver? nuGetVersionResolver = null)
    {
        this.fileGenerator = fileGenerator ?? new FileGenerator();
        this.zipPackager = zipPackager ?? new ZipPackager();
        this.nuGetVersionResolver = nuGetVersionResolver;
    }

    public async Task<byte[]> GenerateAsync(ForgeManifest manifest, string cloudProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(cloudProvider);

        var validator = new ManifestValidator();
        var validationResult = validator.Validate(manifest);
        if (!validationResult.IsValid)
            throw new InvalidOperationException(string.Join("; ", validationResult.Errors));

        var genesisVersion = await ResolveGenesisVersionAsync(cancellationToken);
        var templateRoot = ResolveTemplateRoot(manifest.ServiceType);
        var model = TemplateModelBuilder.Build(manifest, cloudProvider, genesisVersion);
        var files = await fileGenerator.GenerateAsync(templateRoot, model, cancellationToken);
        return zipPackager.Package(files);
    }

    private async Task<string> ResolveGenesisVersionAsync(CancellationToken cancellationToken)
    {
        if (nuGetVersionResolver is null)
            return "3.2.0";

        return await nuGetVersionResolver.GetLatestVersionAsync(GenesisBasePackageId, cancellationToken);
    }

    private static string ResolveTemplateRoot(ServiceType serviceType) => serviceType switch
    {
        ServiceType.RestApi => "Templates/rest-api",
        ServiceType.AngularShell => "Templates/angular-shell",
        ServiceType.AngularMfe => "Templates/angular-microfrontend",
        ServiceType.Monolithic => "Templates/angular-monolith",
        ServiceType.Ionic => "Templates/ionic-mobile",
        ServiceType.GraphQL => "Templates/graphql",
        ServiceType.Grpc => "Templates/grpc",
        _ => throw new InvalidOperationException($"Unsupported service type: {serviceType}"),
    };
}
