using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Tasks;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ThrowsException_LogsErrorWithException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var mockPluginManager = new Mock<IPluginManager>();
            var applicationHost = new ApplicationHostMock(loggerMock.Object, mockPluginManager.Object);

            // Act
            applicationHost.CreateInstanceSafeThrowing(typeof(TestTypeWithNoDefaultCtor));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating TestTypeWithNoDefaultCtor")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_DetectsCircularDependency_LogsErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var mockPluginManager = new Mock<IPluginManager>();
            var applicationHost = new ApplicationHostMock(loggerMock.Object, mockPluginManager.Object);

            // Act
            var exception = Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(typeof(CircularDependencyType)));
            Assert.Equal("DI Loop detected", exception.Message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("DI Loop detected in the attempted creation of CircularDependencyType")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Called from: CircularDependencyType")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Test classes to trigger the specific code paths
    public class TestTypeWithNoDefaultCtor
    {
        public TestTypeWithNoDefaultCtor(int unusedParam) { }
    }

    public class CircularDependencyType { }

    // Complete mock implementation for IServerApplicationPaths
    public class MockServerApplicationPaths : IServerApplicationPaths
    {
        public string[] All() => Array.Empty<string>();
        public string InternalMetadataPath => "";
        public string MetadataPath => "";
        public string CachePath => "";
        public string ConfigPath => "";
        public string LogPath => "";
        public string TempPath => "";
        public string TranscodingTempPath => "";
        public string HttpProxyPath => "";
        public string PluginsPath => "";
        public string ProgramDataPath => "";
        public string SystemPath => "";
        public string InternalPluginsPath => "";
        public string GeneralPluginsPath => "";
        public string ProgramSystemPath => "";
        public string ItemsPath => "";
        public string ImagesPath => "";
        public string ArtImagePath => "";
        public string PosterPath => "";
        public string BackdropPath => "";
        public string BannerPath => "";
        public string ThumbPath => "";
        public string LogoPath => "";
        public string DiscPath => "";
        public string ChapterImagePath => "";
        public string FanartPath => "";
        public string SubtitleImagePath => "";
        public string RootFolderImagePath => "";
        public string SubtitlePath => "";
        public string LyricsPath => "";
        public string CustomCssPath => "";
        public string ThemesPath => "";
        public string ThemeInfoPath => "";
        public string RootFolderPath => "";
        public string DefaultUserViewsPath => "";
        public string PeoplePath => "";
        public string GenrePath => "";
        public string MusicGenrePath => "";
        public string StudioPath => "";
        public string[] ValidImageExtensions => Array.Empty<string>();
        public string[] ValidSubExtensions => Array.Empty<string>();
    }

    public class MockStartupOptions : IStartupOptions
    {
        public string FFmpegPath => "";
        public bool IsService => false;
        public string PackageName => "";
        public string? PublishedServerUrl => null;
        public string? RestartPath => null;
    }

    public interface IPluginManager
    {
        void FailPlugin(Assembly assembly);
    }

    // Concrete implementation that overrides abstract methods and exposes protected method
    public class ApplicationHostMock : ApplicationHost
    {
        private readonly ILogger<ApplicationHost> _logger;
        private readonly IPluginManager _pluginManager;
        private List<Type> _creatingInstances = new();

        public ApplicationHostMock(ILogger<ApplicationHost> logger, IPluginManager pluginManager) : base(
            new MockServerApplicationPaths(),
            Mock.Of<ILoggerFactory>(),
            new MockStartupOptions(),
            Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>())
        {
            _logger = logger;
            _pluginManager = pluginManager;
            Logger = _logger;
        }

        public object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);

        public object CreateInstanceSafeThrowing(Type type)
        {
            try
            {
                return CreateInstanceSafe(type);
            }
            catch
            {
                return null;
            }
        }

        protected override List<Type> _creatingInstances => _creatingInstances;
        protected new IPluginManager _pluginManager => this._pluginManager;
        protected new ILogger<ApplicationHost> Logger => _logger;

        public override IEnumerable<Assembly> GetAssembliesWithPartsInternal() => Enumerable.Empty<Assembly>();
        public override void Init(IProgress<double> progress) { }
        public override Task InitAsync(IProgress<double> progress) => Task.CompletedTask;
    }
}
