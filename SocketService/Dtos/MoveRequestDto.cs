using Shared;

namespace SocketService.Dtos
{
	public class MoveRequestDto : IEventDto
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
		public string Event { get; set; } = RoutingKey.Move;
	}
}
