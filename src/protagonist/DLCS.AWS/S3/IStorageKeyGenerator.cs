﻿using DLCS.AWS.S3.Models;
using DLCS.Core.Types;
using Version = IIIF.ImageApi.Version;

namespace DLCS.AWS.S3;

public interface IStorageKeyGenerator
{
    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for storing tile-ready asset
    /// </summary>
    /// <param name="assetId">Unique identifier for Asset</param>
    /// <returns><see cref="ObjectInBucket"/> for tile-ready asset</returns>
    RegionalisedObjectInBucket GetStorageLocation(AssetId assetId);
    
    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for stored original file.
    /// This is for assets for "file" delivery, or images where origin is tile-optimised
    /// </summary>
    /// <param name="assetId">Unique identifier for Asset</param>
    /// <returns><see cref="ObjectInBucket"/> for stored original asset</returns>
    RegionalisedObjectInBucket GetStoredOriginalLocation(AssetId assetId);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for pre-generated thumbnail
    /// </summary>
    /// <param name="assetId">Unique identifier for Asset</param>
    /// <param name="longestEdge">Size of thumbnail</param>
    /// <param name="open">Whether thumbnail is open or requires auth</param>
    /// <returns><see cref="ObjectInBucket"/> for thumbnail</returns>
    ObjectInBucket GetThumbnailLocation(AssetId assetId, int longestEdge, bool open = true);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for legacy pre-generated thumbnail
    /// </summary>
    /// <param name="assetId">Unique identifier for Asset</param>
    /// <param name="width">Width of thumbnail</param>
    /// <param name="height">Height of thumbnail</param>
    /// <returns><see cref="ObjectInBucket"/> for thumbnail</returns>
    ObjectInBucket GetLegacyThumbnailLocation(AssetId assetId, int width, int height);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for json object storing pregenerated thumbnail sizes
    /// </summary>
    /// <param name="assetId">Unique identifier for Asset</param>
    /// <returns><see cref="ObjectInBucket"/> for sizes json</returns>
    ObjectInBucket GetThumbsSizesJsonLocation(AssetId assetId);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for largest pre-generated thumbnail.
    /// i.e. low.jpg
    /// </summary>
    /// <param name="assetId">Unique identifier for Asset</param>
    /// <returns><see cref="ObjectInBucket"/> for largest thumbnail</returns>
    ObjectInBucket GetLargestThumbnailLocation(AssetId assetId);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for root location of thumbnails for asset, rather than an individual file
    /// </summary>
    /// <param name="assetId">Unique identifier for Asset</param>
    /// <returns><see cref="ObjectInBucket"/> for largest thumbnail</returns>
    ObjectInBucket GetThumbnailsRoot(AssetId assetId);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> item in output bucket
    /// </summary>
    /// <param name="key">Object key</param>
    /// <returns><see cref="ObjectInBucket"/> for specified key in output bucket</returns>
    ObjectInBucket GetOutputLocation(string key);

    /// <summary>
    /// Get <see cref="RegionalisedObjectInBucket"/> for requested AV file 
    /// </summary>
    /// <param name="assetId">AssetId request is for</param>
    /// <param name="assetPath">Requested asset path</param>
    /// <returns><see cref="RegionalisedObjectInBucket"/> for AV file</returns>
    RegionalisedObjectInBucket GetTimebasedAssetLocation(AssetId assetId, string assetPath);
    
    /// <summary>
    /// Get <see cref="ObjectInBucket"/> for requested AV file 
    /// </summary>
    /// <param name="fullAssetPath">Full asset path, including AssetId</param>
    /// <returns><see cref="RegionalisedObjectInBucket"/> for AV file</returns>
    ObjectInBucket GetTimebasedAssetLocation(string fullAssetPath);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> item for info.json object
    /// </summary>
    /// <param name="assetId">AssetId request is for</param>
    /// <param name="imageServer">Name of image server being used</param>
    /// <param name="imageApiVersion">IIIF ImageApi version</param>
    /// <returns><see cref="ObjectInBucket"/> for specified key in output bucket</returns>
    ObjectInBucket GetInfoJsonLocation(AssetId assetId, string imageServer, Version imageApiVersion);

    /// <summary> 
    /// Get <see cref="RegionalisedObjectInBucket"/> item for an asset that is directly uploaded to the DLCS,
    /// and uses the DLCS' own origin bucket. 
    /// </summary>
    /// <param name="assetId">AssetId request is for</param>
    /// <returns><see cref="RegionalisedObjectInBucket"/> for uploaded asset</returns>
    RegionalisedObjectInBucket GetAssetAtOriginLocation(AssetId assetId);

    /// <summary>
    /// Ensure the <see cref="RegionalisedObjectInBucket"/> object has region property set.
    /// </summary>
    /// <param name="objectInBucket">Object to ensure region set on</param>
    void EnsureRegionSet(RegionalisedObjectInBucket objectInBucket);

    /// <summary>
    /// Get s3:// URI for specified <see cref="RegionalisedObjectInBucket"/> 
    /// </summary>
    /// <param name="objectInBucket">Object to get s3 URI for</param>
    /// <param name="useRegion">
    /// If true, include the region in URI. This isn't official but required by deliverator
    /// </param>
    /// <returns>s3:// uri</returns>
    Uri GetS3Uri(ObjectInBucket objectInBucket, bool useRegion = false);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> item for timebased asset that is to be transcoded
    /// </summary>
    /// <returns><see cref="ObjectInBucket"/> for specified key in timebased input bucket</returns>
    ObjectInBucket GetTimebasedInputLocation(AssetId assetId);
    
    /// <summary>
    /// Get <see cref="ObjectInBucket"/> item for timebased asset that has to been transcoded
    /// </summary>
    /// <returns><see cref="ObjectInBucket"/> for specified key in timebased input bucket</returns>
    ObjectInBucket GetTimebasedInputLocation(string key);
    
    /// <summary>
    /// Get <see cref="ObjectInBucket"/> item for timebased asset that has to been transcoded
    /// </summary>
    /// <returns><see cref="ObjectInBucket"/> for specified key in timebased input bucket</returns>
    ObjectInBucket GetTimebasedOutputLocation(string key);

    /// <summary>
    /// Get <see cref="ObjectInBucket"/> item for timebased metadata object
    /// </summary>
    /// <returns><see cref="ObjectInBucket"/> for specified asset's metadata file</returns>
    ObjectInBucket GetTimebasedMetadataLocation(AssetId assetId);
}