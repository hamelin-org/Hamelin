namespace Hamelin;

internal static class PipelineExitCodes
{
    public const int Success = 0;
    public const int StoppedOnError = -1;
    public const int ContinuedAfterError = -2;
    public const int StoppedAfterCancel = -3;
}
