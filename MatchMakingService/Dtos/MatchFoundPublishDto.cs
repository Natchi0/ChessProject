using Shared;

namespace MatchMakingService.Dtos
{
	public class MatchFoundPublishDto : IEventDto
	{
		public required int Player1Id { get; set; }
		public required int Player2Id { get; set; }
		public string Event { get; set; } = ERoutingKey.MatchFound; // evento por defecto
	}
}
