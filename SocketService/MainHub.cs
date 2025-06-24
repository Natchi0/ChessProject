using MatchMakingService;
using Microsoft.AspNetCore.SignalR;
using SocketService.Dtos;
using SocketService.MessageServices;

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
					ConnectionId = connectionId,
					Event = "find.match"
				};

				await _messageBusClient.PublishNewMatchRequest(requestMatchPublishDto);

				//await _matchService.RequestMatch(playerId, connectionId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"EXCEPTION {ex.Message}");
				await Clients.Caller.SendAsync("MatchRejected", ex.Message);
			}
		}
	}
}
