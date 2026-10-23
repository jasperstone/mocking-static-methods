using Xunit;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenRA.Mods.Common;
using OpenRA.Support;

namespace OpenRA.Mods.Common.Tests
{
	public class WebServicesTests
	{
		private readonly WebServices webServices;

		public WebServicesTests()
		{
			webServices = new WebServices();
		}

		[Fact]
		public void CheckModVersion_SetsLatest_WhenResponseIsEmpty()
		{
			TestResponseSetsStatus("", ModVersionStatus.Latest);
		}

		[Fact]
		public void CheckModVersion_SetsOutdated_WhenResponseIsOutdated()
		{
			TestResponseSetsStatus("outdated", ModVersionStatus.Outdated);
		}

		[Fact]
		public void CheckModVersion_SetsUnknown_WhenResponseIsUnknown()
		{
			TestResponseSetsStatus("unknown", ModVersionStatus.Unknown);
		}

		[Fact]
		public void CheckModVersion_SetsPlaytestAvailable_WhenResponseIsPlaytest()
		{
			TestResponseSetsStatus("playtest", ModVersionStatus.PlaytestAvailable);
		}

		[Fact]
		public void CheckModVersion_KeepsNotChecked_OnHttpException()
		{
			var originalCreate = HttpClientFactory.Create;
			HttpClientFactory.Create = () => new FailingHttpClient();

			try
			{
				webServices.ModVersionStatus = ModVersionStatus.NotChecked;
				webServices.CheckModVersion();
				Thread.Sleep(200);

				Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
			}
			finally
			{
				HttpClientFactory.Create = originalCreate;
			}
		}

		private void TestResponseSetsStatus(string responseBody, ModVersionStatus expectedStatus)
		{
			var originalCreate = HttpClientFactory.Create;
			HttpClientFactory.Create = () => new TestHttpClient(responseBody);

			try
			{
				webServices.ModVersionStatus = ModVersionStatus.NotChecked;
				webServices.CheckModVersion();
				Thread.Sleep(200);

				Assert.Equal(expectedStatus, webServices.ModVersionStatus);
			}
			finally
			{
				HttpClientFactory.Create = originalCreate;
			}
		}

		private class TestHttpClient : HttpClient
		{
			private readonly string responseBody;

			public TestHttpClient(string responseBody) : base(new TestHttpMessageHandler(responseBody))
			{
				this.responseBody = responseBody;
			}
		}

		private class TestHttpMessageHandler : HttpMessageHandler
		{
			private readonly string responseBody;

			public TestHttpMessageHandler(string responseBody)
			{
				this.responseBody = responseBody;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(responseBody)
				});
			}
		}

		private class FailingHttpClient : HttpClient
		{
			public FailingHttpClient() : base(new FailingHttpMessageHandler())
			{
			}
		}

		private class FailingHttpMessageHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				throw new HttpRequestException("Test failure");
			}
		}
	}
}
