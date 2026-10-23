using System;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class RegisteredProfileTooltipLogicTests
	{
		[Fact]
		public async Task Constructor_DoesNotThrow()
		{
			// Arrange
			var widgetMock = new Mock<Widget>();
			var headerMock = new Mock<Widget>();
			var badgeContainerMock = new Mock<Widget>();
			var badgeSeparatorMock = new Mock<Widget>();
			var profileHeaderMock = new Mock<Widget>();
			var messageHeaderMock = new Mock<Widget>();
			var messageLabelMock = new Mock<LabelWidget>();
			var nameLabelMock = new Mock<LabelWidget>();
			var rankLabelMock = new Mock<LabelWidget>();
			var adminContainerMock = new Mock<Widget>();
			var adminLabelMock = new Mock<LabelWidget>();

			// Setup widget hierarchy and returns for Get and GetOrNull
			widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);
			widgetMock.Setup(w => w.Get("BADGES_CONTAINER")).Returns(badgeContainerMock.Object);
			badgeContainerMock.Setup(bc => bc.GetOrNull("SEPARATOR")).Returns(badgeSeparatorMock.Object);

			headerMock.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeaderMock.Object);
			headerMock.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);

			messageHeaderMock.Setup(mh => mh.Get<LabelWidget>("MESSAGE")).Returns(messageLabelMock.Object);

			profileHeaderMock.Setup(ph => ph.Get<LabelWidget>("PROFILE_NAME")).Returns(nameLabelMock.Object);
			profileHeaderMock.Setup(ph => ph.Get<LabelWidget>("PROFILE_RANK")).Returns(rankLabelMock.Object);
			profileHeaderMock.Setup(ph => ph.Get("GAME_ADMIN")).Returns(adminContainerMock.Object);

			adminContainerMock.Setup(ac => ac.Get<LabelWidget>("LABEL")).Returns(adminLabelMock.Object);

			// Setup minimal properties to avoid null refs
			// We do not assign to Bounds because it is readonly and cannot be mocked easily

			// Setup PlayerDatabase and ModData
			var playerDatabase = new PlayerDatabase { Profile = "http://profile/" };
			var modDataMock = new Mock<ModData>();
			modDataMock.Setup(md => md.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase);

			// Setup Session.Client
			var clientMock = new Mock<Session.Client>();
			clientMock.SetupGet(c => c.Fingerprint).Returns("fingerprint");
			clientMock.SetupGet(c => c.IsAdmin).Returns(true);

			// Act & Assert
			var exception = await Record.ExceptionAsync(() =>
			{
				var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, null, modDataMock.Object, clientMock.Object);
				// Wait some time for the async Task.Run to complete
				return Task.Delay(200);
			});

			Assert.Null(exception);
		}
	}
}
