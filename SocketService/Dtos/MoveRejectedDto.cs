using Shared;

namespace SocketService.Dtos
{
	public class MoveRejectedDto : IEventDto
	{
		public int PlayerId { get; set; }
		public int GameId { get; set; }
		public string Message { get; set; } = "";
		public string Event { get; set; } = RoutingKey.MoveRejected;
	}
}
