namespace PaymentService.Models.Stripe
{
    public record PaymentRequest(bool failCheckout, string RequestId, string Name, string Email, double TotalCost);

    public record StripeCustomer(string Name, string Email, string CustomerId);
    public record AddStripeCustomer(string Email, string Name, AddStripeCard CreditCard);
    public record AddStripeCard(string Name, string CardNumber, string ExpirationYear, string ExpirationMonth, string Cvc);

    public record AddStripePayment(string CustomerId, string ReceiptEmail, string Description, string Currency, long Amount);
    public record StripePayment(string CustomerId, string ReceiptEmail, string Description, string Currency, long Amount, string PaymentId);
}