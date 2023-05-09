namespace CheckoutServiceWorkflowSample.Models
{
    public record CustomerOrder(string Name, string Email, OrderItem OrderItem);
    public record OrderItem(string Name, int Quantity);
    public record InventoryItem(int ProductId, string Name, double PerItemCost, int Quantity);
    
    public record InventoryResult(bool Available, InventoryItem? productItem, double TotalCost);

    public record PaymentRequest(bool failCheckout, string RequestId, InventoryResult purchaseRequest);
    public record PaymentResponse(string test, string test2);

    public record CheckoutResult(bool Processed);
}