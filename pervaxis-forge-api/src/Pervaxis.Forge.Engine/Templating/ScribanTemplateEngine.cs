using Scriban;

namespace Pervaxis.Forge.Engine.Templating;

public sealed class ScribanTemplateEngine : ITemplateEngine
{
    public string Render(string templateText, TemplateModel model)
    {
        var template = Template.Parse(templateText);

        if (template.HasErrors)
            throw new InvalidOperationException(string.Join(Environment.NewLine, template.Messages.Select(message => message.Message)));

        var context = new Scriban.Runtime.ScriptObject
        {
            ["Product"] = model.Manifest.Product,
            ["VerticalSlug"] = model.Manifest.VerticalSlug,
            ["ServiceName"] = model.Manifest.ServiceName,
            ["ServiceType"] = model.Manifest.ServiceType.ToString(),
            ["ComponentPrefix"] = model.Manifest.ComponentPrefix,
            ["CloudProvider"] = model.Manifest.CloudProvider,
            ["DotNetNamespace"] = model.DerivedNames.DotNetNamespace,
            ["DotNetClassName"] = model.DerivedNames.DotNetClassName,
            ["AngularShellComponentName"] = model.DerivedNames.AngularShellComponentName,
            ["AngularMfeComponentName"] = model.DerivedNames.AngularMfeComponentName,
            ["AngularShellRoutePath"] = model.DerivedNames.AngularShellRoutePath,
            ["AngularMfeRoutePath"] = model.DerivedNames.AngularMfeRoutePath,
            ["model"] = new Scriban.Runtime.ScriptObject
            {
                ["manifest"] = model.Manifest,
                ["names"] = new Scriban.Runtime.ScriptObject
                {
                    ["dotNetNamespace"] = model.DerivedNames.DotNetNamespace,
                    ["dotNetClassName"] = model.DerivedNames.DotNetClassName,
                    ["angularShellComponentName"] = model.DerivedNames.AngularShellComponentName,
                    ["angularMfeComponentName"] = model.DerivedNames.AngularMfeComponentName,
                    ["angularShellRoutePath"] = model.DerivedNames.AngularShellRoutePath,
                    ["angularMfeRoutePath"] = model.DerivedNames.AngularMfeRoutePath,
                },
            },
            ["secrets"] = new Scriban.Runtime.ScriptObject
            {
                ["SONAR_TOKEN"] = string.Empty,
                ["NUGET_API_KEY"] = string.Empty,
            },
        };

        string result;

        try
        {
            result = template.Render(context);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to render Scriban template.", ex);
        }

        if (template.HasErrors)
            throw new InvalidOperationException(string.Join(Environment.NewLine, template.Messages.Select(message => message.Message)));

        return result;
    }
}
