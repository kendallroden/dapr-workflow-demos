using Dapr.Workflow;

namespace HelloWorldWorkflowSample
{
    public class HelloWorldWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {
            var message = await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input);

            return message;
        }
    }
}