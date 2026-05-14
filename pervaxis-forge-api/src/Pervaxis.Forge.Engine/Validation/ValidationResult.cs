namespace Pervaxis.Forge.Engine.Validation;

public sealed record ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public required IReadOnlyList<string> Errors { get; init; }
}
