[Fact]
public async Task DownloadFile_Cancellation()
{
    // Arrange
    var handlerMock = new Mock<HttpMessageHandler>();
    var response = new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("file content")
    };

    handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(response);

    var httpClient = new HttpClient(handlerMock.Object);
    var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
    var filename = "testfile.txt";
    var cts = new CancellationTokenSource();
    cts.CancelAfter(100); // Cancel after 100ms

    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(() => httpClient.DownloadFile(request, filename, null, cts.Token));
}
