using Shared;

namespace SocketService.Dtos
{
	public class MatchFoundPublishDto
	{
		public required int Player1Id { get; set; }
		public required int Player2Id { get; set; }
		public required int GameId { get; set; }
		public required int[] BoardState { get; set; } = new int[64]; //por defecto tablero vacio
		public required string Event { get; set; } = ERoutingKey.MatchFound; // evento por defecto
	}
}
