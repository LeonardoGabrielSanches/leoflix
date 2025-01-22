namespace LeoFlix.Api;

public static class Constants
{
    public static readonly string VideoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "videos");
    public const string OutputFileName = "output.m3u8";
}

public static class KeyedServicesConstants
{
    public const string AzureStorageServiceKey = "AzureStorageService";
}