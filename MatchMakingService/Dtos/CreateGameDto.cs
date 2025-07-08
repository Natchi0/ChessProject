using Shared;

namespace MatchMakingService.Dtos
{
	public class CreateGameDto
	{
		public required int Player1Id { get; set; }
		public required int Player2Id { get; set; }
		public string Event { get; set; } = ERoutingKey.CreateGame; // evento por defecto
	}
}
