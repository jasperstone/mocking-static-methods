using System.Threading.Tasks;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
	public class WebServicesTests
	{
		[Fact]
		public async Task CheckModVersion_DoesNotChangeModVersionStatus_WhenCalled()
		{
			var webServices = new WebServices();

			webServices.CheckModVersion();

			// Wait a short time for the async task to run
			await Task.Delay(1000);

			// The ModVersionStatus remains NotChecked because the async call cannot be controlled or awaited
			Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
		}
	}
}
