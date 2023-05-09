namespace PaymentService.Models.Stripe
{
    public record StripeCustomer(string Name,string Email,string CustomerId);
	public record AddStripeCard(string Name,string CardNumber,string ExpirationYear,string ExpirationMonth,string Cvc);
	public record AddStripeCustomer(string Email, string Name, AddStripeCard CreditCard);

	public record StripePayment(string CustomerId,string ReceiptEmail,string Description,string Currency,long Amount,string PaymentId);
	public record AddStripePayment(string CustomerId,string ReceiptEmail,string Description,string Currency,long Amount);

	public record OrderPayload(bool failPayment, string OrderId, string Name, string Email, double TotalCost);
    public record OrderItem(string Name, int Quantity);
}