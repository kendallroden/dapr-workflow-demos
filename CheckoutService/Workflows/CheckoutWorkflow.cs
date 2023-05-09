using Dapr.Workflow;
using DurableTask.Core.Exceptions;

using CheckoutServiceWorkflowSample.Activities;
using CheckoutServiceWorkflowSample.Models;

namespace CheckoutServiceWorkflowSample.Workflows
{
    public class CheckoutWorkflow : Workflow<CustomerOrder, CheckoutResult>
    {
        //Function chaining 
        public override async Task<CheckoutResult> RunAsync(WorkflowContext context, CustomerOrder order)
        {
            string orderId = context.InstanceId;

            // Notify an Checkout has been received
            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Received order {orderId} for {order.OrderItem.Quantity} {order.OrderItem.Name}"));

            // Determine if there is enough product inventory to fulfill the order request 
            var inventoryResult = new InventoryResult(false, null, 0);
            try {
            
                context.SetCustomStatus("Checking product inventory");

                inventoryResult = await context.CallActivityAsync<InventoryResult>(
                    nameof(CheckInventoryActivity),
                    order);
                
                if (!inventoryResult.Available)
                {
                    // End the workflow here since we don't have sufficient inventory
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"{orderId} cancelled: Insufficient inventory available"));
                    
                    context.SetCustomStatus("Insufficient inventory to fulfill order");
                    
                    return new CheckoutResult(Processed: false);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Checkout for order: {orderId} failed due to {ex.InnerException.Message}"));
                        
                    context.SetCustomStatus($"Error processing request: {ex.InnerException.Message}");
                    
                    return new CheckoutResult(Processed: false);
                }
            }
        
            // Process payment for the order 
            try {
                
                context.SetCustomStatus("Payment processing");
                
                await context.CallActivityAsync(
                    nameof(ProcessPaymentActivity),
                    new PaymentRequest(false, RequestId: orderId, inventoryResult)); 
            }
            catch (Exception ex) {
                
                if (ex.InnerException is TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Processing payment for {orderId} failed due to {ex.Message}"));
                        context.SetCustomStatus("Payment failed to process");
                        return new CheckoutResult(Processed: false);
                }
            }
            
            // Decrement inventory to account for execution of purchase 
            try
            {
                await context.CallActivityAsync(
                    nameof(UpdateInventoryActivity),
                    order);
                
                context.SetCustomStatus("Updating inventory as a result of order payment");
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskFailedException)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyActivity),
                        new Notification($"Checkout for order {orderId} failed! Processing payment refund."));
                    
                    context.SetCustomStatus("Issuing refund due to insufficient inventory to fulfill");

                    await context.CallActivityAsync(
                        nameof(RefundPaymentActivity),
                        new PaymentRequest(false, RequestId: orderId, inventoryResult));
                    
                    context.SetCustomStatus("Payment refunded");

                    return new CheckoutResult(Processed: false);
                }
            }

            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Checkout for order {orderId} has completed!"));
            
            context.SetCustomStatus("Checkout completed");

            return new CheckoutResult(Processed: true);
        }
    }
}