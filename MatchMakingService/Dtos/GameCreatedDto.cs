using ChessUtilsLib;
using Shared;

namespace MatchMakingService.Dtos
{
	public class GameCreatedDto : IEventDto
	{
		public required int GameId { get; set; }
		public required int Player1Id { get; set; }
		public required int Player2Id { get; set; }
		public int[] BoardState { get; set; } = new int[64]; // por defecto tablero vacio
		public EState State { get; set; }
		public required int MatchRequestId { get; set; }
		public string Event { get; set; } = ERoutingKey.GameCreated; // evento por defecto
	}
}
