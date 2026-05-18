using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ICliHttpClientFactory> _mockCliHttpClientFactory;
    private readonly Mock<ICancellationTokenProvider> _mockCancellationTokenProvider;
    private readonly Mock<IRemoteServiceExceptionHandler> _mockRemoteServiceExceptionHandler;
    private readonly Mock<IJsonSerializer> _mockJsonSerializer;
    private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

    public AbpIoSourceCodeStoreTests()
    {
        _mockCliHttpClientFactory = new Mock<ICliHttpClientFactory>();
        _mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        _mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        _mockJsonSerializer = new Mock<IJsonSerializer>();

        _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
            Mock.Of<IOptions<AbpCliOptions>>(),
            _mockJsonSerializer.Object,
            _mockRemoteServiceExceptionHandler.Object,
            _mockCancellationTokenProvider.Object,
            _mockCliHttpClientFactory.Object,
            Mock.Of<CliVersionService>());
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnTrue_WhenVersionExists()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.Content.ReadAsStringAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync("{\"LeptonXVersions\": [{\"Name\": \"1.0.0\"}], \"FrameworkAndCommercialVersions\": []}");
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _mockCliHttpClientFactory.Setup(f => f.CreateClient())
            .Returns(mockHttpClient.Object);

        // Act
        var result = await _abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnFalse_WhenVersionDoesNotExist()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockResponse.Setup(r => r.Content.ReadAsStringAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync("{\"LeptonXVersions\": [], \"FrameworkAndCommercialVersions\": []}");
        mockResponse.Setup(r => r.IsSuccessStatusCode).Returns(true);

        mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _mockCliHttpClientFactory.Setup(f => f.CreateClient())
            .Returns(mockHttpClient.Object);

        // Act
        var result = await _abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

        // Assert
        Assert.False(result);
    }
}
