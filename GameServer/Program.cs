using DAL;
using DAL.DTOs;
using GameServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using System.Net;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
await ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);
ConfigureEndpoints(app);

app.Run();

async Task ConfigureServices(IServiceCollection services, ConfigurationManager config)
{
	//DbContextFactoy se usa para crear instancias sin romper los singletons y cosas asi
	//ya que se crean por cada request y asi se evitan compartir contextos entre distintas peticiones o hilos
	services.AddDbContextFactory<AppDbContext>(options =>
		options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

	services.AddSingleton<GameHandler>();

	services.AddCors(options =>
	{
		options.AddPolicy(name: "defaultPolicy", policy =>
		{
			policy.WithOrigins("http://localhost:5173")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
		});
	});

	services.AddSignalR();

	//inicializar RabbitMQ
	string rabbitHost = config["RabbitMQ:HostName"]!;
	await RabbitMQInitializer.SetupAsync(rabbitHost);
}

void ConfigureMiddleware(WebApplication app)
{
	app.UseCors("defaultPolicy");
}

//Apis minimas ya que no son muchos endpoints
void ConfigureEndpoints(WebApplication app)
{
	app.MapHub<GameHub>("/gameHub");

	//crear un nuevo juego
	app.MapPost("/createGame", async ([FromServices] GameHandler handler, [FromServices] IDbContextFactory<AppDbContext> dbContextFactory, [FromBody] CreateGameRequest request) =>
	{
		Console.WriteLine("CREANDO JUEGO Paso1");
		//verificar que los ids no sean iguales
		if (request.Id1 == request.Id2)
		{
			return Results.BadRequest("Los ids no pueden ser iguales");
		}

		//verificar que los jugadores existan
		using var db = dbContextFactory.CreateDbContext();
		var player1Exists = await db.Players.AnyAsync(p => p.Id == request.Id1);
		var player2Exists = await db.Players.AnyAsync(p => p.Id == request.Id2);

		Console.WriteLine("CREANDO JUEGO Paso 2");

		if (!player1Exists || !player2Exists)
		{
			return Results.BadRequest("Uno o ambos jugadores no existen");
		}

		var result = await handler.CreateGame(request.Id1, request.Id2);
		return Results.Ok(result);
	});

	app.MapGet("/", () => { return "Game Server is running"; });

}