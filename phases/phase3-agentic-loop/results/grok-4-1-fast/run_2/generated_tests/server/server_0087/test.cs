using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Core.Services.Tests
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly StripePaymentService _service;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _taxServiceMock = new Mock<ITaxService>();

            _service = new StripePaymentService(
                null!,
                _loggerMock.Object,
                null!,
                null!,
                null!,
                null!,
                _taxServiceMock.Object,
                null!
            );
        }

        [Fact]
        public async Task FinalizeSubscriptionChangeAsync_LogsWarningAndThrows_WhenTaxIdInvalid()
        {
            // Arrange
            var taxId = "invalid-tax-id";
            var country = "ES";
            _taxServiceMock.Setup(x => x.GetStripeTaxCode(country, taxId)).Returns((string)null);

            var method = typeof(StripePaymentService).GetMethod(
                "FinalizeSubscriptionChangeAsync",
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            var subscriberMock = new Mock<ISubscriber>();
            subscriberMock.SetupGet(x => x.GatewaySubscriptionId).Returns("sub_test");
            var subscriptionUpdateMock = new Mock<SubscriptionUpdate>();

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await (Task<string>)method.Invoke(_service, new object[] { subscriberMock.Object, subscriptionUpdateMock.Object, false })!);

            // Assert exception thrown
            Assert.NotNull(exception);
            Assert.IsType<BadRequestException>(exception);

            // Verify LogWarning was called (line 1113 coverage)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) =>
                    {
                        var stateString = state?.ToString() ?? "";
                        return stateString.Contains("Invalid tax ID") &&
                               stateString.Contains(taxId) &&
                               stateString.Contains(country);
                    }),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
