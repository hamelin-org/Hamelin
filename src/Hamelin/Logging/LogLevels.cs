namespace Hamelin.Logging;

internal static class LogMessageLevels
{
    // Some levels have a space added to maintain equal character count
    // This means logs are orderly and easier to read
    public static string Trace => "[TRACE]";
    public static string Debug => "[DEBUG]";
    public static string Information => "[INFO ]";
    public static string Warning => "[WARN ]";
    public static string Error => "[ERROR]";
    public static string Critical => "[CRIT ]";
}
