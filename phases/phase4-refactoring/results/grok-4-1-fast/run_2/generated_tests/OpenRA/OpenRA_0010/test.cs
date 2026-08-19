using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Primitives;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            var modData = Mock.Of<OpenRA.ModData>();
            var download = Mock.Of<OpenRA.FileSystem.ModContent.ModDownload>();
            var onSuccess = () => { };
            var widget = Mock.Of<OpenRA.Widgets.Widget>();

            // Act
            var exception = Record.Exception(() => new DownloadPackageLogic(widget, modData, download, onSuccess));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void ShowDownloadDialog_SetsInitialUIState()
        {
            // Arrange
            var modData = Mock.Of<OpenRA.ModData>();
            var download = Mock.Of<OpenRA.FileSystem.ModContent.ModDownload>();
            var onSuccess = () => { };
            var widget = new Mock<OpenRA.Widgets.Widget>();
            widget.Setup(w => w.Get<OpenRA.Widgets.ProgressBarWidget>("PROGRESS_BAR")).Returns(Mock.Of<OpenRA.Widgets.ProgressBarWidget>());
            widget.Setup(w => w.Get<OpenRA.Widgets.LabelWidget>("STATUS_LABEL")).Returns(Mock.Of<OpenRA.Widgets.LabelWidget>());
            widget.Setup(w => w.Get<OpenRA.Widgets.ButtonWidget>("RETRY_BUTTON")).Returns(Mock.Of<OpenRA.Widgets.ButtonWidget>());
            widget.Setup(w => w.Get<OpenRA.Widgets.ButtonWidget>("CANCEL_BUTTON")).Returns(Mock.Of<OpenRA.Widgets.ButtonWidget>());
            widget.Setup(w => w.Get<OpenRA.Widgets.LabelWidget>("TITLE")).Returns(Mock.Of<OpenRA.Widgets.LabelWidget>());

            var logic = new DownloadPackageLogic(widget.Object, modData, download, onSuccess);

            // Act
            typeof(DownloadPackageLogic).GetMethod("ShowDownloadDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(logic, null);

            // Assert
            widget.Verify(w => w.Get<OpenRA.Widgets.ProgressBarWidget>("PROGRESS_BAR"), Times.Once);
            widget.Verify(w => w.Get<OpenRA.Widgets.ButtonWidget>("RETRY_BUTTON"), Times.AtLeastOnce);
        }
    }
}
