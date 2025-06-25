using Shared;

namespace MatchMakingService.Dtos
{
	public class RequestMatchPublishDto
	{
		public required int PlayerId { get; set; }
		public required string ConnectionId { get; set; }
		public required string Event { get; set; } = ERoutingKey.FindMatch; // evento por defecto
	}
}
