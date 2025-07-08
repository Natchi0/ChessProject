using Shared;

namespace GameServer.Dtos
{
	public class CreateGameDto
	{
		public required int Player1Id { get; set; }
		public required int Player2Id { get; set; }
		public int MatchRequestId { get; set; }
		public required string Event { get; set; } = ERoutingKey.CreateGame; 
	}
}
