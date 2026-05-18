using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Server;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Support;
using Xunit;
using S = OpenRA.Server.Server;

namespace OpenRA.Mods.Common.Tests.ServerTraits
{
	public class MasterServerPingerTests
	{
		private readonly Mock<ModData> modDataMock;
		private readonly Mock<WebServices> webServicesMock;
		private readonly S server;

		public MasterServerPingerTests()
		{
			modDataMock = new Mock<ModData>();
			webServicesMock = new Mock<WebServices>();
			webServicesMock.Setup(ws => ws.ServerAdvertise).Returns("https://example.com/api");
			modDataMock.Setup(md => md.GetOrCreate<WebServices>()).Returns(webServicesMock.Object);

			// Create real Server instance with mocked ModData
			var serverSettings = new ServerSettings(); // Assuming default constructor exists
			server = new S(serverSettings, modDataMock.Object, null, null); // Simplified constructor args
		}

		[Fact]
		public async Task UpdateMasterServer_CallsHttpClientPostAsync()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("[0]OK")
				});

			var httpClient = new HttpClient(handlerMock.Object);

			// Replace HttpClientFactory static field with our client
			var factoryField = typeof(HttpClientFactory).GetField("client", BindingFlags.Static | BindingFlags.NonPublic)!;
			factoryField.SetValue(null, httpClient);

			var pinger = new MasterServerPinger();
			SetPrivateField(pinger, "isInitialPing", true);

			// Act
			await InvokeUpdateMasterServer(pinger, server, "test-post-data");

			// Assert
			handlerMock.Protected().Verify("SendAsync", Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString() == "https://example.com/api"),
				ItExpr.IsAny<CancellationToken>());
		}

		[Fact]
		public async Task UpdateMasterServer_HandlesHttpClientException()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ThrowsAsync(new HttpRequestException("Test exception"));

			var httpClient = new HttpClient(handlerMock.Object);

			var factoryField = typeof(HttpClientFactory).GetField("client", BindingFlags.Static | BindingFlags.NonPublic)!;
			factoryField.SetValue(null, httpClient);

			var pinger = new MasterServerPinger();

			// Act
			await InvokeUpdateMasterServer(pinger, server, "test-post-data");

			// Assert - test completes without crashing
			Assert.True(true);
		}

		[Fact]
		public async Task UpdateMasterServer_ProcessesInitialPingResponse()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("[0]Success")
				});

			var httpClient = new HttpClient(handlerMock.Object);

			var factoryField = typeof(HttpClientFactory).GetField("client", BindingFlags.Static | BindingFlags.NonPublic)!;
			factoryField.SetValue(null, httpClient);

			var pinger = new MasterServerPinger();
			SetPrivateField(pinger, "isInitialPing", true);

			// Act
			await InvokeUpdateMasterServer(pinger, server, "test-post-data");

			// Assert
			Assert.False((bool)GetPrivateField(pinger, "isInitialPing"));
		}

		private static async Task InvokeUpdateMasterServer(MasterServerPinger pinger, S server, string postData)
		{
			var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance)!;
			var task = (Task)method.Invoke(pinger, new object[] { server, postData })!;
			await task;
		}

		private static void SetPrivateField(object obj, string fieldName, object value)
		{
			var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!;
			field.SetValue(obj, value);
		}

		private static object GetPrivateField(object obj, string fieldName)
		{
			var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!;
			return field.GetValue(obj)!;
		}
	}
}
