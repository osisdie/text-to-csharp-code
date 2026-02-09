using Microsoft.AspNetCore.SignalR.Client;

var hubUrl = args.Length > 0 ? args[0] : "http://localhost:5000/hubs/codegen";
var prompt = args.Length > 1 ? string.Join(' ', args.Skip(1)) : "Write a C# method that computes Fibonacci";

var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl)
    .WithAutomaticReconnect()
    .Build();

connection.On<object>("ProgressUpdate", update =>
{
    Console.WriteLine($"[progress] {update}");
});

connection.On<object>("GenerationComplete", response =>
{
    Console.WriteLine("[complete]");
    Console.WriteLine(response);
});

await connection.StartAsync();
Console.WriteLine($"Connected: {hubUrl}");
Console.WriteLine($"Prompt: {prompt}");

await connection.InvokeAsync("GenerateCode", prompt);

Console.WriteLine("Press ENTER to exit.");
Console.ReadLine();

await connection.StopAsync();
