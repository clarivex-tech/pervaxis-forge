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
using FluentAssertions;
using Pervaxis.Forge.Engine.Naming;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Naming;

public class NamingConventionTests
{
    [Theory]
    [InlineData("intake-service", "IntakeService")]
    [InlineData("billing-api", "BillingApi")]
    [InlineData("v2-order-service", "V2OrderService")]
    [InlineData("intake", "Intake")]
    [InlineData("x1-y2-z3", "X1Y2Z3")]
    public void ToPascalCase_ReturnsExpectedValue(string input, string expected)
    {
        NamingConvention.ToPascalCase(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ToPascalCase_RejectsInvalidInput(string? input)
    {
        Action act = () => NamingConvention.ToPascalCase(input!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("kebab");
    }

    [Theory]
    [InlineData("intake-service", "intake")]
    [InlineData("intake-SERVICE", "intake")]
    [InlineData("my-service-account", "my-service-account")]
    [InlineData("service", "service")]
    [InlineData("billing-service-v2", "billing-service-v2")]
    public void StripServiceSuffix_ReturnsExpectedValue(string input, string expected)
    {
        NamingConvention.StripServiceSuffix(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("intake-service", "intake")]
    [InlineData("billing-api", "billing")]
    [InlineData("single", "single")]
    [InlineData("v2-order-service", "v2")]
    [InlineData("multi-part-name", "multi")]
    public void GetFirstSegment_ReturnsExpectedValue(string input, string expected)
    {
        NamingConvention.GetFirstSegment(input).Should().Be(expected);
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
