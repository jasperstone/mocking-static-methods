using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common;
using System.Net;
using System.Threading;

namespace OpenRA.Mods.Common.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_ShouldSetModVersionStatus()
        {
            // Arrange
            var webServices = new WebServices();
            var httpClient = new HttpClient();
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:5000/");
            httpListener.Start();

            var context = await httpListener.GetContextAsync();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/plain";
            context.Response.OutputStream.Write(System.Text.Encoding.UTF8.GetBytes("latest"), 0, 5);
            context.Response.OutputStream.Close();

            // Act
            await Task.Run(() => webServices.CheckModVersion());

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }
    }
}
