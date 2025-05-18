using DAL;
using DAL.DTOs;
using GameServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

//endopont por defecto ws://localhost:5093 | http://localhost:5093
var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
app.UseWebSockets();

ConfigureMiddleware(app);
ConfigureEndpoints(app);

app.Run();

//configura los servicios de la aplicacion, como la base de datos y el GameHandler
void ConfigureServices(IServiceCollection services, ConfigurationManager config)
{
	//DbContextFactoy se usa para crear instancias sin romper los singletons y cosas asi
	//ya que se crean por cada request y asi se evitan compartir contextos entre distintas peticiones o hilos
	services.AddDbContextFactory<AppDbContext>(options =>
		options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

	services.AddSingleton<GameHandler>();
}

//middleware
void ConfigureMiddleware(WebApplication app)
{
	app.UseWebSockets();
	app.Urls.Add("http://localhost:6666");
}

//endpoints de la aplicacion
//Apis minimas ya que no son muchos endpoints
void ConfigureEndpoints(WebApplication app)
{
	//endpoint para el websocket
	//dentro del metodo se maneja la coneccion y los tipos de mensajes, join, makeMove, etc
	app.MapGet("/", async context =>
	{
		if (!context.WebSockets.IsWebSocketRequest)
		{
			context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			return;
		}

		var ws = await context.WebSockets.AcceptWebSocketAsync();
		var gameHandler = context.RequestServices.GetRequiredService<GameHandler>();
		await gameHandler.HandleWebSocket(ws);
	});

	//crear un nuevo juego
	app.MapPost("/createGame", async ([FromServices] GameHandler handler, [FromServices] IDbContextFactory<AppDbContext> dbContextFactory, [FromBody] CreateGameRequest request) =>
	{
		//verificar que los ids no sean iguales
		if (request.Id1 == request.Id2)
		{
			return Results.BadRequest("Los ids no pueden ser iguales");
		}

		//verificar que los jugadores existan
		using var db = dbContextFactory.CreateDbContext();
		var player1Exists = await db.Players.AnyAsync(p => p.Id == request.Id1);
		var player2Exists = await db.Players.AnyAsync(p => p.Id == request.Id2);

		if (!player1Exists || !player2Exists)
		{
			return Results.BadRequest("Uno o ambos jugadores no existen");
		}

		var result = await handler.CreateGame(request.Id1, request.Id2);
		return Results.Ok(result);
	});

}