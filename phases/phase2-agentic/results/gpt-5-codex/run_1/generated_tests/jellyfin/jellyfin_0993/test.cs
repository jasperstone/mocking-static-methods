using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_LogsDebugAndUpdatesDimensions_WhenDimensionsMissing()
        {
            // Arrange
            var logger = new TestLogger<ImageProcessor>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            appPathsMock.SetupGet(x => x.ImageCachePath).Returns(Path.GetTempPath());

            var fileSystemMock = new Mock<IFileSystem>();

            var imageEncoderMock = new Mock<IImageEncoder>();
            const string imagePath = "/path/to/image.jpg";
            var expectedDimensions = new ImageDimensions(320, 240);
            imageEncoderMock.Setup(x => x.GetImageSize(imagePath)).Returns(expectedDimensions);

            var serverConfiguration = new ServerConfiguration
            {
                ParallelImageEncodingLimit = 1
            };
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.SetupGet(x => x.Configuration).Returns(serverConfiguration);

            var processor = new ImageProcessor(
                logger,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            BaseItem item = new Photo();
            var info = new ItemImageInfo
            {
                Path = imagePath,
                Width = 0,
                Height = 0
            };

            // Act
            var result = processor.GetImageDimensions(item, info);

            // Assert
            Assert.Equal(expectedDimensions.Width, result.Width);
            Assert.Equal(expectedDimensions.Height, result.Height);
            Assert.Equal(expectedDimensions.Width, info.Width);
            Assert.Equal(expectedDimensions.Height, info.Height);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Debug, entry.LogLevel);
            Assert.Equal($"Getting image size for item {item.GetType().Name} {imagePath}", entry.Message);

            var stateValues = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(entry.State);
            Assert.Contains(stateValues, kvp => kvp.Key == "ItemType" && (string)kvp.Value == item.GetType().Name);
            Assert.Contains(stateValues, kvp => kvp.Key == "Path" && (string)kvp.Value == imagePath);

            imageEncoderMock.Verify(x => x.GetImageSize(imagePath), Times.Once);
        }

        [Fact]
        public void GetImageDimensions_ReturnsExistingDimensionsWithoutLogging_WhenDimensionsAlreadyPresent()
        {
            // Arrange
            var logger = new TestLogger<ImageProcessor>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            appPathsMock.SetupGet(x => x.ImageCachePath).Returns(Path.GetTempPath());

            var fileSystemMock = new Mock<IFileSystem>();
            var imageEncoderMock = new Mock<IImageEncoder>();

            var serverConfiguration = new ServerConfiguration
            {
                ParallelImageEncodingLimit = 1
            };
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.SetupGet(x => x.Configuration).Returns(serverConfiguration);

            var processor = new ImageProcessor(
                logger,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            BaseItem item = new Photo();
            var info = new ItemImageInfo
            {
                Path = "/path/to/existing.jpg",
                Width = 200,
                Height = 100
            };

            // Act
            var result = processor.GetImageDimensions(item, info);

            // Assert
            Assert.Equal(200, result.Width);
            Assert.Equal(100, result.Height);
            Assert.Equal(200, info.Width);
            Assert.Equal(100, info.Height);

            Assert.Empty(logger.Entries);
            imageEncoderMock.Verify(x => x.GetImageSize(It.IsAny<string>()), Times.Never);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add(new LogEntry(logLevel, eventId, state!, exception, formatter(state, exception)));
            }

            public sealed record LogEntry(LogLevel LogLevel, EventId EventId, object State, Exception? Exception, string Message);

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
