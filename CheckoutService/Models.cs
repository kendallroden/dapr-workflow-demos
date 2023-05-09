namespace CheckoutServiceWorkflowSample.Models
{
    public record CheckoutPayload(string Name, string Email, CheckoutItem CheckoutItem);
    public record CheckoutItem(string Name, int Quantity);
   
    public record InventoryItem(string Name, double PerItemCost, int Quantity);
    public record InventoryRequest(string RequestId, string ItemName, int Quantity);
    public record InventoryResult(bool Success, InventoryItem? CheckoutPayload, double TotalCost);

    public record PaymentRequest(bool failCheckout, string CheckoutId, string Name, string Email, double TotalCost);
    public record PaymentResponse(string test, string test2);

    public record CheckoutResult(bool Processed);
}