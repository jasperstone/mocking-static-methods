using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LogsWarning_WhenLatestVersionIsNullAndVersionNotSpecified()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            // CliHttpClientFactory has no parameterless constructor, so we mock as interface or use null if not used in this test
            CliHttpClientFactory cliHttpClientFactory = null;

            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();

            var testStore = new TestAbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactory,
                cliVersionServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            testStore.SetLatestVersion(null);
            testStore.SetLocalTemplates(new List<TemplateFile>
            {
                new TemplateFile(new byte[0], "Template1", "1.0.0", "1.0.0"),
                new TemplateFile(new byte[0], "Template2", "2.0.0", "2.0.0")
            });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CliUsageException>(() => testStore.GetAsync("name", "type"));

            Assert.Equal("Use command: abp new Acme.BookStore -v version", ex.Message);

            // Verify that the expected warnings were logged
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "The remote service is currently unavailable, please specify the version."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == string.Empty),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(2)); // two empty lines logged

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Find the following template in your cache directory: "),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Template1") && v.ToString().Contains("1.0.0")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Template2") && v.ToString().Contains("2.0.0")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        private class TestAbpIoSourceCodeStore : AbpIoSourceCodeStore
        {
            private string _latestVersion;
            private List<TemplateFile> _localTemplates = new List<TemplateFile>();

            public TestAbpIoSourceCodeStore(
                IOptions<AbpCliOptions> options,
                IJsonSerializer jsonSerializer,
                IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
                ICancellationTokenProvider cancellationTokenProvider,
                CliHttpClientFactory cliHttpClientFactory,
                CliVersionService cliVersionService)
                : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
            {
            }

            public void SetLatestVersion(string version)
            {
                _latestVersion = version;
            }

            public void SetLocalTemplates(List<TemplateFile> templates)
            {
                _localTemplates = templates;
            }

            // We simulate the private methods by new methods with same signature but new keyword
            protected new Task<string> GetLatestSourceCodeVersionAsync(string name, string type, string templateSource, bool includePreReleases)
            {
                return Task.FromResult(_latestVersion);
            }

            protected new List<TemplateFile> GetLocalTemplates()
            {
                return _localTemplates;
            }
        }
    }
}
