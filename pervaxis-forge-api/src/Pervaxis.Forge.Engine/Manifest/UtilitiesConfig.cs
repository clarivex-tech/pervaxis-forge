namespace Pervaxis.Forge.Engine.Manifest;

public sealed record UtilitiesConfig
{
    public bool PdfEnabled { get; init; } = false;
    public bool TemplateEngineEnabled { get; init; } = false;
    public bool HashingEnabled { get; init; } = false;
}
