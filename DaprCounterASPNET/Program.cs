var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddDapr();

var app = builder.Build();

app.MapControllers();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
