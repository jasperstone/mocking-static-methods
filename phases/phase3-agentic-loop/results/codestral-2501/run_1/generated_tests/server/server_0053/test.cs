using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Models;
using Bit.Core.Exceptions;
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

        // Mock method for testing purposes
        public async Task UpdateTaxInformation(Customer customer, TaxInformation taxInformation)
        {
            var taxId = customer.TaxIds?.FirstOrDefault();

            if (taxId != null)
            {
                await _stripeAdapterMock.Object.TaxIdDeleteAsync(customer.Id, taxId.Id);
            }

            if (!string.IsNullOrWhiteSpace(taxInformation.TaxId))
            {
                var taxIdType = taxInformation.TaxIdType;
                if (string.IsNullOrWhiteSpace(taxIdType))
                {
                    taxIdType = _taxServiceMock.Object.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId);

                    if (taxIdType == null)
                    {
                        _loggerMock.Object.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                            taxInformation.Country,
                            taxInformation.TaxId);

                        throw new BadRequestException("billingTaxIdTypeInferenceError");
                    }
                }

                try
                {
                    await _stripeAdapterMock.Object.TaxIdCreateAsync(customer.Id,
                        new TaxIdCreateOptions { Type = taxIdType, Value = taxInformation.TaxId });

                    if (taxIdType == StripeConstants.TaxIdType.SpanishNIF)
                    {
                        await _stripeAdapterMock.Object.TaxIdCreateAsync(customer.Id,
                            new TaxIdCreateOptions { Type = StripeConstants.TaxIdType.EUVAT, Value = $"ES{taxInformation.TaxId}" });
                    }
                }
                catch (StripeException e)
                {
                    switch (e.StripeError.Code)
                    {
                        case StripeConstants.ErrorCodes.TaxIdInvalid:
                            _loggerMock.Object.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.",
                                taxInformation.TaxId,
                                taxInformation.Country);

                            throw new BadRequestException("billingInvalidTaxIdError");

                        default:
                            throw;
                    }
                }
            }
        }
    }
}
