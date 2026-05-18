using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class WebServicesTests
{
    public class MockHttpQueryBuilder
    {
        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _parameters;

        public MockHttpQueryBuilder(string baseUrl)
        {
            _baseUrl = baseUrl;
            _parameters = new Dictionary<string, string>();
        }

        public MockHttpQueryBuilder Add(string key, string value)
        {
            _parameters[key] = value;
            return this;
        }

        public override string ToString()
        {
            var queryString = _parameters
                .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}")
                .ToArray();

            return $"{_baseUrl}?{string.Join("&", queryString)}";
        }
    }

    [Fact]
    public async Task CheckModVersion_CallsGetAsyncWithCorrectUrl()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("latest")
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
        var webServices = new WebServices(httpClient);

        var queryBuilder = new MockHttpQueryBuilder("https://master.openra.net/versioncheck");
        queryBuilder.Add("protocol", "1")
                    .Add("engine", "1.0.0")
                    .Add("mod", "TestMod")
                    .Add("version", "1.0.0");

        var expectedUrl = queryBuilder.ToString();

        // Act
        await webServices.CheckModVersionAsync();

        // Assert
        await handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
