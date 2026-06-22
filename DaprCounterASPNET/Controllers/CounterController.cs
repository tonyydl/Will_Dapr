using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CounterController : ControllerBase
{
    [HttpGet("counter")]
    public async Task<IActionResult> GetCounter([FromServices] DaprClient daprClient)
    {
        int counter = await daprClient.GetStateAsync<int>("statestore", "counter");
        return Ok(counter);
    }

    [HttpPut("counter")]
    public async Task<IActionResult> PutCounter([FromServices] DaprClient daprClient)
    {
        var counter = await daprClient.GetStateEntryAsync<int>("statestore", "counter");

        counter.Value += 1;

        if (await counter.TrySaveAsync())
        {
            return Ok(counter.Value);
        }
        else
        {
            return BadRequest();
        }
    }
}
