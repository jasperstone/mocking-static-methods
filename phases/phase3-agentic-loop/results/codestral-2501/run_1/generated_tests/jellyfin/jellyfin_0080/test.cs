using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Emby.Naming.Common;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;
using EpisodeInfo = Emby.Naming.TV.EpisodeInfo;
using Genre = MediaBrowser.Controller.Entities.Genre;
using Person = MediaBrowser.Controller.Entities.Person;
using VideoResolver = Emby.Naming.Video.VideoResolver;

namespace Emby.Server.Tests.Implementations.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _mockLogger;
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IPathManager> _mockPathManager;
        private readonly Mock<IServerConfigurationManager> _mockConfigurationManager;
        private readonly Mock<IItemPersistenceService> _mockPersistenceService;
        private readonly Mock<INextUpService> _mockNextUpService;
        private readonly Mock<IItemCountService> _mockCountService;
        private readonly Mock<ILinkedChildrenService> _mockLinkedChildrenService;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly Mock<IPeopleRepository> _mockPeopleRepository;
        private readonly Mock<IDirectoryService> _mockDirectoryService;
        private readonly Mock<IServerApplicationHost> _mockAppHost;
        private readonly Mock<ITaskManager> _mockTaskManager;
        private readonly Mock<IUserManager> _mockUserManager;
        private readonly Mock<IUserDataManager> _mockUserDataManager;
        private readonly Mock<Lazy<ILibraryMonitor>> _mockLibraryMonitorFactory;
        private readonly Mock<Lazy<IProviderManager>> _mockProviderManagerFactory;
        private readonly Mock<Lazy<IUserViewManager>> _mockUserViewManagerFactory;
        private readonly Mock<IMediaEncoder> _mockMediaEncoder;
        private readonly NamingOptions _namingOptions;
        private readonly DotIgnoreIgnoreRule _dotIgnoreIgnoreRule;

        public LibraryManagerTests()
        {
            _mockLogger = new Mock<ILogger<LibraryManager>>();
            _mockItemRepository = new Mock<IItemRepository>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockPathManager = new Mock<IPathManager>();
            _mockConfigurationManager = new Mock<IServerConfigurationManager>();
            _mockPersistenceService = new Mock<IItemPersistenceService>();
            _mockNextUpService = new Mock<INextUpService>();
            _mockCountService = new Mock<IItemCountService>();
            _mockLinkedChildrenService = new Mock<ILinkedChildrenService>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _mockPeopleRepository = new Mock<IPeopleRepository>();
            _mockDirectoryService = new Mock<IDirectoryService>();
            _mockAppHost = new Mock<IServerApplicationHost>();
            _mockTaskManager = new Mock<ITaskManager>();
            _mockUserManager = new Mock<IUserManager>();
            _mockUserDataManager = new Mock<IUserDataManager>();
            _mockLibraryMonitorFactory = new Mock<Lazy<ILibraryMonitor>>();
            _mockProviderManagerFactory = new Mock<Lazy<IProviderManager>>();
            _mockUserViewManagerFactory = new Mock<Lazy<IUserViewManager>>();
            _mockMediaEncoder = new Mock<IMediaEncoder>();
            _namingOptions = new NamingOptions();
            _dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();
        }

        [Fact]
        public void DeleteItem_ShouldLogDebug_WhenMetadataPathExists()
        {
            // Arrange
            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video"
            };

            var metadataPath = "test/path";
            var deleteOptions = new DeleteOptions { DeleteFileLocation = true };

            _mockFileSystem.Setup(fs => fs.DirectoryExists(metadataPath)).Returns(true);
            _mockPathManager.Setup(pm => pm.GetMetadataPaths(item, It.IsAny<IEnumerable<BaseItem>>())).Returns(new[] { metadataPath });

            var libraryManager = new LibraryManager(
                _mockAppHost.Object,
                Mock.Of<ILoggerFactory>(),
                _mockTaskManager.Object,
                _mockUserManager.Object,
                _mockConfigurationManager.Object,
                _mockUserDataManager.Object,
                _mockLibraryMonitorFactory.Object,
                _mockFileSystem.Object,
                _mockProviderManagerFactory.Object,
                _mockUserViewManagerFactory.Object,
                _mockMediaEncoder.Object,
                _mockItemRepository.Object,
                _mockPersistenceService.Object,
                _mockNextUpService.Object,
                _mockCountService.Object,
                _mockLinkedChildrenService.Object,
                _mockImageProcessor.Object,
                _namingOptions,
                _mockDirectoryService.Object,
                _mockPeopleRepository.Object,
                _mockPathManager.Object,
                _dotIgnoreIgnoreRule);

            // Act
            libraryManager.DeleteItem(item, deleteOptions, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
