using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public void Constructor_CanBeCreated()
        {
            // Arrange
            var logger = Mock.Of<ILogger<TrickplayManager>>();
            var mediaEncoder = Mock.Of<IMediaEncoder>();
            var fileSystem = Mock.Of<IFileSystem>();
            var encodingHelper = new EncodingHelper(Mock.Of<ILogger<EncodingHelper>>(), mediaEncoder);
            var config = Mock.Of<IServerConfigurationManager>();
            var imageEncoder = Mock.Of<IImageEncoder>();
            var dbProvider = Mock.Of<IDbContextFactory<JellyfinDbContext>>();
            var appPaths = Mock.Of<IApplicationPaths>();
            var pathManager = Mock.Of<IPathManager>();

            // Act
            var sut = new TrickplayManager(logger, mediaEncoder, fileSystem, encodingHelper, config, imageEncoder, dbProvider, appPaths, pathManager);

            // Assert
            Assert.NotNull(sut);
        }

        [Fact]
        public async Task GenerateTrickplayDataAsync_WhenSuccessful_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var video = new Video { Id = "test-id", Path = "/path/to/media.mp4" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var cancellationToken = new CancellationToken();

            var mocks = new Dictionary<Type, object>
            {
                [typeof(ILogger<TrickplayManager>)] = loggerMock.Object,
                [typeof(IMediaEncoder)] = Mock.Of<IMediaEncoder>(),
                [typeof(IFileSystem)] = Mock.Of<IFileSystem>(),
                [typeof(IServerConfigurationManager)] = Mock.Of<IServerConfigurationManager>(),
                [typeof(IImageEncoder)] = Mock.Of<IImageEncoder>(),
                [typeof(IDbContextFactory<JellyfinDbContext>)] = Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                [typeof(IApplicationPaths)] = Mock.Of<IApplicationPaths>(),
                [typeof(IPathManager)] = Mock.Of<IPathManager>()
            };

            var encodingHelper = new EncodingHelper(Mock.Of<ILogger<EncodingHelper>>(), Mock.Of<IMediaEncoder>());
            var sut = new TrickplayManager(
                loggerMock.Object,
                mocks[typeof(IMediaEncoder)] as IMediaEncoder,
                mocks[typeof(IFileSystem)] as IFileSystem,
                encodingHelper,
                mocks[typeof(IServerConfigurationManager)] as IServerConfigurationManager,
                mocks[typeof(IImageEncoder)] as IImageEncoder,
                mocks[typeof(IDbContextFactory<JellyfinDbContext>)] as IDbContextFactory<JellyfinDbContext>,
                mocks[typeof(IApplicationPaths)] as IApplicationPaths,
                mocks[typeof(IPathManager)] as IPathManager);

            // Setup logger verification for the specific LogInformation call (line 361)
            loggerMock.Setup(x => x.LogInformation(
                It.Is<string>(msg => msg == "Finished creation of trickplay files for {0}"),
                It.IsAny<object[]>()))
                .Verifiable();

            // Act
            await sut.GenerateTrickplayDataAsync(video, libraryOptions, cancellationToken);

            // Assert
            loggerMock.Verify();
        }
    }
}
