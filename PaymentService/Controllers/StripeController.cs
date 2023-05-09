using System.Text.Json;
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
        public async Task<IActionResult> Payment([FromBody] PaymentRequest order,
            CancellationToken ct)
        {
            string cardToUse = succeedCard;
           
           if(order.failCheckout)
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
    
            try {
                var response = await _stripeService.AddStripePaymentAsync(payment, ct);

                return new OkObjectResult(new StringContent(JsonSerializer.Serialize(response)));
     
            }
            catch (Exception ex)
            {
                return StatusCode(500, new StringContent(JsonSerializer.Serialize(ex.Message)));
            }      
           
        }
   
    }
}

