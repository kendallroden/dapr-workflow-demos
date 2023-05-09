using Microsoft.AspNetCore.Mvc;
using PaymentService.Contracts;
using PaymentService.Models.Stripe;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    public class StripeController : Controller
    {
        private readonly IStripeAppService _stripeService;
        private static string failCard = "4000000000000341";
        private static string succeedCard = "4242424242424242";

        public StripeController(IStripeAppService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("payment")]
        public async Task<ActionResult<StripePayment>> Payment([FromBody] OrderPayload order,
            CancellationToken ct)
        {
            string cardToUse = succeedCard;
           
           if(order.failPayment)
           {
             cardToUse = failCard;
           }
            
            var customer = new AddStripeCustomer(
                Email: order.Email, 
                Name: order.Name, 
                CreditCard: new AddStripeCard(order.Name, cardToUse, "2024", "12", "210")); 
                  
            
            // Seed a stripe customer with appropriate card details
            var createdCustomer = await _stripeService.AddStripeCustomerAsync(customer, ct);

            var payment = new AddStripePayment(
                CustomerId: createdCustomer.CustomerId,
                ReceiptEmail: order.Email, 
                Description: $"Order payment for customer {order.Name}",
                Currency: "USD",
                Amount: ((long)order.TotalCost)
            );

            // Charge customer for requested amount 
            return await _stripeService.AddStripePaymentAsync(payment, ct); 
        }
   
    }
}

