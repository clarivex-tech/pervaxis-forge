using Scriban;

namespace Pervaxis.Forge.Engine.Templating;

public sealed class ScribanTemplateEngine : ITemplateEngine
{
    // GitHub Actions (${{ }}) and Angular template ({{ 'key' | pipe }}) expressions conflict with Scriban {{ }}.
    // Escape both before parsing and restore after rendering.
    private const string GitHubExprPlaceholder = " GH ";
    private const string AngularExprPlaceholder = " NG_EXPR ";

    public string Render(string templateText, TemplateModel model)
    {
        var processedText = templateText
            .Replace("${{", GitHubExprPlaceholder)
            .Replace("{{ '", AngularExprPlaceholder);
        var template = Template.Parse(processedText);

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
                ["cloud_provider"] = model.CloudProvider,
                ["current_year"] = model.CurrentYear,
                ["current_month"] = DateTime.UtcNow.ToString("MM"),
                ["current_day"] = DateTime.UtcNow.ToString("dd"),
                ["selected_modules"] = model.SelectedModules.Select(m => new Scriban.Runtime.ScriptObject
                {
                    ["name"] = m.Name,
                    ["package_name"] = m.PackageName,
                    ["di_extension_name"] = m.DiExtensionName,
                }).ToList(),
                ["selected_canvas_modules"] = model.SelectedCanvasModules.Select(m => new Scriban.Runtime.ScriptObject
                {
                    ["name"] = m.Name,
                    ["package_name"] = m.PackageName,
                    ["import_name"] = m.ImportName,
                }).ToList(),
                ["names"] = new Scriban.Runtime.ScriptObject
                {
                    ["namespace"] = model.DerivedNames.DotNetNamespace,
                    ["dot_net_namespace"] = model.DerivedNames.DotNetNamespace,
                    ["dot_net_class_name"] = model.DerivedNames.DotNetClassName,
                    ["angular_shell_component_name"] = model.DerivedNames.AngularShellComponentName,
                    ["angular_mfe_component_name"] = model.DerivedNames.AngularMfeComponentName,
                    ["angular_shell_route_path"] = model.DerivedNames.AngularShellRoutePath,
                    ["angular_mfe_route_path"] = model.DerivedNames.AngularMfeRoutePath,
                    ["project_file"] = model.DerivedNames.ProjectFile,
                    ["test_project_name"] = model.DerivedNames.TestProjectName,
                    ["solution_file"] = model.DerivedNames.SolutionFile,
                    ["api_base_route"] = model.DerivedNames.ApiBaseRoute,
                    ["database_schema"] = model.DerivedNames.DatabaseSchema,
                    ["sqs_prefix"] = model.DerivedNames.SqsPrefix,
                    ["cache_prefix"] = model.DerivedNames.CachePrefix,
                    ["docker_image"] = model.DerivedNames.DockerImage,
                    ["ecs_task_name"] = model.DerivedNames.EcsTaskName,
                    ["folder_name"] = model.DerivedNames.FolderName,
                    ["github_repo_path"] = model.DerivedNames.GitHubRepoPath,
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

        return result
            .Replace(GitHubExprPlaceholder, "${{")
            .Replace(AngularExprPlaceholder, "{{ '");
    }
}
