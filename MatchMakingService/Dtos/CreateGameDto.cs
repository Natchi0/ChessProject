using Shared;

namespace MatchMakingService.Dtos
{
	public class CreateGameDto : IEventDto
	{
		public required int Player1Id { get; set; }
		public required int Player2Id { get; set; }
		public int MatchRequestId { get; set; }
		public string Event { get; set; } = RoutingKey.CreateGame; // evento por defecto
	}
}
