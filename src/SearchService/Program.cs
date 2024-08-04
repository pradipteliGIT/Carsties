using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{//Initialoize mongoDB start
    try
    {
        await DatabaseInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
    //Initialize mongoDB end

});
app.Run();

//Http pooling to request for auction data call
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    =>HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg=>msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_=>TimeSpan.FromSeconds(3));


