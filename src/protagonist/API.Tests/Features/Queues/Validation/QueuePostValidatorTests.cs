﻿using System;
using API.Features.Queues.Validation;
using API.Settings;
using DLCS.HydraModel;
using DLCS.Model.Assets;
using FluentValidation.TestHelper;
using Hydra.Collections;
using Microsoft.Extensions.Options;
using AssetFamily = DLCS.HydraModel.AssetFamily;

namespace API.Tests.Features.Queues.Validation;

public class QueuePostValidatorTests
{
    private readonly QueuePostValidator sut;

    public QueuePostValidatorTests()
    {
        var apiSettings = new ApiSettings { MaxBatchSize = 4 };
        sut = new QueuePostValidator(Options.Create(apiSettings));
    }

    [Fact]
    public void Members_Null()
    {
        var model = new HydraCollection<Image>();
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(r => r.Members);
    }
    
    [Fact]
    public void Members_Empty()
    {
        var model = new HydraCollection<Image> { Members = Array.Empty<Image>() };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(r => r.Members);
    }

    [Fact]
    public void Members_GreaterThanMaxBatchSize()
    {
        var model = new HydraCollection<Image>
            { Members = new[] { new Image(), new Image(), new Image(), new Image(), new Image() } };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(r => r.Members);
    }

    [Fact]
    public void Members_ContainsDuplicateIds()
    {
        var model = new HydraCollection<Image>
        {
            Members = new[]
            {
                new Image { ModelId = "1/2/foo" },
                new Image { ModelId = "1/2/bar" },
                new Image { ModelId = "1/2/foo" },
            }
        };
        var result = sut.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(r => r.Members)
            .WithErrorMessage("Members contains 1 duplicate Id(s): 1/2/foo");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Member_ModelId_NullOrEmpty(string id)
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { ModelId = id } } };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Members[0].ModelId");
    }
    
    [Fact]
    public void Member_Space_Default()
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { Space = 0 } } };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Members[0].Space");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Member_MediaType_NullOrEmpty(string mediaType)
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { MediaType = mediaType } } };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Members[0].MediaType");
    }

    [Fact]
    public void Member_Batch_Provided()
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { Batch = "10" } } };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Members[0].Batch");
    }
    
    [Fact]
    public void Member_Width_Provided()
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { Width = 10 } } };
        var result = sut.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor("Members[0].Width")
            .WithErrorMessage("Should not include width");
    }

    [Fact]
    public void Member_Height_Provided()
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { Height = 10 } } };
        var result = sut.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor("Members[0].Height")
            .WithErrorMessage("Should not include height");
    }
    
    [Fact]
    public void Member_Duration_Provided()
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { Duration = 10 } } };
        var result = sut.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor("Members[0].Duration")
            .WithErrorMessage("Should not include duration");
    }
    
    [Fact]
    public void Member_Finished_Provided()
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { Finished = DateTime.Today } } };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Members[0].Finished");
    }
    
    [Fact]
    public void Member_Created_Provided()
    {
        var model = new HydraCollection<Image> { Members = new[] { new Image { Created = DateTime.Today } } };
        var result = sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Members[0].Created");
    }
}