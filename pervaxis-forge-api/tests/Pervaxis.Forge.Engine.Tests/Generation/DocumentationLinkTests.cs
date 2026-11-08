/*
 ************************************************************************
 * Copyright (C) 2026 Clarivex Technologies Private Limited
 * All Rights Reserved.
 *
 * NOTICE: All intellectual and technical concepts contained
 * herein are proprietary to Clarivex Technologies Private Limited
 * and may be covered by Indian and Foreign Patents,
 * patents in process, and are protected by trade secret or
 * copyright law. Dissemination of this information or reproduction
 * of this material is strictly forbidden unless prior written
 * permission is obtained from Clarivex Technologies Private Limited.
 *
 * Product:   Pervaxis Platform
 * Website:   https://clarivex.tech
 ************************************************************************
 */
using System.IO.Compression;
using System.Text.RegularExpressions;
using FluentAssertions;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

/// <summary>The developer entry documents of a REST print link only to files that exist in that print.</summary>
public class DocumentationLinkTests
{
    private static readonly Regex MarkdownLink = new(@"\]\(([^)\s#]+)(#[^)]*)?\)", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private static ForgeManifest Manifest(bool database, bool multiTenancy = false) => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "CLV",
        CloudProvider = "AWS",
        Database = database ? new DatabaseConfig { Engine = "postgresql", Name = "billing" } : null,
        MultiTenancy = multiTenancy,
        Cognito = new CognitoConfig { UserPoolId = "us-east-1_AbC123xyz", ClientIds = ["1h57kf5cpq17m0eml12EXAMPLE"] },
    };

    private static async Task<Dictionary<string, string>> GenerateAsync(ForgeManifest manifest)
    {
        var bytes = await new PrintGenerator().GenerateAsync(manifest, "AWS");
        using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name)))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName.Replace('\\', '/')] = reader.ReadToEnd();
        }

        return files;
    }

    private static bool IsEntryDocument(string path) =>
        path == "README.md"
        || path == "FORGE_DEVELOPER_GUIDE.md"
        || path.StartsWith("DeveloperReference/", StringComparison.Ordinal)
        || path.EndsWith("/Domains/README.md", StringComparison.Ordinal);

    private static string Resolve(string documentPath, string link)
    {
        var directory = documentPath.Contains('/') ? documentPath[..documentPath.LastIndexOf('/')] : string.Empty;
        var parts = new List<string>(directory.Length == 0 ? [] : directory.Split('/'));
        foreach (var segment in link.Split('/'))
        {
            if (segment == "..")
            {
                if (parts.Count > 0)
                    parts.RemoveAt(parts.Count - 1);
            }
            else if (segment is not ("." or ""))
            {
                parts.Add(segment);
            }
        }

        return string.Join('/', parts);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task EveryRelativeLink_InTheEntryDocuments_PointsToAFileInThePrint(bool database, bool multiTenancy)
    {
        var files = await GenerateAsync(Manifest(database, multiTenancy));

        var broken = new List<string>();
        foreach (var (path, content) in files.Where(f => f.Key.EndsWith(".md", StringComparison.Ordinal) && IsEntryDocument(f.Key)))
        {
            foreach (Match match in MarkdownLink.Matches(content))
            {
                var link = match.Groups[1].Value;
                if (link.Contains("://", StringComparison.Ordinal) || link.StartsWith("mailto:", StringComparison.Ordinal))
                    continue;

                var target = Resolve(path, link);
                if (!files.ContainsKey(target) && !files.Keys.Any(k => k.StartsWith(target + "/", StringComparison.Ordinal)))
                    broken.Add($"{path} -> {link}");
            }
        }

        broken.Should().BeEmpty();
    }

    [Fact]
    public async Task TheReadme_LeadsATenantPrintsDeveloperToTheTenancyDocs()
    {
        var files = await GenerateAsync(Manifest(database: true));

        var readme = files["README.md"];
        readme.Should().Contain("DeveloperReference/TenancyQuickStart.md")
            .And.Contain("DeveloperReference/Tenancy.md")
            .And.Contain("DeveloperReference/GoingMultiTenant.md")
            .And.Contain("Domains/README.md");

        files.Single(f => f.Key.EndsWith("/Domains/README.md", StringComparison.Ordinal)).Value
            .Should().Contain("TenancyQuickStart.md").And.NotContain("DbContext, repositories (if feature-scoped)");
        files[".claude/CLAUDE.md"].Should().Contain("guides/TENANCY_RULES.md");
        files.Keys.Should().Contain(".claude/guides/TENANCY_RULES.md");

        var guide = files["FORGE_DEVELOPER_GUIDE.md"];
        guide.Should().Contain(": TenantEntity<Guid>").And.Contain("ITenantContext");
        guide.Should().NotContain("GetDashboardAsync(string tenantId", "the tenant must never be a method parameter");
    }

    [Fact]
    public async Task APrintWithoutADatabase_HasNoTenancyDocs()
    {
        var files = await GenerateAsync(Manifest(database: false));

        files["README.md"].Should().NotContain("TenancyQuickStart");
        files.Keys.Should().NotContain("DeveloperReference/TenancyQuickStart.md");
        files.Keys.Should().NotContain(".claude/guides/TENANCY_RULES.md");
        files[".claude/CLAUDE.md"].Should().NotContain("TENANCY_RULES");
    }
}
