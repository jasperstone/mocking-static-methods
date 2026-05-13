using System;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using MediaBrowser.Controller.Extensions;

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
        public void GetFFmpegSkipValidation_CallsGetValueWithCorrectKey()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegSkipValidationKey)).Returns(true);

            var result = mockConfig.Object.GetFFmpegSkipValidation();

            Assert.True(result);
            mockConfig.Verify(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegSkipValidationKey), Times.Once);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_CallsGetValueWithCorrectKey()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(true);

            var result = mockConfig.Object.GetFFmpegImgExtractPerfTradeoff();

            Assert.True(result);
            mockConfig.Verify(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey), Times.Once);
        }

        [Fact]
        public void UseUnixSocket_CallsGetValueWithCorrectKey()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetValue<bool>(ConfigurationExtensions.BindToUnixSocketKey)).Returns(true);

            var result = mockConfig.Object.UseUnixSocket();

            Assert.True(result);
            mockConfig.Verify(c => c.GetValue<bool>(ConfigurationExtensions.BindToUnixSocketKey), Times.Once);
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
        public void GetSqliteCacheSize_CallsGetValueWithCorrectKey()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetValue<int?>(ConfigurationExtensions.SqliteCacheSizeKey)).Returns(1024);

            var result = mockConfig.Object.GetSqliteCacheSize();

            Assert.Equal(1024, result);
            mockConfig.Verify(c => c.GetValue<int?>(ConfigurationExtensions.SqliteCacheSizeKey), Times.Once);
        }
    }
}
