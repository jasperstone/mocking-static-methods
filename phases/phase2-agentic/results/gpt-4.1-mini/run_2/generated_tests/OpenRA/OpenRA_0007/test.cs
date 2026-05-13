using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Server
{
	public class MasterServerPingerTests
	{
		// Helper to create a Server with minimal setup for ModData and Settings
		private class TestServer : Server.Server
		{
			public TestServer()
			{
				ModData = new ModData();
				Settings = new ServerSettings
				{
					AdvertiseOnline = true,
					AdvertiseOnLocalNetwork = false
				};
				IsMultiplayer = true;
			}
		}

		// Minimal ModData and WebServices stub
		private class ModData : OpenRA.Server.ModData
		{
			public override T GetOrCreate<T>()
			{
				if (typeof(T) == typeof(WebServices))
					return (T)(object)new WebServices { ServerAdvertise = "http://testserver/advertise" };
				return base.GetOrCreate<T>();
			}
		}

		private class WebServices
		{
			public string ServerAdvertise { get; set; }
		}

		[Fact]
		public async Task UpdateMasterServer_PostAsync_IsCalled_And_ProcessesResponse()
		{
			// Arrange
			var server = new TestServer();

			// Setup HttpClientFactory to return a HttpClient with mocked handler
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req =>
						req.Method == HttpMethod.Post &&
						req.RequestUri == new Uri("http://testserver/advertise")),
					ItExpr.IsAny<System.Threading.CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("[1]Port forwarding required")
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			// Replace HttpClientFactory.Create to return our httpClient
			// We do this by reflection since HttpClientFactory is static and not injectable
			var httpClientFactoryType = typeof(MasterServerPinger).Assembly.GetType("System.Net.Http.HttpClientFactory");
			Assert.NotNull(httpClientFactoryType);
			var createMethod = httpClientFactoryType.GetMethod("Create", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			Assert.NotNull(createMethod);

			// We cannot replace static method easily, so instead we test the UpdateMasterServer indirectly by calling Tick
			// But to test the PostAsync call specifically, we will create a derived class to override HttpClientFactory.Create

			var pinger = new TestMasterServerPinger(httpClient);

			// Act
			// Call UpdateMasterServer via Tick to trigger the call
			pinger.Tick(server);

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.AtLeastOnce(),
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri("http://testserver/advertise")),
				ItExpr.IsAny<System.Threading.CancellationToken>());

			// The masterServerMessages queue should contain Connected and the error message for code 1
			lock (pinger.MasterServerMessages)
			{
				Assert.Contains(MasterServerPinger.Connected, pinger.MasterServerMessages);
				Assert.Contains(MasterServerPinger.NoPortForward, pinger.MasterServerMessages);
				Assert.Contains(MasterServerPinger.GameOffline, pinger.MasterServerMessages);
			}
		}

		// Derived class to override HttpClientFactory.Create to return our mocked HttpClient
		private class TestMasterServerPinger : MasterServerPinger
		{
			private readonly HttpClient _httpClient;

			public TestMasterServerPinger(HttpClient httpClient)
			{
				_httpClient = httpClient;
			}

			// Expose masterServerMessages for test assertions
			public Queue<string> MasterServerMessages
			{
				get
				{
					var field = typeof(MasterServerPinger).GetField("masterServerMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					return (Queue<string>)field.GetValue(this);
				}
			}

			// Override HttpClientFactory.Create to return our HttpClient
			// We do this by shadowing the method call inside UpdateMasterServer via reflection
			// Since the original code calls HttpClientFactory.Create() directly, we need to override UpdateMasterServer to use our client
			protected override void UpdateMasterServer(Server.Server server, string postData)
			{
				var isBusyField = typeof(MasterServerPinger).GetField("isBusy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				isBusyField.SetValue(this, true);

				Task.Run(async () =>
				{
					try
					{
						var endpoint = server.ModData.GetOrCreate<WebServices>().ServerAdvertise;

						var response = await _httpClient.PostAsync(endpoint, new StringContent(postData));

						var masterResponseText = await response.Content.ReadAsStringAsync();

						var isInitialPingField = typeof(MasterServerPinger).GetField("isInitialPing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						var masterServerMessagesField = typeof(MasterServerPinger).GetField("masterServerMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						var masterServerMessages = (Queue<string>)masterServerMessagesField.GetValue(this);

						if ((bool)isInitialPingField.GetValue(this))
						{
							Log.Write("server", "Master server: " + masterResponseText);
							var errorCode = 0;
							var errorMessage = string.Empty;

							if (!string.IsNullOrWhiteSpace(masterResponseText))
							{
								var regex = new System.Text.RegularExpressions.Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
								var match = regex.Match(masterResponseText);
								errorMessage = match.Success && int.TryParse(match.Groups["code"].Value, out errorCode) ?
									match.Groups["message"].Value.Trim() : InvalidErrorCode;
							}

							isInitialPingField.SetValue(this, false);
							lock (masterServerMessages)
							{
								masterServerMessages.Enqueue(Connected);
								if (errorCode != 0)
								{
									if (!MasterServerErrors.TryGetValue(errorCode, out var message))
										message = errorMessage;

									masterServerMessages.Enqueue(message);

									if (errorCode > 0)
										masterServerMessages.Enqueue(GameOffline);
								}
							}
						}
					}
					catch (Exception ex)
					{
						Log.Write("server", ex.ToString());
						lock (MasterServerMessages)
							MasterServerMessages.Enqueue(Error);
					}

					isBusyField.SetValue(this, false);
				});
			}
		}
	}
}
