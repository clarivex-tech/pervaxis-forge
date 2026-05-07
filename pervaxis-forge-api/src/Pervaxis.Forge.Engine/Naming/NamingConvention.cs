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

namespace Pervaxis.Forge.Engine.Naming;

/// <summary>
/// Static utility methods for name transformation and component prefixing in Pervaxis Forge.
/// </summary>
public static class NamingConvention
{
    /// <summary>
    /// Normalises a pre-registered component abbreviation: lowercases the input and validates it.
    /// </summary>
    /// <param name="registeredAbbreviation">The abbreviation to normalise (2–5 letters, no digits or hyphens).</param>
    /// <returns>The lowercased abbreviation.</returns>
    /// <exception cref="ArgumentException">Thrown if the abbreviation is null, empty, outside the 2–5 character range, or contains non-letter characters.</exception>
    public static string GetComponentPrefix(string registeredAbbreviation)
    {
        if (registeredAbbreviation == null)
        {
            throw new ArgumentException("Abbreviation cannot be null.", nameof(registeredAbbreviation));
        }

        var lowercased = registeredAbbreviation.ToLowerInvariant();

        if (lowercased.Length == 0)
        {
            throw new ArgumentException("Abbreviation cannot be empty.", nameof(registeredAbbreviation));
        }

        if (lowercased.Length < 2 || lowercased.Length > 5)
        {
            throw new ArgumentException("Abbreviation must be between 2 and 5 characters long.", nameof(registeredAbbreviation));
        }

        if (!lowercased.All(c => char.IsLetter(c)))
        {
            throw new ArgumentException("Abbreviation must contain only letters (a–z).", nameof(registeredAbbreviation));
        }

        return lowercased;
    }

    // TODO: implement ToPascalCase, StripServiceSuffix, GetFirstSegment, DeriveAllNames — per FORGE_TECHNICAL_SPECIFICATION.md §10
}
