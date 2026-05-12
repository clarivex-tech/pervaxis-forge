namespace Pervaxis.Forge.Engine.Templating;

public interface ITemplateEngine
{
    string Render(string templateText, TemplateModel model);
}
