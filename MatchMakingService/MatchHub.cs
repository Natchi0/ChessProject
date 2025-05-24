using MatchMakingService.Services;
using Microsoft.AspNetCore.SignalR;

namespace MatchMakingService
{
	public class MatchHub : Hub
	{
		readonly MatchService _matchService;
		public MatchHub(MatchService matchService)
		{
			_matchService = matchService;
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
				await _matchService.RequestMatch(playerId, connectionId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"EXCEPTION {ex.Message}");
				await Clients.Caller.SendAsync("MatchRejected", ex.Message);
			}
		}

	}
}
