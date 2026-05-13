using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LogsWarning_WhenLatestVersionIsNullAndVersionNotSpecified()
        {
            // Arrange
            var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            var jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.ProjectBuilding.IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<Volo.Abp.Threading.ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();

            var store = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Setup GetLatestSourceCodeVersionAsync to return null to simulate remote service unavailable
            var getLatestVersionMethod = typeof(AbpIoSourceCodeStore).GetMethod("GetLatestSourceCodeVersionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(getLatestVersionMethod);
            var getLatestVersionTask = (Task<string>)getLatestVersionMethod.Invoke(store, new object[] { "TestName", "TestType", null, false });
            // We cannot directly mock private method, so we will create a derived class to override it

            var storeMock = new AbpIoSourceCodeStoreMock(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object,
                loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CliUsageException>(() => storeMock.GetAsync("TestName", "TestType", null));
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
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(2)); // two empty string logs

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Find the following template in your cache directory:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("\tTemplate Name\tVersion")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        private class AbpIoSourceCodeStoreMock : AbpIoSourceCodeStore
        {
            private readonly ILogger<AbpIoSourceCodeStore> _logger;

            public AbpIoSourceCodeStoreMock(
                Microsoft.Extensions.Options.IOptions<AbpCliOptions> options,
                Volo.Abp.Json.IJsonSerializer jsonSerializer,
                Volo.Abp.Cli.ProjectBuilding.IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
                Volo.Abp.Threading.ICancellationTokenProvider cancellationTokenProvider,
                Volo.Abp.Cli.Http.CliHttpClientFactory cliHttpClientFactory,
                CliVersionService cliVersionService,
                ILogger<AbpIoSourceCodeStore> logger)
                : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
            {
                _logger = logger;
                Logger = _logger;
            }

            protected override Task<string> GetLatestSourceCodeVersionAsync(string name, string type, string templateSource, bool includePreReleases)
            {
                // Return null to simulate remote service unavailable
                return Task.FromResult<string>(null);
            }

            protected override List<TemplateFile> GetLocalTemplates()
            {
                // Return a sample list to trigger the foreach logging
                return new List<TemplateFile>
                {
                    new TemplateFile(new byte[0], "Template1", "1.0.0", "1.0.0"),
                    new TemplateFile(new byte[0], "Template2", "2.0.0", "2.0.0")
                };
            }
        }
    }
}
