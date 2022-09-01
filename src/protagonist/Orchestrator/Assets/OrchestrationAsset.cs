﻿using System.Collections.Generic;
using DLCS.Core.Types;

namespace Orchestrator.Assets;

/// <summary>
/// Represents an asset during orchestration.
/// </summary>
public class OrchestrationAsset
{
    /// <summary>
    /// Get or set the AssetId for tracked Asset
    /// </summary>
    public AssetId AssetId { get; set; }

    /// <summary>
    /// Get boolean indicating whether asset is restricted or not.
    /// </summary>
    public bool RequiresAuth { get; set; }

    /// <summary>
    /// Gets list of roles associated with Asset
    /// </summary>
    public List<string> Roles { get; set; } = new();
}

public class OrchestrationFile : OrchestrationAsset
{
    /// <summary>
    /// Get or set Asset origin 
    /// </summary>
    public string Origin { get; set; }
}

public class OrchestrationImage : OrchestrationAsset
{
    /// <summary>
    /// Get or set asset Width
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Get or set asset Height
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Get maximum dimension available for unauthorised user
    /// </summary>
    public int MaxUnauthorised { get; set; }

    /// <summary>
    /// Gets or sets list of thumbnail sizes
    /// </summary>
    public List<int[]> OpenThumbs { get; set; } = new();
    
    /// <summary>
    /// Get or set Asset location in S3 
    /// </summary>
    public string? S3Location { get; set; }
}