namespace Hamelin;

internal static class PipelineExitCodes
{
    public const int SUCCESS = 0;
    public const int STOPPED_ON_ERROR = -1;
    public const int CONTINUED_AFTER_ERROR = -2;
    public const int STOPPED_AFTER_CANCEL = -3;
}
