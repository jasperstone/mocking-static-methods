using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class WebServicesTests
{
    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToLatest_WhenResponseIsLatest()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("latest")
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Shim the HttpClientFactory.Create method
        var originalCreate = HttpClientFactory.Create;
        HttpClientFactory.Create = () => mockHttpClient;

        var webServices = new WebServices();

        try
        {
            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the async operation to complete

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }
        finally
        {
            // Restore the original HttpClientFactory.Create method
            HttpClientFactory.Create = originalCreate;
        }
    }

    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToOutdated_WhenResponseIsOutdated()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("outdated")
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Shim the HttpClientFactory.Create method
        var originalCreate = HttpClientFactory.Create;
        HttpClientFactory.Create = () => mockHttpClient;

        var webServices = new WebServices();

        try
        {
            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the async operation to complete

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }
        finally
        {
            // Restore the original HttpClientFactory.Create method
            HttpClientFactory.Create = originalCreate;
        }
    }

    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToUnknown_WhenResponseIsUnknown()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("unknown")
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Shim the HttpClientFactory.Create method
        var originalCreate = HttpClientFactory.Create;
        HttpClientFactory.Create = () => mockHttpClient;

        var webServices = new WebServices();

        try
        {
            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the async operation to complete

            // Assert
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
        }
        finally
        {
            // Restore the original HttpClientFactory.Create method
            HttpClientFactory.Create = originalCreate;
        }
    }

    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToPlaytestAvailable_WhenResponseIsPlaytest()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("playtest")
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Shim the HttpClientFactory.Create method
        var originalCreate = HttpClientFactory.Create;
        HttpClientFactory.Create = () => mockHttpClient;

        var webServices = new WebServices();

        try
        {
            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the async operation to complete

            // Assert
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
        }
        finally
        {
            // Restore the original HttpClientFactory.Create method
            HttpClientFactory.Create = originalCreate;
        }
    }
}
