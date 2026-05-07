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

using FluentAssertions;
using Pervaxis.Forge.Engine.Naming;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Naming;

public class NamingConventionTests
{
    // Smoke test — verifies the assembly loads and the class exists in the expected namespace.
    // Full 50+ test suite comes in the naming-convention implementation sprint.
    [Fact]
    public void NamingConvention_ClassExists_InExpectedNamespace()
    {
        var type = typeof(NamingConvention);

        type.Namespace.Should().Be("Pervaxis.Forge.Engine.Naming");
        type.IsClass.Should().BeTrue();
        type.IsAbstract.Should().BeTrue(); // static classes are abstract + sealed in IL
    }

    [Theory]
    [InlineData("CLV", "clv")]
    [InlineData("clv", "clv")]
    [InlineData("CF", "cf")]
    [InlineData("forge", "forge")]   // 5 chars — valid upper bound
    [InlineData("AB", "ab")]
    public void GetComponentPrefix_NormalisesRegisteredAbbreviation(string input, string expected)
    {
        var result = NamingConvention.GetComponentPrefix(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("x")]          // too short
    [InlineData("toolong")]    // too long
    [InlineData("cl1")]        // digit
    [InlineData("cl-v")]       // hyphen
    public void GetComponentPrefix_ThrowsOnInvalidInput(string input)
    {
        var action = () => NamingConvention.GetComponentPrefix(input);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("registeredAbbreviation");
    }
}
