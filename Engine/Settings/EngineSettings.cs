﻿namespace Engine.Settings;

public class EngineSettings
{
    public ImageIngestSettings ImageIngest { get; set; }
    
    /// <summary>
    /// A collection of customer-specific overrides, keyed by customerId.
    /// </summary> 
    public Dictionary<string, CustomerOverridesSettings> CustomerOverrides { get; set; } =
        new Dictionary<string, CustomerOverridesSettings>();
    
    /// <summary>
    /// Get CustomerSpecificSettings, if found. 
    /// </summary>
    /// <param name="customerId">CustomerId to get settings for.</param>
    /// <returns>Customer specific overrides, or default if not found.</returns>
    public CustomerOverridesSettings GetCustomerSettings(int customerId)
        => CustomerOverrides.TryGetValue(customerId.ToString(), out var settings)
            ? settings
            : CustomerOverridesSettings.Empty;
}

/// <summary>
/// Settings directly related to image ingestion
/// </summary>
/// <remarks>These are for use with Tizer/Appetiser settings.</remarks>
public class ImageIngestSettings
{
    /// <summary>
    /// Path template for where file will be copied to
    /// </summary>
    public string SourceTemplate { get; set; }

    /// <summary>
    /// Path template for where derivatives will be generated into
    /// </summary>
    public string DestinationTemplate { get; set; }

    /// <summary>
    /// Path template for where thumbnail derivatives will generated into
    /// </summary>
    public string ThumbsTemplate { get; set; }

    /// <summary>
    /// S3 template for where derivatives will be copied to
    /// </summary>
    [Obsolete("Use S3KeyGenerator")]
    public string S3Template { get; set; }

    /// <summary>
    /// URI of downstream image/derivative processor
    /// </summary>
    public Uri ImageProcessorUrl { get; set; }

    /// <summary>
    /// Timeout for requests to image-processor 
    /// </summary>
    public int ImageProcessorTimeoutMs { get; set; } = 300000;

    /// <summary>
    /// Root folder for main container
    /// </summary>
    public string ScratchRoot { get; set; }

    /// <summary>
    /// Root folder for use by Image-Processor sidecar
    /// </summary>
    public string ImageProcessorRoot { get; set; }

    /// <summary>
    /// Get the root folder, if forImageProcessor will ensure that it is compatible with needs of image-processor
    /// sidecar.
    /// </summary>
    public string GetRoot(bool forImageProcessor = false)
    {
        if (!forImageProcessor) return ScratchRoot;

        return string.IsNullOrEmpty(ImageProcessorRoot)
            ? ScratchRoot
            : ImageProcessorRoot;
    }
}