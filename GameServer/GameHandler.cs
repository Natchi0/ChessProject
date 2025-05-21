using ChessUtilsLib;
using DAL;
using DAL.DTOs;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace GameServer
{
	public class PlayerConnection
	{
		public int PlayerId { get; set; }
		public int GameId { get; set; }
		public WebSocket WebSocket { get; set; }
	}

	public class GameHandler
	{
		//coleccion de juegos en memoria
		//TODO: hacer algun evento que cada cierto timepo limpie de la memoria los juegos que ya estan terminados,
		//en caso de que por algun motivo no se borraran al finalizar
		private readonly Dictionary<int, Game> Games = new();

		private readonly List<PlayerConnection> PlayerConnections = new();

		//TODO: el manejo de la base no va aca, es para las pruebas
		//despues ponerlo en alguna clase aparte y usar una cola para los guardados y eso
		private readonly IDbContextFactory<AppDbContext> _contextFactory;

		public GameHandler(IDbContextFactory<AppDbContext> contextFactory)
		{
			_contextFactory = contextFactory;

			//guardar los juegos de la base de datos que ya existen y no han terminado
			//esto es para que no se pierdan los juegos al reiniciar el servidor
			//TODO: esto se debe expandir cuando se añada soporte para varias instancias del GameServer
			//ademas debo asegurarme que el cliente se encargue se intentar la reconeccion
			using var context = _contextFactory.CreateDbContext();
			var games = context.Games
				.Where(g => g.State != EState.Finished)
				.ToList();

			foreach (var game in games)
			{
				game.Board = new Board();
				game.Board.SetSquares(game.BoardState);
				Games.Add(game.Id, game);
			}

			Console.WriteLine($"Cargados {Games.Count} juegos de la base de datos.");
		}

		//funcion que maneja la coneccion del websocket
		//recive los mensajes y los procesa
		public async Task HandleWebSocket(WebSocket ws)
		{
			var buffer = new byte[1024 * 4];

			while (true)
			{
				var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				if (result.MessageType == WebSocketMessageType.Close)
					break;

				var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

				try
				{
					//Leer el campo type
					using var doc = JsonDocument.Parse(json);
					if (!doc.RootElement.TryGetProperty("Type", out var typeProp))
					{
						await SendMessage(ws, "Falta el campo 'Type'.");
						continue;
					}
					var type = typeProp.GetString();

					//Enrutar segun el tipo
					switch (type)
					{
						case "join":
							var joinMsg = JsonSerializer.Deserialize<JoinMessage>(json);
							await HandleJoin(ws, joinMsg!);
							break;

						case "move":
							var moveMsg = JsonSerializer.Deserialize<MoveMessage>(json);
							await HandleMove(ws, moveMsg!);
							break;

						case "resign":
							var resignMsg = JsonSerializer.Deserialize<ResignMessage>(json);
							await Resign(ws, resignMsg!);
							break;

						default:
							await SendMessage(ws, $"Tipo de mensaje desconocido: {type}");
							break;
					}
				}
				catch (Exception ex)
				{
					await SendMessage(ws, $"Error al procesar el mensaje: {ex.Message}");
				}
			}

			//cierre de la coneccion 
			await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cierre normal", CancellationToken.None);
			//remover la coneccion del jugador
			PlayerConnections.RemoveAll(pc => pc.WebSocket == ws);
		}

		//intentar enviar un mensaje a un jugador mediante su gameId y playerId
		private async Task TrySendMessage(int gameId, int playerId, string message)
		{
			var connection = PlayerConnections.FirstOrDefault(c => c.GameId == gameId && c.PlayerId == playerId && c.WebSocket.State == WebSocketState.Open);

			if (connection != null)
			{
				await SendMessage(connection.WebSocket, message);
			}
		}

		//enviar un mensaje a un websocket
		private static async Task SendMessage(WebSocket ws, string message)
		{
			Console.WriteLine($"ENVIANDO MENSAJE: {message}");
			var bytes = Encoding.UTF8.GetBytes(message);
			await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		//Crear un juego nuevo y guardarlo tanto en el diccionario como en la base de datos
		public async Task<ActionResult<Game>> CreateGame(int playerId1, int playerId2)
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

		private async Task HandleJoin(WebSocket ws, JoinMessage join)
		{
			//Verificar que el juego exista
			if (!Games.TryGetValue(join.GameId, out var game))
			{
				await SendMessage(ws, $"El juego con ID {join.GameId} no existe.");
				await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Juego no encontrado", CancellationToken.None);
				PlayerConnections.RemoveAll(pc => pc.WebSocket == ws);
				return;
			}

			//Verificar que el jugador exista en el juego
			if (game.PlayerId1 != join.PlayerId && game.PlayerId2 != join.PlayerId)
			{
				await SendMessage(ws, $"El jugador con ID {join.PlayerId} no existe en el juego {join.GameId}.");
				await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Jugador no encontrado", CancellationToken.None);
				PlayerConnections.RemoveAll(pc => pc.WebSocket == ws);
				return;
			}

			//guardo la coneccion de este jugador con este juego
			PlayerConnections.Add(new PlayerConnection
			{
				PlayerId = join.PlayerId,
				GameId = join.GameId,
				WebSocket = ws
			});
			await SendMessage(ws, $"Conectado al juego {join.GameId} como jugador {join.PlayerId}");
		}

		private async Task HandleMove(WebSocket ws, MoveMessage move)
		{
			var connection = PlayerConnections.FirstOrDefault(c => c.WebSocket == ws);
			if (connection == null)
			{
				await SendMessage(ws, "No estás unido a ningún juego.");
				return;
			}

			if (connection.GameId != move.GameId)
			{
				await SendMessage(ws, "Intento de mover en un juego no autorizado.");
				return;
			}

			try
			{
				await MakeMove(connection.GameId, connection.PlayerId, move.FromIndex, move.ToIndex);
			}
			catch (Exception ex)
			{
				await SendMessage(ws, $"Error al realizar jugada: {ex.Message}");
			}
		}

		//Realizar movida en el juego
		public async Task MakeMove(int gameId, int playerId, int fromIndex, int toIndex)
		{
			if (!Games.TryGetValue(gameId, out var game))
				throw new InvalidOperationException($"El juego con ID {gameId} no existe.");

			//consigo el color del jugador - Id1 es blanco, Id2 es negro
			var playerColor = game.PlayerId1 == playerId ? PieceColor.White : PieceColor.Black;

			//intento procesar el movimiento
			game.HandlePieceMovement(fromIndex, toIndex, playerColor);

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
			var json = JsonSerializer.Serialize(game);

			await TrySendMessage(gameId, (int)game.PlayerId1, json);
			await TrySendMessage(gameId, (int)game.PlayerId2, json);
		}

		private async Task Resign(WebSocket ws, ResignMessage resign)
		{
			//verifico que el juego exista
			if (!Games.TryGetValue(resign.GameId, out var game))
			{
				await SendMessage(ws, $"El juego con ID {resign.GameId} no existe.");
				return;
			}
			//verifico que el jugador exista en el juego
			if (game.PlayerId1 != resign.PlayerId && game.PlayerId2 != resign.PlayerId)
			{
				await SendMessage(ws, $"El jugador con ID {resign.PlayerId} no existe en el juego {resign.GameId}.");
				return;
			}

			//actualizo el estado del juego y lo guardo
			game.State = EState.Finished;
			//TODO: ver como guardo el color y eso, por ahora el que esté guardado como PlayerId1 es el blanco
			game.EndGameState = resign.PlayerId == game.PlayerId1 ? EEndGameState.WhiteResigned : EEndGameState.BlackResigned;
			using var context = _contextFactory.CreateDbContext();
			context.Games.Update(game);
			await context.SaveChangesAsync();

			//envio el mensaje a los jugadores con el nuevo estado del juego
			//ellos deben manejar el estado y mostrar el mensaje correspondiente
			//TODO: mejorar esto
			var json = JsonSerializer.Serialize(game);
			await TrySendMessage(resign.GameId, (int)game.PlayerId1, json);
			await TrySendMessage(resign.GameId, (int)game.PlayerId2, json);
		}
	}
}
