using MatchMakingService;
using Microsoft.AspNetCore.SignalR;
using Shared;
using SocketService.Dtos;
using SocketService.MessageServices;
using System.Text.Json;

namespace SocketService
{
	public class MainHub : Hub
	{
		private readonly IMessageBusClient _messageBusClient;

		public MainHub(IMessageBusClient messageBusClient)
		{
			_messageBusClient = messageBusClient;
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			ConnectionMapping.RemoveByConnection(Context.ConnectionId);
			return base.OnDisconnectedAsync(exception);
		}

		public async Task RequestMatch(int playerId)
		{
			Console.WriteLine($"RequestMatch: Player - {Context.ConnectionId} - PlayerId {playerId}");
			var connectionId = Context.ConnectionId;
			ConnectionMapping.Add(playerId, connectionId);

			try
			{
				//mandar un mensaje mediante rabbitMq al MatchMakingService para que procese la solicitud
				RequestMatchPublishDto requestMatchPublishDto = new RequestMatchPublishDto
				{
					PlayerId = playerId,
					Event = RoutingKey.FindMatch
				};

				await _messageBusClient.PublishNewMatchRequest(requestMatchPublishDto);

				//await _matchService.RequestMatch(playerId, connectionId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"EXCEPTION {ex.Message}");
				await Clients.Caller.SendAsync(SocketEvent.MatchRejected, ex.Message);
			}
		}

		public async Task MakeMove(MoveRequestDto move)
		{
			Console.WriteLine($"MakeMove: Player - {move.PlayerId}");
			Console.WriteLine($"DEBUG moveDto: {JsonSerializer.Serialize(move)}");
			try
			{
				await _messageBusClient.PublishEventAsync(move);
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Excepcion {ex.Message}");
				await Clients.Caller.SendAsync(SocketEvent.MoveRejected, "Error del socket");
			}
		}
	}
}
