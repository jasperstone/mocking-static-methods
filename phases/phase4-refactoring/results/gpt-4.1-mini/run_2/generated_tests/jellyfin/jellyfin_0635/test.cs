using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegProbeSize_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.FfmpegProbeSizeKey]).Returns("1234");

            var result = mockConfig.Object.GetFFmpegProbeSize();

            Assert.Equal("1234", result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.FfmpegAnalyzeDurationKey]).Returns("5678");

            var result = mockConfig.Object.GetFFmpegAnalyzeDuration();

            Assert.Equal("5678", result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.FfmpegSkipValidationKey]).Returns("true");

            bool result = bool.Parse(mockConfig.Object[ConfigurationExtensions.FfmpegSkipValidationKey]);

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey]).Returns("true");

            bool result = bool.Parse(mockConfig.Object[ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey]);

            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.BindToUnixSocketKey]).Returns("true");

            bool result = bool.Parse(mockConfig.Object[ConfigurationExtensions.BindToUnixSocketKey]);

            Assert.True(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.UnixSocketPathKey]).Returns("/tmp/socket");

            var result = mockConfig.Object.GetUnixSocketPath();

            Assert.Equal("/tmp/socket", result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.UnixSocketPermissionsKey]).Returns("777");

            var result = mockConfig.Object.GetUnixSocketPermissions();

            Assert.Equal("777", result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsValueFromIndexer()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c[ConfigurationExtensions.SqliteCacheSizeKey]).Returns("1024");

            int result = int.Parse(mockConfig.Object[ConfigurationExtensions.SqliteCacheSizeKey], CultureInfo.InvariantCulture);

            Assert.Equal(1024, result);
        }
    }
}
