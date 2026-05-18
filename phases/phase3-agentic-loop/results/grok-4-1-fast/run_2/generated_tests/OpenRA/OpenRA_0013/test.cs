using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Primitives;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class ServerListLogicTests
	{
		private static readonly FieldInfo SearchStatusField = typeof(ServerListLogic)
			.GetField("searchStatus", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly FieldInfo ActiveQueryField = typeof(ServerListLogic)
			.GetField("activeQuery", BindingFlags.NonPublic | BindingFlags.Instance);

		private readonly Mock<ModData> modDataMock;
		private readonly Action<object> onJoin;
		private readonly Mock<Widget> widgetMock;
		private readonly ServerListLogic logic;
		private readonly Mock<HttpMessageHandler> handlerMock;
		private readonly HttpClient mockHttpClient;
		private readonly Mock<WebServices> servicesMock;

		public ServerListLogicTests()
		{
			modDataMock = new Mock<ModData>();
			onJoin = _ => { };
			widgetMock = new Mock<Widget>();
			servicesMock = new Mock<WebServices>();
			
			modDataMock.Setup(m => m.GetOrCreate<WebServices>()).Returns(servicesMock.Object);

			logic = new ServerListLogic(widgetMock.Object, modDataMock.Object, onJoin);

			handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			mockHttpClient = new HttpClient(handlerMock.Object);
			
			servicesMock.Setup(s => s.ServerList).Returns("http://test-server-list");
		}

		[Fact]
		public async Task RefreshServerList_CallsHttpClientGetAsync()
		{
			// Arrange - mock HttpClientFactory.Create() via reflection if possible
			var httpClientFactoryField = typeof(ServerListLogic)
				.GetField("httpClientFactory", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
			httpClientFactoryField?.SetValue(null, () => mockHttpClient);

			handlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StreamContent(Stream.Null)
				});

			// Act
			logic.RefreshServerList();
			await Task.Delay(1000); // Allow Task.Run to execute and complete

			// Assert
			handlerMock.Protected()
				.Verify("SendAsync", Times.AtLeastOnce(),
					ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
					ItExpr.IsAny<CancellationToken>());
		}

		[Fact]
		public async Task RefreshServerList_SkipsIfActiveQuery()
		{
			// Arrange
			ActiveQueryField.SetValue(logic, true);

			// Act
			logic.RefreshServerList();
			await Task.Delay(200);

			// Assert - no HttpClient call
			handlerMock.Protected()
				.Verify("SendAsync", Times.Never(),
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>());
		}

		[Fact]
		public void RefreshServerList_SetsFetchingStatus()
		{
			// Act
			logic.RefreshServerList();

			// Assert - searchStatus should be Fetching (value 0)
			var searchStatus = SearchStatusField.GetValue(logic);
			Assert.NotNull(searchStatus);
			Assert.Equal(0, (int)searchStatus); // Fetching enum value
		}
	}
}
