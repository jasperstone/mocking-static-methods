// In EncoderValidator.cs
public partial class EncoderValidator
{
    // Change the access modifier from private to internal
    internal IEnumerable<string> GetCodecs(Codec codec)
    {
        string codecstr = codec == Codec.Encoder ? "encoders" : "decoders";
        string output;
        try
        {
            output = GetProcessOutput(_encoderPath, "-" + codecstr, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting available {Codec}", codecstr);
            return Array.Empty<string>();
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            return Array.Empty<string>();
        }

        var required = codec == Codec.Encoder ? _requiredEncoders : _requiredDecoders;

        var found = CodecRegex()
            .Matches(output)
            .Select(x => x.Groups["codec"].Value)
            .Where(x => required.Contains(x));

        _logger.LogInformation("Available {Codec}: {Codecs}", codecstr, found);

        return found;
    }
}
