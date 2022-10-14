﻿using System;
using DLCS.Core.Exceptions;

namespace DLCS.Core.Types;

/// <summary>
/// A record that represents an identifier for a DLCS Asset.
/// </summary>
public class AssetId
{
    /// <summary>Id of customer</summary>
    public int Customer { get; }

    /// <summary>Id of space</summary>
    public int Space { get; }

    /// <summary>Id of asset</summary>
    public string Asset { get; }
    
    /// <summary>
    /// A record that represents an identifier for a DLCS Asset.
    /// </summary>
    /// <param name="customer">Id of customer</param>
    /// <param name="space">Id of space</param>
    /// <param name="asset">Id of asset</param>
    public AssetId(int customer, int space, string asset)
    {
        Customer = customer;
        Space = space;
        Asset = asset;
    }
    
    public override string ToString() => $"{Customer}/{Space}/{Asset}";

    /// <summary>
    /// Return a path for use in the private DLCS API (not the IIIF API)
    /// </summary>
    public string ToApiResourcePath() => $"/customers/{Customer}/spaces/{Space}/images/{Asset}";

    /// <summary>
    /// Create a new AssetId from string in format customer/space/image
    /// </summary>
    /// <param name="assetImageId">string representing assetImageId</param>
    /// <returns>New <see cref="AssetId"/> record</returns>
    /// <exception cref="InvalidAssetIdException">Thrown if string not in expected format</exception>
    public static AssetId FromString(string assetImageId)
    {
        var parts = assetImageId.Split("/", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            throw new InvalidAssetIdException(AssetIdError.InvalidFormat,
                $"AssetId '{assetImageId}' is invalid. Must be in format customer/space/asset");
        }

        try
        {
            return new AssetId(int.Parse(parts[0]), int.Parse(parts[1]), parts[2]);
        }
        catch (FormatException fmEx)
        {
            throw new InvalidAssetIdException(AssetIdError.InvalidFormat,
                $"AssetId '{assetImageId}' is invalid. Must be in format customer/space/asset",
                fmEx);
        }
    }

    /// <summary>
    /// Deconstruct to Tuple
    /// </summary>
    /// <remarks>This was auto-generated by Resharper when converting Record to Class</remarks>
    public void Deconstruct(out int customer, out int space, out string asset)
    {
        customer = Customer;
        space = Space;
        asset = Asset;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((AssetId)obj);
    }

    public static bool operator ==(AssetId assetId1, AssetId assetId2) 
        => assetId1.Equals(assetId2);

    public static bool operator !=(AssetId assetId1, AssetId assetId2) 
        => !(assetId1 == assetId2);

    public override int GetHashCode() => HashCode.Combine(Customer, Space, Asset);

    protected bool Equals(AssetId other) => Customer == other.Customer && Space == other.Space && Asset == other.Asset;
}

