using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
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
        public void GetFFmpegSkipValidation_ReturnsValueFromGetValue()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.FfmpegSkipValidationKey))
                .Returns(Mock.Of<IConfigurationSection>(s => s.Value == "true"));

            var result = ConfigurationExtensions.GetFFmpegSkipValidation(mockConfig.Object);

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsValueFromGetValue()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey))
                .Returns(Mock.Of<IConfigurationSection>(s => s.Value == "false"));

            var result = ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(mockConfig.Object);

            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsValueFromGetValue()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.BindToUnixSocketKey))
                .Returns(Mock.Of<IConfigurationSection>(s => s.Value == "true"));

            var result = ConfigurationExtensions.UseUnixSocket(mockConfig.Object);

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
            mockConfig.Setup(c => c[ConfigurationExtensions.UnixSocketPermissionsKey]).Returns("755");

            var result = mockConfig.Object.GetUnixSocketPermissions();

            Assert.Equal("755", result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsValueFromGetValue()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetSection(ConfigurationExtensions.SqliteCacheSizeKey))
                .Returns(Mock.Of<IConfigurationSection>(s => s.Value == "1024"));

            var result = ConfigurationExtensions.GetSqliteCacheSize(mockConfig.Object);

            Assert.Equal(1024, result);
        }
    }
}
