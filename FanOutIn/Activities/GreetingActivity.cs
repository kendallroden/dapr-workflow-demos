using Dapr.Client;
using Dapr.Workflow;
using FanOutInWorkflowSample.Models;

namespace FanOutInWorkflowSample.Activities
{
    public class GreetingActivity : WorkflowActivity<Greeting, string>
    {
        readonly ILogger _logger;

        public GreetingActivity(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GreetingActivity>();
        }

        public override Task<string> RunAsync(WorkflowActivityContext context, Greeting greeting)
        {
            _logger.LogInformation($"Generating greeting for {greeting.CityName}");

            // Simulate slow processing
            Task.Delay(TimeSpan.FromSeconds(5));

            var message = $"{greeting.Message} {greeting.CityName}";

            return Task.FromResult<string>(message);
        }
    }
}