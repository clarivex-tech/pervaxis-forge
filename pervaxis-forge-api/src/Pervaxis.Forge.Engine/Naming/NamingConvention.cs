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

using System;
using System.Linq;

namespace Pervaxis.Forge.Engine.Naming;

public static class NamingConvention
{
    public static string ToPascalCase(string kebab)
    {
        if (string.IsNullOrWhiteSpace(kebab))
        {
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(kebab));
        }

        return string.Join(
            string.Empty,
            kebab.Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => char.ToUpperInvariant(segment[0]) + segment[1..].ToLowerInvariant()));
    }

    public static string StripServiceSuffix(string name)
    {
        if (name.EndsWith("-service", StringComparison.OrdinalIgnoreCase))
        {
            return name[..^8];
        }

        return name;
    }

    public static string GetFirstSegment(string name)
    {
        var hyphenIndex = name.IndexOf('-');
        return hyphenIndex < 0 ? name : name[..hyphenIndex];
    }

    public static string GetComponentPrefix(string product)
    {
        var normalized = product.ToLowerInvariant();
        if (normalized == "clarivolt")
        {
            return "clv";
        }

        return normalized.Length < 3 ? normalized : normalized[..3];
    }
}
