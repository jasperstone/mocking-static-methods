using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Volo.Abp.Cli.Http;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LogsWarning_WhenLatestVersionIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Loose, null, null);

            // Instead of mocking CliVersionService, use a minimal subclass with parameterless constructor
            var cliVersionService = new TestCliVersionService();

            var store = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionService);

            store.Logger = loggerMock.Object;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CliUsageException>(() => store.GetAsync("nonexistent-template", "type"));

            // Verify the warning logs
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
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Find the following template in your cache directory:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Template Name")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Exception message check
            Assert.Equal("Use command: abp new Acme.BookStore -v version", ex.Message);
        }

        private class TestCliVersionService : CliVersionService
        {
            public TestCliVersionService() : base(null, null, null)
            {
            }
        }
    }
}
