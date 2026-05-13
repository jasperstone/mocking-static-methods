using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using MediaBrowser.Controller.Extensions;

namespace MediaBrowser.Controller.Tests.Extensions
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegProbeSize_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c[ConfigurationExtensions.FfmpegProbeSizeKey]).Returns("12345");

            // Act
            var result = ConfigurationExtensions.GetFFmpegProbeSize(mockConfiguration.Object);

            // Assert
            Assert.Equal("12345", result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c[ConfigurationExtensions.FfmpegAnalyzeDurationKey]).Returns("67890");

            // Act
            var result = ConfigurationExtensions.GetFFmpegAnalyzeDuration(mockConfiguration.Object);

            // Assert
            Assert.Equal("67890", result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegSkipValidationKey)).Returns(true);

            // Act
            var result = ConfigurationExtensions.GetFFmpegSkipValidation(mockConfiguration.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(true);

            // Act
            var result = ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(mockConfiguration.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.BindToUnixSocketKey)).Returns(true);

            // Act
            var result = ConfigurationExtensions.UseUnixSocket(mockConfiguration.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetUnixSocketPath_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c[ConfigurationExtensions.UnixSocketPathKey]).Returns("/tmp/socket");

            // Act
            var result = ConfigurationExtensions.GetUnixSocketPath(mockConfiguration.Object);

            // Assert
            Assert.Equal("/tmp/socket", result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c[ConfigurationExtensions.UnixSocketPermissionsKey]).Returns("0777");

            // Act
            var result = ConfigurationExtensions.GetUnixSocketPermissions(mockConfiguration.Object);

            // Assert
            Assert.Equal("0777", result);
        }

        [Fact]
        public void GetSqliteCacheSize_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetValue<int?>(ConfigurationExtensions.SqliteCacheSizeKey)).Returns(1000);

            // Act
            var result = ConfigurationExtensions.GetSqliteCacheSize(mockConfiguration.Object);

            // Assert
            Assert.Equal(1000, result);
        }
    }
}
