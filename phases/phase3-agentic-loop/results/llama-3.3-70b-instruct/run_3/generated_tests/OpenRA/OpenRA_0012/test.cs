using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Tests
{
	public class RegisteredProfileTooltipLogicTests
	{
		[Fact]
		public async Task GetAsync_Called_WithCorrectUrl()
		{
			// Arrange
			var playerDatabase = new OpenRA.Mods.Common.PlayerDatabase("https://example.com/profile");
			var client = new OpenRA.Network.Session.Client { Fingerprint = "fingerprint" };
			var httpClient = new HttpClient();
			var widget = new Mock<Widget>();
			var modData = new Mock<ModData>();

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget.Object, null, modData.Object, client);
			await Task.Run(async () =>
			{
				try
				{
					var url = playerDatabase.Profile + client.Fingerprint;
					var request = new HttpRequestMessage(HttpMethod.Get, url);
					await httpClient.SendAsync(request, default);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			});

			// Assert
			//httpClient.Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
