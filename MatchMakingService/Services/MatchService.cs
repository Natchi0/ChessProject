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
		private readonly HttpClient _httpClient;

		public MatchService(AppDbContext context, HttpClient httpClient)
		{
			_context = context;
			_httpClient = httpClient;
		}

		public async Task<RequestMatchInfoDto> RequestMatch(int playerId, string connectionId)
		{
			if (!await PlayerExists(playerId))
				throw new Exception("El jugador no existe");

			//evitar que el jugador pida un match si ya tiene una en waiting
			if(await HasPendingMatchRequest(playerId))
				throw new Exception("Ya existe una request en waiting");

			//verificar que el jugador no tenga mas de 5 juegos activos 
			//TODO: Esto no tendra efecto hasta que logre implementar que un jugador tenga mas de una partida al mismo tiempo
			if (await GetActiveGamesCount(playerId) >= 1)//cambiar a 5, el 1 es temporal
				throw new Exception("El jugador ya tiene 5 juegos activos");

			var matchRequest = await GetOldestWaitingRequest();

			if (matchRequest != null)
			{
				Console.WriteLine($"Se encontro una match request en waiting: {matchRequest.Id} - PlayerId: {matchRequest.PlayerId} - MatchedPlayerId: {playerId}");
				CreateGameDto gameRequest = new CreateGameDto
				{
					Player1Id = matchRequest.PlayerId,
					Player2Id = playerId
				};

				//TODO: cambiar para usar rabbit
				//Crear el juego
				var gameResponse = await _httpClient.PostAsJsonAsync("http://gameserver:8080/createGame", gameRequest);
				if (!gameResponse.IsSuccessStatusCode)
					throw new Exception("Error al crear el juego");

				var createdGame = await gameResponse.Content.ReadFromJsonAsync<Game>();

				if (createdGame == null)
					throw new Exception("Error al deserializar el juego creado");
				
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
			matchRequest = MatchRequest.CreateWaiting(playerId);

			_context.MatchRequests.Add(matchRequest);
			await _context.SaveChangesAsync();

			return new RequestMatchInfoDto
			{
				MatchRequest = matchRequest,
				MatchInfo = null
			};
		}

		private async Task<bool> PlayerExists(int playerId)
		{
			return await _context.Players.AnyAsync(p => p.Id == playerId);
		}

		private async Task<bool> HasPendingMatchRequest(int playerId)
		{
			return await _context.MatchRequests
				.AnyAsync(mr => mr.PlayerId == playerId && mr.Status == EMatchRequestStatus.Waiting);
		}

		private async Task<int> GetActiveGamesCount(int playerId)
		{
			return await _context.Games
				.CountAsync(g => (g.PlayerId1 == playerId || g.PlayerId2 == playerId) && g.State != EState.Finished);
		}

		private async Task<MatchRequest?> GetOldestWaitingRequest()
		{
			//buscar una match request en waiting, la mas vieja - FIFO
			//TODO esto es vital movel a redis para evitar que un cuello de botella al intentar leer del sql
			return await _context.MatchRequests
				.Where(mr => mr.Status == EMatchRequestStatus.Waiting)
				.OrderBy(mr => mr.RequestedAt)
				.FirstOrDefaultAsync();
		}
	}
}
