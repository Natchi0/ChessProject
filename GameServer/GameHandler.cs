using ChessUtilsLib;
using DAL;
using DAL.DTOs;
using DAL.Models;
using GameServer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace GameServer
{

	public class GameHandler
	{
		private readonly IHubContext<GameHub> _hub;

		//coleccion de juegos en memoria
		//TODO: hacer algun evento que cada cierto timepo limpie de la memoria los juegos que ya estan terminados,
		//en caso de que por algun motivo no se borraran al finalizar
		private readonly Dictionary<int, Game> Games = new();

		//TODO: el manejo de la base no va aca, es para las pruebas
		//despues ponerlo en alguna clase aparte y usar una cola para los guardados y eso
		private readonly IDbContextFactory<AppDbContext> _contextFactory;

		public GameHandler(IDbContextFactory<AppDbContext> contextFactory, IHubContext<GameHub> hubContext)
		{
			_contextFactory = contextFactory;
			_hub = hubContext;
		}

		//Crear un juego nuevo y guardarlo tanto en el diccionario como en la base de datos
		public async Task<Game> CreateGame(int playerId1, int playerId2)
		{
			var game = new Game
			{
				PlayerId1 = playerId1,
				PlayerId2 = playerId2,
				State = EState.WhiteTurn,
				StartedAt = DateTime.UtcNow,
			};
			game.InitGame();

			// Usar un contexto nuevo por operación
			using var context = _contextFactory.CreateDbContext();
			context.Games.Add(game);
			await context.SaveChangesAsync();

			Games.Add(game.Id, game);

			return game;
		}

		//Realizar movida en el juego
		public async Task<Game> MakeMove(int gameId, int playerId, int fromIndex, int toIndex)
		{
			if (!Games.TryGetValue(gameId, out var game))
				throw new InvalidOperationException($"El juego con ID {gameId} no existe.");

			//consigo el color del jugador - Id1 es blanco, Id2 es negro
			var playerColor = game.PlayerId1 == playerId ? PieceColor.White : PieceColor.Black;

			try
			{
				game.HandlePieceMovement(fromIndex, toIndex, playerColor);
			}
			catch (Exception ex)
			{
				throw new Exception($"Movimiento inválido: {ex.Message}");
			}

			//guardo en la base
			//TODO: usar alguna cola o algo para mandar los cambios a la base de datos
			using var context = _contextFactory.CreateDbContext();
			context.Games.Update(game);
			context.Moves.Add(new Move
			{
				GameId = game.Id,
				Game = game,
				MovNum = game.Turns,
				PlayerId = playerId,
				Movement = new int[] { fromIndex, toIndex }
			});
			await context.SaveChangesAsync();

			//envio el nuevo estado del juego
			return game;
			//var group = GameHub.GetGroupName(gameId);
			//await _hub.Clients.Group(group)
			//		 .SendAsync("GameUpdated", game);
		}

		//private async Task Resign(WebSocket ws, ResignMessage resign)
		//{
		//	//verifico que el juego exista
		//	if (!Games.TryGetValue(resign.GameId, out var game))
		//	{
		//		await SendMessage(ws, $"El juego con ID {resign.GameId} no existe.");
		//		return;
		//	}
		//	//verifico que el jugador exista en el juego
		//	if (game.PlayerId1 != resign.PlayerId && game.PlayerId2 != resign.PlayerId)
		//	{
		//		await SendMessage(ws, $"El jugador con ID {resign.PlayerId} no existe en el juego {resign.GameId}.");
		//		return;
		//	}

		//	//actualizo el estado del juego y lo guardo
		//	game.State = EState.Finished;
		//	//TODO: ver como guardo el color y eso, por ahora el que esté guardado como PlayerId1 es el blanco
		//	game.EndGameState = resign.PlayerId == game.PlayerId1 ? EEndGameState.WhiteResigned : EEndGameState.BlackResigned;
		//	using var context = _contextFactory.CreateDbContext();
		//	context.Games.Update(game);
		//	await context.SaveChangesAsync();

		//	//envio el mensaje a los jugadores con el nuevo estado del juego
		//	//ellos deben manejar el estado y mostrar el mensaje correspondiente
		//	//TODO: mejorar esto
		//	var json = JsonSerializer.Serialize(game);
		//	await TrySendMessage(resign.GameId, (int)game.PlayerId1, json);
		//	await TrySendMessage(resign.GameId, (int)game.PlayerId2, json);
		//}
	}
}
