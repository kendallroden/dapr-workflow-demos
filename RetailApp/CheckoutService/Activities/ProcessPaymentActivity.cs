using Dapr.Workflow;
using Dapr.Client;
using CheckoutServiceWorkflowSample.Models;

namespace CheckoutServiceWorkflowSample.Activities
{
    public class ProcessPaymentActivity : WorkflowActivity<PaymentRequest, PaymentResponse>
    {

        readonly ILogger _logger;

        public ProcessPaymentActivity(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();
        }


        public override async Task<PaymentResponse> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            // Use Dapr svc-to-svc to invoke Payment microservice 
            var invokeClient = DaprClient.CreateInvokeHttpClient();
            invokeClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            try
            {

                var response = await invokeClient.PostAsJsonAsync("http://payment/api/Stripe/payment", req);

                if (response.IsSuccessStatusCode)
                {
                    return new PaymentResponse(true);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    return new PaymentResponse(false);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}