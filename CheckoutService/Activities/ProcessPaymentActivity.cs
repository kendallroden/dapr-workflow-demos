using Dapr.Workflow;
using Dapr.Client;
using CheckoutServiceWorkflowSample.Models;

namespace CheckoutServiceWorkflowSample.Activities
{
    public class ProcessPaymentActivity : WorkflowActivity<PaymentRequest, object?>
    {

        readonly ILogger _logger;

        public ProcessPaymentActivity(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();
        }
        

        public override async Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Processing payment for order {RequestId} for {Quantity} {Name}",
                req.RequestId,
                req.purchaseRequest.productItem.Quantity,
                req.purchaseRequest.productItem.Name
                );
            
            // Use Dapr svc-to-svc to invoke Payment microservice 
            var invokeClient = DaprClient.CreateInvokeHttpClient(); 
            invokeClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var result = await invokeClient.PostAsJsonAsync("http://payment/api/Stripe/payment", req);

            return result.StatusCode; 
        }
    }
}