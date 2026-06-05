using Scriban;

namespace Pervaxis.Forge.Engine.Templating;

public sealed class ScribanTemplateEngine : ITemplateEngine
{
    // GitHub Actions (${{ }}) and Angular template ({{ 'key' | pipe }}) expressions conflict with Scriban {{ }}.
    // Escape both before parsing and restore after rendering.
    private const string GitHubExprPlaceholder = " GH ";
    private const string AngularExprPlaceholder = " NG_EXPR ";
    private const string AngularIfPlaceholder = " NG_IF ";
    private const string AngularForPlaceholder = " NG_FOR ";
    private const string AngularSwitchPlaceholder = " NG_SWITCH ";
    private const string AngularOptionalChainPlaceholder = " NG_OPTCHAIN ";
    private const string AngularNonNullPlaceholder = " NG_NONNULL ";

    public string Render(string templateText, TemplateModel model)
    {
        var processedText = templateText
            .Replace("${{", GitHubExprPlaceholder)
            .Replace("{{ '", AngularExprPlaceholder);
        processedText = processedText
            .Replace("@if", AngularIfPlaceholder)
            .Replace("@for", AngularForPlaceholder)
            .Replace("@switch", AngularSwitchPlaceholder)
            .Replace("?.", AngularOptionalChainPlaceholder)
            .Replace(")!", AngularNonNullPlaceholder);
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
                ["manifest"] = new Scriban.Runtime.ScriptObject
                {
                    ["product"] = model.Manifest.Product,
                    ["vertical_slug"] = model.Manifest.VerticalSlug,
                    ["service_name"] = model.Manifest.ServiceName,
                    ["service_type"] = model.Manifest.ServiceType.ToString(),
                    ["component_prefix"] = model.Manifest.ComponentPrefix,
                    ["cloud_provider"] = model.Manifest.CloudProvider,
                    ["metadata"] = new Scriban.Runtime.ScriptObject
                    {
                        ["author"] = model.Manifest.Metadata?.Author,
                        ["description"] = model.Manifest.Metadata?.Description,
                        ["version"] = model.Manifest.Metadata?.Version,
                        ["created_by"] = model.Manifest.Metadata?.CreatedBy,
                        ["created_at_utc"] = model.Manifest.Metadata?.CreatedAtUtc?.ToString("O"),
                    },
                    ["auth"] = model.Manifest.Auth is { } auth
                        ? new Scriban.Runtime.ScriptObject
                        {
                            ["api_key_enabled"] = auth.ApiKeyEnabled,
                            ["jwt_enabled"] = auth.JwtEnabled,
                            ["mtls_enabled"] = auth.MtlsEnabled,
                        }
                        : null,
                    ["resilience"] = model.Manifest.Resilience is { } resilience
                        ? new Scriban.Runtime.ScriptObject
                        {
                            ["retry_enabled"] = resilience.RetryEnabled,
                            ["circuit_breaker_enabled"] = resilience.CircuitBreakerEnabled,
                            ["timeout_enabled"] = resilience.TimeoutEnabled,
                            ["internal_http_client"] = resilience.InternalHttpClient,
                            ["external_http_client"] = resilience.ExternalHttpClient,
                        }
                        : null,
                    ["observability"] = model.Manifest.Observability is { } observability
                        ? new Scriban.Runtime.ScriptObject
                        {
                            ["serilog_enabled"] = observability.SerilogEnabled,
                            ["cloud_watch_enabled"] = observability.CloudWatchEnabled,
                            ["open_telemetry_enabled"] = observability.OpenTelemetryEnabled,
                            ["correlation_id_enabled"] = observability.CorrelationIdEnabled,
                            ["prometheus_enabled"] = observability.PrometheusEnabled,
                        }
                        : null,
                    ["validation"] = model.Manifest.Validation is { } validation
                        ? new Scriban.Runtime.ScriptObject
                        {
                            ["fluent_validation_enabled"] = validation.FluentValidationEnabled,
                        }
                        : null,
                    ["background_jobs"] = model.Manifest.BackgroundJobs is { } backgroundJobs
                        ? new Scriban.Runtime.ScriptObject
                        {
                            ["provider"] = backgroundJobs.Provider,
                        }
                        : null,
                    ["utilities"] = model.Manifest.Utilities is { } utilities
                        ? new Scriban.Runtime.ScriptObject
                        {
                            ["pdf_enabled"] = utilities.PdfEnabled,
                            ["template_engine_enabled"] = utilities.TemplateEngineEnabled,
                            ["hashing_enabled"] = utilities.HashingEnabled,
                        }
                        : null,
                    ["multi_tenancy"] = model.Manifest.MultiTenancy,
                    ["database"] = model.Manifest.Database is { } db
                        ? new Scriban.Runtime.ScriptObject
                        {
                            ["engine"] = db.Engine,
                            ["name"] = db.Name,
                            ["host"] = db.Host,
                            ["port"] = db.Port,
                        }
                        : null,
                    ["has_messaging"] = model.Manifest.Queue is not null,
                },
                ["cloud_provider"] = model.CloudProvider,
                ["current_year"] = model.CurrentYear,
                ["current_month"] = DateTime.UtcNow.ToString("MM"),
                ["current_day"] = DateTime.UtcNow.ToString("dd"),
                ["selected_modules"] = model.SelectedModules.Select(m => new Scriban.Runtime.ScriptObject
                {
                    ["name"] = m.Name,
                    ["package_name"] = m.PackageName,
                    ["di_extension_name"] = m.DiExtensionName,
                    ["version"] = m.Version,
                }).ToList(),
                ["selected_canvas_modules"] = model.SelectedCanvasModules.Select(m => new Scriban.Runtime.ScriptObject
                {
                    ["name"] = m.Name,
                    ["package_name"] = m.PackageName,
                    ["import_name"] = m.ImportName,
                    ["version"] = m.Version,
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
            .Replace(AngularExprPlaceholder, "{{ '")
            .Replace(AngularIfPlaceholder, "@if")
            .Replace(AngularForPlaceholder, "@for")
            .Replace(AngularSwitchPlaceholder, "@switch")
            .Replace(AngularOptionalChainPlaceholder, "?.")
            .Replace(AngularNonNullPlaceholder, ")!");
    }
}
