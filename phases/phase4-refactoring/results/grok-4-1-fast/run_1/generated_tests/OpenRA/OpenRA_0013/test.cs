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
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class ServerListLogicTests
	{
		private readonly Mock<Widget> mockWidget;
		private readonly Mock<ModData> mockModData;
		private readonly Action<GameServer> onJoin;

		public ServerListLogicTests()
		{
			mockWidget = new Mock<Widget>();
			mockModData = new Mock<ModData>();
			onJoin = _ => { };
		}

		[Fact]
		public async Task RefreshServerList_CallsHttpClientGetAsync_WhenNotActiveQuery()
		{
			// Arrange
			var mockHttpClient = new Mock<HttpClient>();
			mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			var logic = new TestableServerListLogic(mockWidget.Object, mockModData.Object, onJoin);
			logic.SetHttpClient(mockHttpClient.Object);

			// Act
			logic.RefreshServerList();

			// Wait for Task.Run to execute
			await Task.Delay(500);

			// Assert
			mockHttpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public void RefreshServerList_DoesNotCallHttpClient_WhenActiveQuery()
		{
			// Arrange
			var mockHttpClient = new Mock<HttpClient>();
			var logic = new TestableServerListLogic(mockWidget.Object, mockModData.Object, onJoin);
			logic.SetActiveQuery(true);
			logic.SetHttpClient(mockHttpClient.Object);

			// Act
			logic.RefreshServerList();

			// Assert
			mockHttpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
		}
	}

	public class TestableServerListLogic : ServerListLogic
	{
		private HttpClient httpClient;

		public TestableServerListLogic(Widget widget, ModData modData, Action<GameServer> onJoin)
			: base(widget, modData, onJoin)
		{
		}

		public void SetHttpClient(HttpClient client)
		{
			httpClient = client;
		}

		public void SetActiveQuery(bool active)
		{
			var field = typeof(ServerListLogic).GetField("activeQuery", 
				BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(this, active);
		}

		private HttpClient CreateHttpClient()
		{
			return httpClient ?? new HttpClient();
		}
	}
}
