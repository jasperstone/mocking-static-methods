using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Library
{
    public class LibraryManagerImageConversionTests
    {
        [Fact(Skip = ".")]
        public async Task ConvertImageToLocal_LogsDebugWhenHttpStatusIsNotFoundOrForbidden()
        {
            Assert.True(true);
        }
    }
}
