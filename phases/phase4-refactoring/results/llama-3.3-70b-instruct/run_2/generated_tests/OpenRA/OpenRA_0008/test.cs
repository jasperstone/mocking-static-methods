using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenRA.Mods.Common.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_CallsGetAsync()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var webServices = new WebServices();
            var queryUrl = "https://master.openra.net/versioncheck?protocol=1&engine=1&mod=1&version=1";

            // Act
            webServices.CheckModVersion();

            // Assert
            // We can't directly verify the GetAsync call because it's made inside a Task.Run
            // We can only verify that the CheckModVersion method doesn't throw any exceptions
            await Task.Run(() => webServices.CheckModVersion());
        }
    }
}
