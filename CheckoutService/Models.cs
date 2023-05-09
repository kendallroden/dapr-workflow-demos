namespace CheckoutServiceWorkflowSample.Models
{
    public record OrderItem(string Name, int Quantity);
    public record InventoryItem(int ProductId, string Name, double PerItemCost, int Quantity);
    public record InventoryResult(bool Available, InventoryItem? productItem, double TotalCost);

    public record CustomerOrder(bool FailCheckout, string Name, string Email, OrderItem OrderItem);

    public record PaymentRequest(bool FailCheckout, string RequestId, string Name, string Email, double TotalCost);
    public record PaymentResponse(bool Success); 

    public record CheckoutResult(bool Processed);
}