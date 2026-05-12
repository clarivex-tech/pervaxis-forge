using FluentAssertions;
using Pervaxis.Forge.Engine.Templating;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Templating;

public class TemplateLoaderTests
{
    private readonly TemplateLoader loader = new(typeof(Pervaxis.Forge.Engine.Templating.TemplateLoader).Assembly);

    [Fact]
    public async Task LoadAsync_ReturnsEmbeddedTemplateText()
    {
        var content = await loader.LoadAsync("test-template.sbn");

        content.Should().Contain("Product:");
        content.Should().Contain("Service:");
    }

    [Fact]
    public async Task LoadAsync_ThrowsWhenTemplateIsMissing()
    {
        Func<Task> action = () => loader.LoadAsync("missing-template.sbn");

        await action.Should().ThrowAsync<FileNotFoundException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LoadAsync_ThrowsWhenResourceSuffixIsEmpty(string resourceSuffix)
    {
        Func<Task> action = () => loader.LoadAsync(resourceSuffix);

        await action.Should().ThrowAsync<ArgumentException>();
    }
}
