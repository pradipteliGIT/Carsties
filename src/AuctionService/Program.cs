using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//This line will locate any classes derived from automapper profile and register mapper in memory
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

//Adding seed data start
try
{
    DbInitializer.InitializeDatabase(app);
}
catch(Exception ex)
{
    Console.WriteLine(ex.ToString());
}
//Adding seed data end

app.Run();
