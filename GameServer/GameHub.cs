using Microsoft.AspNetCore.SignalR;
using System.Net.WebSockets;

namespace GameServer
{
	public class MoveRequestDto
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
	}

	public class GameHub : Hub
	{
		readonly GameHandler _handler;
		public GameHub(GameHandler handler)
		{
			_handler = handler;
		}

		public Task JoinGame(int gameId)
		{
			//TODO: hacer validaciones
			//Verificar que el juego exista

			//Verificar que el jugador exista en el juego / su id sea uno de los del juego
			Console.WriteLine($"JoinGame: Player - {Context.ConnectionId} - Game {gameId}");
			return Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(gameId));
		}

		public Task LeaveGame(int gameId)
		{
			return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(gameId));
		}

		public Task MakeMove(int gameId, int playerId, int fromIndex, int toIndex)
		{
			return Clients.Group(GetGroupName(gameId)).SendAsync("MakeMove", gameId, playerId, fromIndex, toIndex);
		}

		public async Task Move(MoveRequestDto move)
		{
			// TODO: agrgar validaciones como que la coneccion corresponda a la partida y eso
			Console.WriteLine($"Move: Player - {Context.ConnectionId} - Game {move.GameId} - From {move.FromIndex} - To {move.ToIndex}");
			try
			{
				//GameHandler publicara el evento GameUpdated en caso de ser exitoso
				await _handler.MakeMove(move.GameId, move.PlayerId, move.FromIndex, move.ToIndex);
			}
			catch (Exception ex)
			{
				//en caso de un erro se le envia el mensaje al jugador que hizo la jugada
				await Clients.Caller.SendAsync("MoveRejected", ex.Message);
			}
		}

		public static string GetGroupName(int gameId)
		{
			return $"game-{gameId}";
		}
	}
}
