using Dapr.Workflow;
using FanOutInWorkflowSample.Models;
using FanOutInWorkflowSample.Activities;
using System.Text;

namespace FanOutInWorkflowSample.Workflows
{
    // Inherits from the base workflow class 
    public class FanOutInWorkflow : Workflow<GreetingsRequest, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, GreetingsRequest greetings)
        {
            string processId = context.InstanceId;

            if (!context.IsReplaying)
            {
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new Notification($"Received greetings list, processing {processId}.... "));
            }

            List<Task<string>> tasks = new();

            foreach (var greeting in greetings.Greetings)
            {
                var result = context.CallActivityAsync<string>(nameof(GreetingActivity), greeting);

                tasks.Add(result);
            }

            await Task.WhenAll(tasks);

            if (!context.IsReplaying)
            {
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new Notification($"All greetings processed for {processId}.... "));
            }

            var sb = new StringBuilder();
            foreach (var completedParallelActivity in tasks)
            {
                sb.AppendLine(completedParallelActivity.Result);
            }

            return sb.ToString();
        }
    }
}