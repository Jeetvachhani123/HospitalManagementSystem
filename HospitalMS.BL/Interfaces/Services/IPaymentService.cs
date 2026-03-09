using System.Threading.Tasks;

namespace HospitalMS.BL.Interfaces.Services;

public interface IPaymentService
{
    Task<string> CreateCheckoutSessionAsync(int invoiceId, decimal amount, string currency, string successUrl, string cancelUrl);

    Task<bool> HandleWebhookAsync(string json, string signature);
}