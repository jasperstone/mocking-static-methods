using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task IsVersionExists_CallsHttpClientGetAsync_AndReturnsExpectedResult()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"FrameworkAndCommercialVersions\":[],\"LeptonXVersions\":[]}")
        };

        mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<TimeSpan?>()))
            .Returns(mockHttpClient.Object);

        var mockOptions = new Mock<IOptions<AbpCliOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new AbpCliOptions());

        var mockJsonSerializer = new Mock<IJsonSerializer>();
        var mockRemoteHandler = new Mock<IRemoteServiceExceptionHandler>();
        var mockTokenProvider = new Mock<ICancellationTokenProvider>();
        var mockVersionService = new Mock<CliVersionService>();

        var store = new AbpIoSourceCodeStore(
            mockOptions.Object,
            mockJsonSerializer.Object,
            mockRemoteHandler.Object,
            mockTokenProvider.Object,
            mockHttpClientFactory.Object,
            mockVersionService.Object
        );

        // Act
        var result = await store.IsVersionExists("TestTemplate", "1.0.0");

        // Assert
        Assert.True(result);
        mockHttpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
