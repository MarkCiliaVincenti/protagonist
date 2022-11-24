﻿using DLCS.Model.Policies;
using FluentAssertions;
using Xunit;

namespace DLCS.Model.Tests.Policies;

public class ImageOptimisationPolicyXTests
{
    [Theory]
    [InlineData("video-max")]
    [InlineData("")]
    [InlineData(" ")]
    public void IsNoOp_False(string id)
    {
        var policy = new ImageOptimisationPolicy { Id = id };

        policy.IsNotProcessed().Should().BeFalse();
    }
    
    [Fact]
    public void IsNoOp_True()
    {
        var policy = new ImageOptimisationPolicy { Id = "none" };

        policy.IsNotProcessed().Should().BeTrue();
    }
    
    [Theory]
    [InlineData("video-max")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsNoOpIdentifier_False(string id)
    {
        ImageOptimisationPolicyX.IsNotProcessedIdentifier(id).Should().BeFalse();
    }
    
    [Fact]
    public void IsNoOpIdentifier_True()
    {
        ImageOptimisationPolicyX.IsNotProcessedIdentifier("none").Should().BeTrue();
    }
}