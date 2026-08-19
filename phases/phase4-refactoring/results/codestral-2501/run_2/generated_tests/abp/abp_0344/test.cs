using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetPackageListAsync_ShouldReturnPackageList()
        {
            // Arrange
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

            var packageListJson = "[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]";
            jsonSerializerMock.Setup(j => j.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(new List<NpmPackageInfo>
                {
                    new NpmPackageInfo { Name = "package1" },
                    new NpmPackageInfo { Name = "package2" }
                });

            var handler = new HttpClientHandler();
            var httpClient = new HttpClient(handler);
            cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            var npmPackageInfoProvider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object);

            // Act
            var result = await npmPackageInfoProvider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("package1", result[0].Name);
            Assert.Equal("package2", result[1].Name);
        }
    }

    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }
}
