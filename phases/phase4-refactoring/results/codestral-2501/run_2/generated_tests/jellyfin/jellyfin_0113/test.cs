using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Library;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Emby.Server.Implementations.Library.Resolvers;
using Emby.Naming.Common;
using Emby.Naming.TV;
using Emby.Server.Implementations.Library.Validators;
using Emby.Server.Implementations.Playlists;
using Emby.Server.Implementations.ScheduledTasks.Tasks;
using Emby.Server.Implementations.Sorting;
using Jellyfin.Data;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;
using EpisodeInfo = Emby.Naming.TV.EpisodeInfo;
using Genre = MediaBrowser.Controller.Entities.Genre;
using Person = MediaBrowser.Controller.Entities.Person;
using VideoResolver = Emby.Naming.Video.VideoResolver;

namespace Emby.Server.Tests.Implementations.Library
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task LogWarning_WhenImageNotFound_ShouldLogWarning()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var nextUpServiceMock = new Mock<INextUpService>();
            var countServiceMock = new Mock<IItemCountService>();
            var linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            var pathManagerMock = new Mock<IPathManager>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactoryMock = new Mock<Lazy<ILibraryMonitor>>();
            var providerManagerFactoryMock = new Mock<Lazy<IProviderManager>>();
            var userViewManagerFactoryMock = new Mock<Lazy<IUserViewManager>>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var peopleRepositoryMock = new Mock<IPeopleRepository>();
            var namingOptionsMock = new NamingOptions();

            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var libraryManager = new LibraryManager(
                appHostMock.Object,
                loggerFactoryMock.Object,
                taskManagerMock.Object,
                userManagerMock.Object,
                configurationManagerMock.Object,
                userDataManagerMock.Object,
                libraryMonitorFactoryMock.Object,
                fileSystemMock.Object,
                providerManagerFactoryMock.Object,
                userViewManagerFactoryMock.Object,
                mediaEncoderMock.Object,
                itemRepositoryMock.Object,
                persistenceServiceMock.Object,
                nextUpServiceMock.Object,
                countServiceMock.Object,
                linkedChildrenServiceMock.Object,
                imageProcessorMock.Object,
                namingOptionsMock,
                new Mock<IDirectoryService>().Object,
                peopleRepositoryMock.Object,
                pathManagerMock.Object,
                dotIgnoreIgnoreRuleMock.Object);

            var item = new Movie(); // Using a concrete subclass
            var image = new ItemImageInfo { Path = "non_existent_path" };
            var outdated = new List<ItemImageInfo> { image };

            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            await libraryManager.UpdateImagesAsync(item, outdated);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at {ImagePath}")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}
