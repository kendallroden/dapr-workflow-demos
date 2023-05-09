using Dapr.Workflow;
using CheckoutServiceWorkflowSample.Models;

namespace CheckoutServiceWorkflowSample.Activities
{
    public class RefundPaymentActivity : WorkflowActivity<PaymentRequest, object?>
    {
        readonly ILogger _logger;

        public RefundPaymentActivity(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RefundPaymentActivity>();
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Refunding payment: {CheckoutId} for ${totalCost}",
                req.CheckoutId,
                req.TotalCost);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(2));

            _logger.LogInformation(
                "Payment for request ID '{CheckoutId}' refunded successfully",
                req.CheckoutId);

            return null;
        }
    }
}