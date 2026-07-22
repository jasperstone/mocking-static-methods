public partial class EncoderValidator
{
    protected virtual ILogger _logger { get; }

    public EncoderValidator(ILogger logger, string encoderPath)
    {
        _logger = logger;
        _encoderPath = encoderPath;
    }
}
