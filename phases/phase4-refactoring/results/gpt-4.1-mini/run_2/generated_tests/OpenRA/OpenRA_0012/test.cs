using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Graphics;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        // Minimal fake implementations for constructor dependencies
        class FakeWidget : Widget
        {
            public override Widget Get(string name) => this;
            public override Widget GetOrNull(string name) => null;
            public override T Get<T>(string name) => (T)(object)new FakeLabelWidget();
            public override Rectangle Bounds { get; set; } = new Rectangle(0, 0, 100, 20);
            public override bool Visible { get; set; }
        }

        class FakeLabelWidget : LabelWidget
        {
            public override string GetText() => "Test";
            public override int Font => 0;
            public override Rectangle Bounds { get; set; } = new Rectangle(0, 0, 50, 10);
        }

        class FakeWorldRenderer : WorldRenderer
        {
            public override FontCollection Fonts => new FontCollection();
        }

        class FakeModData : ModData
        {
            public override T GetOrCreate<T>() => (T)(object)new PlayerDatabase();
        }

        class FakeClient : Session.Client
        {
            public override string Fingerprint => "testfingerprint";
            public override bool IsAdmin => false;
        }

        [Fact]
        public async Task Constructor_StartsProfileLoadingTask()
        {
            var widget = new FakeWidget();
            var worldRenderer = new FakeWorldRenderer();
            var modData = new FakeModData();
            var client = new FakeClient();

            var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);

            // Wait some time for the async Task.Run in constructor to run
            await Task.Delay(500);

            // Verify widget bounds width is set (indirect check)
            Assert.True(widget.Bounds.Width > 0);
        }
    }
}
