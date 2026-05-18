using Xunit;
using Moq;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Models;
using Bit.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Threading.Tasks;

namespace Bit.Core.Billing.Tests.Services.Implementations
{
    public class SubscriberServiceTests
    {
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<ILogger<SubscriberService>> _loggerMock;
        private readonly SubscriberService _subscriberService;

        public SubscriberServiceTests()
        {
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _taxServiceMock = new Mock<ITaxService>();
            _loggerMock = new Mock<ILogger<SubscriberService>>();
            _subscriberService = new SubscriberService(
                null,
                null,
                _loggerMock.Object,
                null,
                null,
                null,
                _stripeAdapterMock.Object,
                _taxServiceMock.Object,
                null);
        }

        [Fact]
        public async Task UpdateTaxInformation_ShouldLogWarning_WhenTaxIdTypeIsNull()
        {
            // Arrange
            var taxInformation = new TaxInformation("US", "12345", "123456789", null, "123 Main St", null, "Anytown", "CA");
            var customer = new Customer { Id = "cus_123" };
            _taxServiceMock.Setup(x => x.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>())).Returns((string)null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _subscriberService.UpdateTaxInformation(customer, taxInformation));
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}
