using System.Collections.Generic;
using MediaBrowser.Controller.Extensions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MediaBrowser.Controller.Tests.Extensions
{
    public class ConfigurationExtensionsTests
    {
        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData(null, false)]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsExpectedValue(string? configuredValue, bool expected)
        {
            var builder = new ConfigurationBuilder();

            if (configuredValue is not null)
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey] = configuredValue
                });
            }

            var configuration = builder.Build();

            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            Assert.Equal(expected, result);
        }
    }
}
