using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
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
using Emby.Naming.TV;
using Emby.Server.Implementations.Library.Resolvers;
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
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.MediaEncoding;
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
using Episode = MediaBrowser.Controller.Entities.TV.Episode;
using EpisodeInfo = Emby.Naming.TV.EpisodeInfo;
using Genre = MediaBrowser.Controller.Entities.Genre;
using Person = MediaBrowser.Controller.Entities.Person;
using VideoResolver = Emby.Naming.Video.VideoResolver;

public class LibraryManagerTests
{
    [Fact]
    public async Task ConvertImageToLocal_ShouldLogDebugOnHttpRequestException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LibraryManager>>();
        var providerManagerMock = new Mock<IProviderManager>();
        var item = new Mock<BaseItem>().Object;
        var image = new ItemImageInfo { Path = "http://example.com/image.jpg", Type = ImageType.Primary };
        var imageIndex = 0;
        var removeOnFailure = false;

        providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.NotFound));

        var libraryManager = new LibraryManager(
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<ILoggerFactory>(),
            Mock.Of<ITaskManager>(),
            Mock.Of<IUserManager>(),
            Mock.Of<IServerConfigurationManager>(),
            Mock.Of<IUserDataManager>(),
            Mock.Of<Lazy<ILibraryMonitor>>(),
            Mock.Of<IFileSystem>(),
            Mock.Of<Lazy<IProviderManager>>(),
            Mock.Of<Lazy<IUserViewManager>>(),
            Mock.Of<IMediaEncoder>(),
            Mock.Of<IItemRepository>(),
            Mock.Of<IItemPersistenceService>(),
            Mock.Of<INextUpService>(),
            Mock.Of<IItemCountService>(),
            Mock.Of<ILinkedChildrenService>(),
            Mock.Of<IImageProcessor>(),
            Mock.Of<NamingOptions>(),
            Mock.Of<IDirectoryService>(),
            Mock.Of<IPeopleRepository>(),
            Mock.Of<IPathManager>(),
            Mock.Of<DotIgnoreIgnoreRule>(),
            loggerMock.Object,
            providerManagerMock.Object
        );

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
