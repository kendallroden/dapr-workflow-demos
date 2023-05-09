namespace FanOutInWorkflowSample.Models
{
    public record Greeting(string CityName, string Message);

    public record GreetingsRequest(List<Greeting> Greetings);
}