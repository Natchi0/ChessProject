using Shared;

namespace MatchMakingService.Dtos
{
	public class RequestMatchPublishDto
	{
		public required int PlayerId { get; set; }
		public required string Event { get; set; } = RoutingKey.FindMatch; // evento por defecto
	}
}
