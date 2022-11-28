﻿namespace DLCS.Model.Assets;

public static class AssetDeliveryChannels
{
    public const string Image = "iiif-img";
    public const string Timebased = "iiif-av";
    public const string File = "file";

    /// <summary>
    /// All possible delivery channels
    /// </summary>
    public static string[] All { get; } = { File, Timebased, Image };

    /// <summary>
    /// All possible delivery channels as a comma-delimited string
    /// </summary>
    public static string AllString = string.Join(',', All);
}