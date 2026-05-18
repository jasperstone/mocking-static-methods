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
        public void GetFFmpegSkipValidation_ReturnsValueFromGetSectionAndGet()
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("true");
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.FfmpegSkipValidationKey)).Returns(mockSection.Object);

            var result = mockConfig.Object.GetFFmpegSkipValidation();

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsValueFromGetSectionAndGet()
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("true");
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(mockSection.Object);

            var result = mockConfig.Object.GetFFmpegImgExtractPerfTradeoff();

            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsValueFromGetSectionAndGet()
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("true");
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.BindToUnixSocketKey)).Returns(mockSection.Object);

            var result = mockConfig.Object.UseUnixSocket();

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
        public void GetSqliteCacheSize_ReturnsValueFromGetSectionAndGet()
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("1024");
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.SqliteCacheSizeKey)).Returns(mockSection.Object);

            var result = mockConfig.Object.GetSqliteCacheSize();

            Assert.Equal(1024, result);
        }
    }
}
