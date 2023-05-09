using Dapr.Workflow;
using System.Text;

namespace ChainingWorkflowSample
{
    public class ChainingWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {
            var greetings = new List<string>(); 

            greetings.Add(await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input));

            greetings.Add(await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input));

            greetings.Add(await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input));

            var sb = new StringBuilder();
            
            foreach (var greeting in greetings)
            {
                sb.AppendLine($"{greeting} + {greetings.FindIndex(g => g.Contains(greeting))}");
            }

            return sb.ToString(); 
        }
    }
}