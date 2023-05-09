using Dapr.Workflow;
using Dapr.Client;
using CheckoutServiceWorkflowSample.Models;

namespace CheckoutServiceWorkflowSample.Activities
{
    public class CheckInventoryActivity : WorkflowActivity<InventoryRequest, object?>
    {
        readonly ILogger _logger;
        readonly DaprClient _client;
        static readonly string storeName = "statestore";

        public CheckInventoryActivity(ILoggerFactory loggerFactory, DaprClient client)
        {
            _logger = loggerFactory.CreateLogger<CheckInventoryActivity>();
            _client = client;
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, InventoryRequest req)
        {

            // Check inventory state store to get accurate inventory count 
            InventoryItem item = await _client.GetStateAsync<InventoryItem>(
                storeName,
                req.ItemName.ToLowerInvariant());
            
            if(item == null)
            {
                return new InventoryResult(false, null, 0);
            }
            
            _logger.LogInformation(
                "Status: Found {quantity} {name} in inventory",
                item.Quantity,
                item.Name);

            var totalCost = req.Quantity * item.PerItemCost; 
            
            // See if there're enough items to purchase
            if (item.Quantity >= req.Quantity)
            {
                // Simulate slow processing
                await Task.Delay(TimeSpan.FromSeconds(2));
                
                return new InventoryResult(true, item, totalCost);
            }
                
            return new InventoryResult(false, item, totalCost);
        }
    }
}