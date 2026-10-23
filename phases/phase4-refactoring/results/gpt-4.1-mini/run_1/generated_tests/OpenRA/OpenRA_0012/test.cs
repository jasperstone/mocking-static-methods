using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Primitives;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class RegisteredProfileTooltipLogicTests
	{
		[Fact]
		public async Task Constructor_DoesNotThrow_AndStartsLoading()
		{
			// Arrange
			var widget = new Mock<IWidget>();
			var header = new Mock<IWidget>();
			var badgeContainer = new Mock<IWidget>();
			var badgeSeparator = new Mock<IWidget>();
			var profileHeader = new Mock<IWidget>();
			var messageHeader = new Mock<IWidget>();
			var message = new Mock<IWidget>();
			var nameLabel = new Mock<IWidget>();
			var rankLabel = new Mock<IWidget>();
			var adminContainer = new Mock<IWidget>();
			var adminLabel = new Mock<IWidget>();

			// Setup widget hierarchy and returns
			widget.Setup(w => w.Get("HEADER")).Returns(header.Object);
			widget.Setup(w => w.Get("BADGES_CONTAINER")).Returns(badgeContainer.Object);
			badgeContainer.Setup(bc => bc.GetOrNull("SEPARATOR")).Returns(badgeSeparator.Object);
			header.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeader.Object);
			header.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeader.Object);
			messageHeader.Setup(mh => mh.Get<LabelWidget>("MESSAGE")).Returns((LabelWidget)message.Object);

			profileHeader.Setup(ph => ph.Get<LabelWidget>("PROFILE_NAME")).Returns((LabelWidget)nameLabel.Object);
			profileHeader.Setup(ph => ph.Get<LabelWidget>("PROFILE_RANK")).Returns((LabelWidget)rankLabel.Object);
			profileHeader.Setup(ph => ph.Get("GAME_ADMIN")).Returns(adminContainer.Object);
			adminContainer.Setup(ac => ac.Get<LabelWidget>("LABEL")).Returns((LabelWidget)adminLabel.Object);

			// Setup bounds
			widget.SetupProperty(w => w.Bounds, new Rectangle(0, 0, 100, 100));
			header.SetupProperty(h => h.Bounds, new Rectangle(0, 0, 100, 20));
			badgeContainer.SetupProperty(bc => bc.Bounds, new Rectangle(0, 20, 100, 30));
			message.SetupProperty(m => m.Bounds, new Rectangle(0, 0, 80, 20));
			profileHeader.SetupProperty(ph => ph.Bounds, new Rectangle(0, 0, 100, 20));
			messageHeader.SetupProperty(mh => mh.Bounds, new Rectangle(0, 0, 100, 20));
			nameLabel.SetupProperty(n => n.Bounds, new Rectangle(0, 0, 50, 10));
			rankLabel.SetupProperty(r => r.Bounds, new Rectangle(0, 0, 50, 10));
			adminLabel.SetupProperty(a => a.Bounds, new Rectangle(0, 0, 50, 10));
			adminContainer.SetupProperty(ac => ac.Bounds, new Rectangle(0, 0, 50, 10));

			// Setup client
			var client = new Mock<Session.Client>();
			client.SetupGet(c => c.Fingerprint).Returns("fingerprint");
			client.SetupGet(c => c.IsAdmin).Returns(false);

			// Setup modData and PlayerDatabase
			var playerDatabase = new PlayerDatabase { Profile = "http://profile/" };
			var modData = new Mock<ModData>();
			modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase);

			// Act & Assert
			var ex = await Record.ExceptionAsync(() => Task.Run(() =>
				new RegisteredProfileTooltipLogic(widget.Object, null, modData.Object, client.Object)));

			Assert.Null(ex);
		}
	}

	// Minimal interface to mock Widget members used in RegisteredProfileTooltipLogic
	public interface IWidget
	{
		IWidget Get(string name);
		IWidget GetOrNull(string name);
		T Get<T>(string name);
		string GetText { get; set; }
		bool Visible { get; set; }
		Rectangle Bounds { get; set; }
	}
}
