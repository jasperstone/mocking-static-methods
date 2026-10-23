using System;
using System.Threading.Tasks;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
	public class WebServicesTests
	{
		[Fact]
		public void CheckModVersion_DoesNotThrow_AndInitialStatusIsNotChecked()
		{
			// Arrange
			var webServices = new WebServices();

			// Assert initial status
			Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);

			// Act & Assert: calling CheckModVersion does not throw
			var ex = Record.Exception(() => webServices.CheckModVersion());
			Assert.Null(ex);
		}
	}
}
