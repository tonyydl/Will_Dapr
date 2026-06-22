using Dapr.Client;

var daprClient = new DaprClientBuilder().Build();

while (true)
{
    var counter = await daprClient.InvokeMethodAsync<int>(
        HttpMethod.Put,
        "DaprCounterASPNET",
        "counter/counter"
    );
    Console.WriteLine($"Counter = {counter}");
    await Task.Delay(1000);
}
