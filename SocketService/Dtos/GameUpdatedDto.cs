using ChessUtilsLib;
using Shared;

namespace SocketService.Dtos
{
	public class GameUpdatedDto : IEventDto
	{
		public int Id { get; set; }
		public int Player1Id { get; set; }
		public int Player2Id { get; set; }
		public EState State { get; set; }
		public EEndGameState? EndGameState { get; set; }
		public int[] BoardState { get; set; } = new int[64];
		public int? EnPassant { get; set; }
		public int? EnPassantTargetColor { get; set; }

		public int HalfMoves { get; set; } = 0;

		public bool CastleWK { get; set; }
		public bool CastleWQ { get; set; }
		public bool CastleBK { get; set; }
		public bool CastleBQ { get; set; }

		public bool WhiteInCheck { get; set; }
		public bool BlackInCheck { get; set; }
		public string Event { get; set; } = RoutingKey.GameUpdated;
	}
}
