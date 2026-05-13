using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class ItchIntegrationTests
{
    [Fact]
    public async Task GetPlayerName_SuccessfulApiResponse_InvokesCallbackWithDisplayName()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"user\":{\"display_name\":\"TestUser\"}}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var itchIntegration = new ItchIntegration();

        var callbackInvoked = false;
        string callbackName = null;
        Action<string> callback = name =>
        {
            callbackInvoked = true;
            callbackName = name;
        };

        // Act
        itchIntegration.GetPlayerName(callback);
        await Task.Delay(1000); // Wait for the task to complete

        // Assert
        Assert.True(callbackInvoked);
        Assert.Equal("TestUser", callbackName);
    }

    [Fact]
    public async Task GetPlayerName_SuccessfulApiResponse_InvokesCallbackWithUsername()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"user\":{\"username\":\"TestUser\"}}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var itchIntegration = new ItchIntegration();

        var callbackInvoked = false;
        string callbackName = null;
        Action<string> callback = name =>
        {
            callbackInvoked = true;
            callbackName = name;
        };

        // Act
        itchIntegration.GetPlayerName(callback);
        await Task.Delay(1000); // Wait for the task to complete

        // Assert
        Assert.True(callbackInvoked);
        Assert.Equal("TestUser", callbackName);
    }

    [Fact]
    public async Task GetPlayerName_FailedApiResponse_DoesNotInvokeCallback()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var itchIntegration = new ItchIntegration();

        var callbackInvoked = false;
        Action<string> callback = name => callbackInvoked = true;

        // Act
        itchIntegration.GetPlayerName(callback);
        await Task.Delay(1000); // Wait for the task to complete

        // Assert
        Assert.False(callbackInvoked);
    }
}
