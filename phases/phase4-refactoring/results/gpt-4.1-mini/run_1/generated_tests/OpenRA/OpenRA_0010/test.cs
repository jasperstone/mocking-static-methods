using System;
using System.Reflection;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
{
    public class DownloadPackageLogicTests
    {
        // Minimal dummy Widget implementation to satisfy constructor
        class DummyWidget : OpenRA.Widgets.Widget
        {
            public DummyWidget() : base(null) { }
        }

        [Fact]
        public void DownloadPackageLogic_ConstructWithMirrorList_DoesNotThrow()
        {
            // Arrange
            var modDataType = typeof(ModData);
            var modDataCtor = modDataType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            Assert.NotNull(modDataCtor);
            var modData = modDataCtor.Invoke(null);

            // Create ModDownload instance via reflection
            var modDownloadType = typeof(ModContent.ModDownload);
            var ctor = modDownloadType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object) }, null);
            Assert.NotNull(ctor);
            var download = ctor.Invoke(new object[] { null });

            // Set readonly fields via reflection
            void SetField(string name, object value)
            {
                var field = modDownloadType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Assert.NotNull(field);
                field.SetValue(download, value);
            }

            SetField("MirrorList", "https://example.com/mirrors.txt");
            SetField("URL", "https://example.com/file.zip");
            SetField("Title", "Test Download");
            SetField("Type", "zip");

            var onSuccess = new Action(() => { });
            var widget = new DummyWidget();

            // Act & Assert
            var ex = Record.Exception(() => Activator.CreateInstance(typeof(DownloadPackageLogic), widget, modData, download, onSuccess));
            Assert.Null(ex);
        }
    }
}
