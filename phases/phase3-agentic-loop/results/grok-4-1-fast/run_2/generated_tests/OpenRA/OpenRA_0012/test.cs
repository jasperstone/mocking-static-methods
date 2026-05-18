using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using OpenRA;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Widgets;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.FileFormats;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task HttpClient_GetAsync_IsCalled_WithCorrectUrl()
        {
            // Arrange
            var widget = CreateMinimalTestWidget();
            var worldRendererMock = new Mock<WorldRenderer>();
            var modDataMock = new Mock<ModData>();
            var playerDatabaseMock = new Mock<PlayerDatabase>();
            playerDatabaseMock.Setup(p => p.Profile).Returns("https://example.com/api/");
            modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);

            var client = new Session.Client { Fingerprint = "test-fingerprint-123" };

            // Mock all the dependencies that get accessed
            worldRendererMock.Setup(wr => wr.Fonts[It.IsAny<string>()]).Returns(new Mock<Font>().Object);
            Game.Renderer = worldRendererMock.Object;

            var handlerMock = new Mock<HttpMessageHandler>();
            var expectedUrl = "https://example.com/api/test-fingerprint-123";
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == expectedUrl && r.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Array.Empty<byte>())
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Replace HttpClientFactory instance via reflection
            var httpClientFactoryType = Type.GetType("OpenRA.Support.HttpClientFactory, OpenRA");
            var instanceField = httpClientFactoryType?.GetField("instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var originalInstance = instanceField?.GetValue(null);

            try
            {
                instanceField?.SetValue(null, new TestHttpClientFactory(() => httpClient));

                // Act - constructor starts Task.Run immediately
                var logic = new RegisteredProfileTooltipLogic(widget, worldRendererMock.Object, modDataMock.Object, client);

                // Wait for the background task to complete
                await Task.Delay(500);
            }
            finally
            {
                // Restore original
                instanceField?.SetValue(null, originalInstance);
            }

            // Assert - verify GetAsync was called via the handler
            handlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>());
        }

        private Widget CreateMinimalTestWidget()
        {
            var widget = new TestWidget("ROOT");
            var header = new TestWidget("HEADER");
            var profileHeader = new TestWidget("PROFILE_HEADER");
            var messageHeader = new TestWidget("MESSAGE_HEADER");
            var message = new TestLabelWidget("MESSAGE") { Font = "GameFont" };
            var badgeContainer = new TestWidget("BADGES_CONTAINER");

            messageHeader.AddChild(message);
            header.AddChild(profileHeader);
            header.AddChild(messageHeader);
            widget.AddChild(header);
            widget.AddChild(badgeContainer);

            return widget;
        }

        private class TestWidget : Widget
        {
            public TestWidget(string id)
            {
                Id = id;
                Bounds = new int2(0, 0);
            }

            public override Widget Get(string key) => this;
            public override Widget GetOrNull(string key) => this;
        }

        private class TestLabelWidget : Widget
        {
            public TestLabelWidget(string id) : base()
            {
                Id = id;
                Bounds = new int2(0, 0);
            }
        }

        private class TestHttpClientFactory
        {
            private readonly Func<HttpClient> factory;
            public TestHttpClientFactory(Func<HttpClient> factory) => this.factory = factory;
            public HttpClient Create() => factory();
        }
    }
}
