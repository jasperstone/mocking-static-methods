using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using NuGet.Versioning;
using Volo.Abp.Cli.GitHub;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Console;
using Volo.Abp.Cli.ProjectBuilding.Templates.Maui;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.ProjectBuilding.Templates.Wpf;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_ThrowsException_WhenRemoteServiceIsUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<Volo.Abp.Threading.ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<Volo.Abp.Cli.Version.CliVersionService>();
            var optionsMock = new Mock<IOptions<Volo.Abp.Cli.AbpCliOptions>>();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => abpIoSourceCodeStore.GetAsync("name", "type"));
        }

        [Fact]
        public async Task GetAsync_LogsWarning_WhenRemoteServiceIsUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<Volo.Abp.Threading.ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<Volo.Abp.Cli.Version.CliVersionService>();
            var optionsMock = new Mock<IOptions<Volo.Abp.Cli.AbpCliOptions>>();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            // Act
            try
            {
                await abpIoSourceCodeStore.GetAsync("name", "type");
            }
            catch (Exception)
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }
    }
}
