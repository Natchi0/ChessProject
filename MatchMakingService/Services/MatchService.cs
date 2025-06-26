using ChessUtilsLib;
using DAL;
using DAL.DTOs;
using DAL.Models;
using MatchMakingService.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MatchMakingService.Services
{
	public class MatchService
	{
		private readonly AppDbContext _context;
		private readonly IHubContext<MatchHub> _hub;
		private readonly HttpClient _httpClient;

		public MatchService(AppDbContext context, IHubContext<MatchHub> hubContext, HttpClient httpClient)
		{
			_context = context;
			_hub = hubContext;
			_httpClient = httpClient;
		}

		public async Task<RequestMatchInfoDto> RequestMatch(int playerId, string connectionId)
		{
			//chequear que el jugador exista
			var player = await _context.Players.FindAsync(playerId);
			if (player == null)
			{
				throw new Exception("El jugador no existe");
			}
			Console.WriteLine($"El jugador {playerId} existe");

			//evitar que el jugador pida un match si ya tiene una en waiting
			var existingRequest = await _context.MatchRequests
				.FirstOrDefaultAsync(mr => mr.PlayerId == playerId && mr.Status == EMatchRequestStatus.Waiting);

			if (existingRequest != null)
			{
				throw new Exception("Ya existe una request en waiting");
			}
			Console.WriteLine($"El jugador no tiene una request en waiting");

			//verificar que el jugador no tenga mas de 5 juegos activos 
			//TODO: Esto no tendra efecto hasta que logre implementar que un jugador tenga mas de una partida al mismo tiempo
			var activeGames = await _context.Games
				.Where(g => (g.PlayerId1 == playerId || g.PlayerId2 == playerId) && g.State != EState.Finished)
				.ToListAsync();

			if (activeGames.Count >= 1)//cambiar a 5, el 1 es temporal
			{
				Console.WriteLine($"EL JUGADRO TIENE {activeGames.Count} GAMES");
				throw new Exception("El jugador ya tiene 5 juegos activos");
			}
			Console.WriteLine($"El jugador {playerId} tiene {activeGames.Count} juegos activos");

			//buscar una match request en waiting, la mas vieja - FIFO
			var matchRequest = await _context.MatchRequests
				.Where(mr => mr.Status == EMatchRequestStatus.Waiting)
				.OrderBy(mr => mr.RequestedAt)
				.FirstOrDefaultAsync();

			if (matchRequest != null)
			{
				Console.WriteLine($"Se encontro una match request en waiting: {matchRequest.Id} - PlayerId: {matchRequest.PlayerId} - MatchedPlayerId: {playerId}");
				CreateGameRequest gameRequest = new CreateGameRequest
				{
					Id1 = matchRequest.PlayerId,
					Id2 = playerId
				};
				Console.WriteLine("llamando al gameEngine");
				//TODO: cambiar para usar rabbit
				//Crear el juego
				var gameResponse = await _httpClient.PostAsJsonAsync("http://gameserver:8080/createGame", gameRequest);
				if (!gameResponse.IsSuccessStatusCode)
					throw new Exception("Error al crear el juego");

				var createdGame = await gameResponse.Content.ReadFromJsonAsync<Game>();

				if (createdGame == null)
					throw new Exception("Error al deserializar el juego creado");

				Console.WriteLine("se deserializo el jeugo");
				
				//hay una request en waiting, la emparejo con el jugador
				matchRequest.MatchedPlayerId = playerId;
				matchRequest.Status = EMatchRequestStatus.Accepted;
				matchRequest.GameId = createdGame.Id;

				_context.MatchRequests.Update(matchRequest);
				await _context.SaveChangesAsync();


				//Genero la informacion de la partida qe se le enviará a los jugadores
				MatchInfo matchInfo = new MatchInfo
				{
					Player1Id = matchRequest.PlayerId,
					Player2Id = playerId,
					GameId = createdGame.Id,
					BoardState = createdGame.BoardState
				};

				return new RequestMatchInfoDto
				{
					MatchRequest = matchRequest,
					MatchInfo = matchInfo
				};
			}

			Console.WriteLine("No se encontro una match request en waiting");
			//si no se fue es porque no hay match requests en waiting, creo una
			matchRequest = new MatchRequest
			{
				PlayerId = playerId,
				RequestedAt = DateTime.UtcNow,
				Status = EMatchRequestStatus.Waiting,
			};

			_context.MatchRequests.Add(matchRequest);
			await _context.SaveChangesAsync();

			Console.WriteLine($"Se creo una nueva match request: {matchRequest.Id} - PlayerId: {matchRequest.PlayerId}");

			return new RequestMatchInfoDto
			{
				MatchRequest = matchRequest,
				MatchInfo = null
			};
		}
	}
}
