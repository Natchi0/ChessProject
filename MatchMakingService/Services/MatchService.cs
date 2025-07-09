using ChessUtilsLib;
using DAL;
using DAL.DTOs;
using DAL.Models;
using MatchMakingService.Dtos;
using MatchMakingService.MessageServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MatchMakingService.Services
{
	public class MatchService
	{
		private readonly AppDbContext _context;
		private readonly IMessageBusClient _messageBusClient;
		private readonly HttpClient _httpClient;

		public MatchService(AppDbContext context, IMessageBusClient messageBusClient, HttpClient httpClient)
		{
			_context = context;
			_messageBusClient = messageBusClient;
			_httpClient = httpClient;
		}

		//si no hay match request en waiting, crea una nueva y no devuelvo ningun evento ya que queda esperando
		//si hay match request en waiting, emparejo y mando el evento de crear juego, ese despues se encarga de crear el juego y avisar al usuario
		public async Task RequestMatch(int playerId, string connectionId)
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
				//hay una match request en waiting, la emparejo con el jugador
				//y mando el evento al GameServer para crear el juego
				Console.WriteLine($"Se encontro una match request en waiting: {matchRequest.Id} - PlayerId: {matchRequest.PlayerId} - MatchedPlayerId: {playerId}");
				CreateGameDto gameRequest = new CreateGameDto
				{
					Player1Id = matchRequest.PlayerId,
					Player2Id = playerId,
					MatchRequestId = matchRequest.Id
				};

				//publicar el evento de crear juego
				await _messageBusClient.PublishCreateGameAsync(gameRequest);

				//publicar el evento para avisar que se hizo el match
				MatchFoundPublishDto matchFound = new MatchFoundPublishDto
				{
					Player1Id = matchRequest.PlayerId,
					Player2Id = playerId
				};
				await _messageBusClient.PublishMatchFoundAsync(matchFound);
				return;
			}

			//si no se fue es porque no hay match requests en waiting, creo una
			Console.WriteLine("No se encontro una match request en waiting");

			matchRequest = MatchRequest.CreateWaiting(playerId);

			_context.MatchRequests.Add(matchRequest);
			await _context.SaveChangesAsync();
		}

		// Finaliza el proceso de emparejamiento una vez que se ha creado el juego
		// esto solamente sirve para que la info en la base de datos sea consistente
		// el mismo evento que provoca esta funcion es recivido por el SocketService para que los jugadores inicien el jeugo
		public async Task FinishRequestMatchProcess(GameCreatedDto createdGameInfo)
		{
			if (createdGameInfo == null)
				throw new ArgumentNullException(nameof(createdGameInfo), "El GameCreatedDto no puede ser nulo");
			
			var matchRequest = await _context.MatchRequests.FirstOrDefaultAsync(mr => mr.Id == createdGameInfo.MatchRequestId);
			if (matchRequest == null)
				throw new Exception("No se encontro la MatchRequest asociada al juego creado");

			//actualizar la informacion del MatchRequest
			//asigno solo el jugador2 ya que el jugador1 fue asignado al crear la MatchRequest
			matchRequest.MatchedPlayerId ??= createdGameInfo.Player2Id;
			matchRequest.Status = EMatchRequestStatus.Accepted;
			matchRequest.GameId ??= createdGameInfo.GameId;

			_context.MatchRequests.Update(matchRequest);
			await _context.SaveChangesAsync();
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
