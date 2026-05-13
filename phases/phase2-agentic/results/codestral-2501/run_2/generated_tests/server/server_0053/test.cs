using Xunit;
using Moq;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Constants;
using Bit.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Threading.Tasks;

namespace Bit.Core.Tests.Billing.Services.Implementations
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
                null, null, _loggerMock.Object, null, null, null, _stripeAdapterMock.Object, _taxServiceMock.Object, null);
        }

        [Fact]
        public async Task UpdateTaxInformation_ShouldLogWarning_WhenTaxIdTypeIsNull()
        {
            // Arrange
            var taxInformation = new TaxInformation("US", "12345", "123456789", null, "123 Main St", null, "Anytown", "CA");
            var customer = new Customer { Id = "cus_123" };

            _stripeAdapterMock.Setup(x => x.CustomerGetAsync(It.IsAny<string>(), It.IsAny<CustomerGetOptions>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(customer);

            _taxServiceMock.Setup(x => x.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string)null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _subscriberService.UpdateTaxInformation(taxInformation, "cus_123"));

            _loggerMock.Verify(
                x => x.LogWarning(
                    "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTaxInformation_ShouldLogWarning_WhenTaxIdIsInvalid()
        {
            // Arrange
            var taxInformation = new TaxInformation("US", "12345", "123456789", "eu_vat", "123 Main St", null, "Anytown", "CA");
            var customer = new Customer { Id = "cus_123" };

            _stripeAdapterMock.Setup(x => x.CustomerGetAsync(It.IsAny<string>(), It.IsAny<CustomerGetOptions>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(customer);

            _stripeAdapterMock.Setup(x => x.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>(), It.IsAny<RequestOptions>()))
                .ThrowsAsync(new StripeException(new StripeError { Code = StripeConstants.ErrorCodes.TaxIdInvalid }));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _subscriberService.UpdateTaxInformation(taxInformation, "cus_123"));

            _loggerMock.Verify(
                x => x.LogWarning(
                    "Invalid tax ID '{TaxID}' for country '{Country}'.",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
