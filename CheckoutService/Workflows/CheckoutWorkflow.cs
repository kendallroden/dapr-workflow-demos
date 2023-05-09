using Dapr.Workflow;
using DurableTask.Core.Exceptions;

using CheckoutServiceWorkflowSample.Activities;
using CheckoutServiceWorkflowSample.Models;

namespace CheckoutServiceWorkflowSample.Workflows
{
    public class CheckoutProcessingWorkflow : Workflow<CheckoutPayload, CheckoutResult>
    {
        //Function chaining 
        public override async Task<CheckoutResult> RunAsync(WorkflowContext context, CheckoutPayload Checkout)
        {
            string CheckoutId = context.InstanceId;

            // Notify an Checkout has been received
            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Received Checkout request {CheckoutId} for {Checkout.CheckoutItem.Quantity} {Checkout.CheckoutItem.Name}"));

            // Determine if there is enough of the item available for purchase by checking the inventory
            var inventoryResult = new InventoryResult(false, null, 0);
            try {
            
                inventoryResult = await context.CallActivityAsync<InventoryResult>(
                    nameof(CheckInventoryActivity),
                    new InventoryRequest(RequestId: CheckoutId, Checkout.CheckoutItem.Name, Checkout.CheckoutItem.Quantity));
                
                context.SetCustomStatus("Success: Sufficient stock to fulfill Checkout");
                
                if (!inventoryResult.Success)
                {
                    // End the workflow here since we don't have sufficient inventory
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Insufficient inventory to fulfill {CheckoutId}"));
                    context.SetCustomStatus("Failure: Insufficient stock to fulfill Checkout");
                    return new CheckoutResult(Processed: false);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Checkout {CheckoutId} failed due to {ex.Message}"));
                        context.SetCustomStatus("Failure: Unable to retrieve inventory");
                        return new CheckoutResult(Processed: false);
                }
            }
        
            // Process payment for the requested item quantity 
            try {
                await context.CallActivityAsync(
                    nameof(ProcessPaymentActivity),
                    new PaymentRequest(false, CheckoutId, Checkout.Name, Checkout.Email, inventoryResult.TotalCost)); 
                
                context.SetCustomStatus("Success: Payment processed");
            }
            catch (Exception ex) {
                
                if (ex.InnerException is TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Processing payment for {CheckoutId} failed due to {ex.Message}"));
                        context.SetCustomStatus("Failure: Payment not processed");
                        return new CheckoutResult(Processed: false);
                }
            }
            
            // Decrement inventory to account for execution of purchase 
            try
            {
                await context.CallActivityAsync(
                    nameof(UpdateInventoryActivity),
                    new InventoryRequest(RequestId: CheckoutId, Checkout.CheckoutItem.Name, Checkout.CheckoutItem.Quantity));
                
                context.SetCustomStatus("Success: Inventory Updated");
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Checkout {CheckoutId} failed! Processing payment refund."));
                    await context.CallActivityAsync(
                        nameof(RefundPaymentActivity),
                        new PaymentRequest(false, CheckoutId, Checkout.Name, Checkout.Email, inventoryResult.TotalCost));
                    context.SetCustomStatus("Failure: Payment refunded due to insufficient inventory");

                    return new CheckoutResult(Processed: false);
                }
            }

            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Checkout {CheckoutId} has completed!"));
            
            context.SetCustomStatus("Success: Checkout completed");

            return new CheckoutResult(Processed: true);
        }
    }
}