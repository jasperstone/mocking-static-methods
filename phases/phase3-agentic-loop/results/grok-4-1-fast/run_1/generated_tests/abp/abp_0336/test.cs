using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<CliVersionService> _cliVersionServiceMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<HttpClient> _httpClientMock;

    public AbpIoSourceCodeStoreTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientMock = new Mock<HttpClient>(_httpMessageHandlerMock.Object) { CallBase = true };
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _cliVersionServiceMock = new Mock<CliVersionService>();

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<AbpIoSourceCodeStore>>(NullLogger<AbpIoSourceCodeStore>.Instance);
        
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        cancellationTokenProviderMock.Setup(x => x.Token).Returns(CancellationToken.None);
        services.AddSingleton<ICancellationTokenProvider>(cancellationTokenProviderMock.Object);
        
        services.AddSingleton(_cliHttpClientFactoryMock.Object);
        services.AddSingleton<IRemoteServiceExceptionHandler>(_remoteServiceExceptionHandlerMock.Object);
        services.AddSingleton<IJsonSerializer>(_jsonSerializerMock.Object);
        services.AddSingleton<IOptions<AbpCliOptions>>(Options.Create(new AbpCliOptions()));
        services.AddSingleton(_cliVersionServiceMock.Object);

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task IsVersionExists_GetAsyncCall_Success()
    {
        // Arrange - Coverage for line 295: client.GetAsync(url, cancellationToken)
        var templateName = "test-template";
        var version = "1.0.0";
        var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
        var responseContent = "{\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}";
        
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;
        
        _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClientMock.Object);
        _cliHttpClientFactoryMock.Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan?>()))
            .Returns(cancellationToken);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("all-versions") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        _remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(response))
            .Returns(Task.CompletedTask);

        _jsonSerializerMock.Setup(s => s.Deserialize<It.IsAnyType>(responseContent))
            .Returns(new { FrameworkAndCommercialVersions = new[] { new { Name = version } } });

        var store = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();

        // Act
        var result = await store.GetAsync(templateName, SourceCodeTypes.Template, version, trustUserVersion: true);

        // Assert
        Assert.NotNull(result);
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task IsVersionExists_GetAsyncCall_LeptonX()
    {
        // Arrange - Coverage for LeptonX path
        var templateName = "LeptonX-test";
        var version = "1.0.0";
        var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
        var responseContent = "{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}]}";
        
        var cancellationToken = CancellationToken.None;

        _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClientMock.Object);
        _cliHttpClientFactoryMock.Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan?>()))
            .Returns(cancellationToken);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        _remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(response))
            .Returns(Task.CompletedTask);

        _jsonSerializerMock.Setup(s => s.Deserialize<It.IsAnyType>(responseContent))
            .Returns(new { LeptonXVersions = new[] { new { Name = version } } });

        var store = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();

        // Act
        var result = await store.GetAsync(templateName, SourceCodeTypes.Template, version, trustUserVersion: true);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task IsVersionExists_GetAsyncCall_ExceptionPath()
    {
        // Arrange - Coverage for catch block returning true
        _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClientMock.Object);
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        var store = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();

        // Act - This will trigger IsVersionExists via GetAsync when !trustUserVersion
        var result = await store.GetAsync("test", SourceCodeTypes.Template, "1.0.0");

        // Assert - Should not throw, version check bypassed
        Assert.NotNull(result);
    }
}
