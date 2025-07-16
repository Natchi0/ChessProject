using ChessUtilsLib;
using DAL.Models;
using Shared;
using System.Diagnostics.CodeAnalysis;

namespace GameServer.Dtos
{
	public class GameUpdatedDto : IEventDto
	{
		public required int Id { get; set; }
		public required int Player1Id { get; set; }
		public required int Player2Id { get; set; }
		public required EState State { get; set; }
		public required EEndGameState? EndGameState { get; set; }
		public required int[] BoardState { get; set; } = new int[64];
		public required int? EnPassant { get; set; }
		public required int? EnPassantTargetColor { get; set; }

		public required int HalfMoves { get; set; } = 0;

		public required bool CastleWK { get; set; }
		public required bool CastleWQ { get; set; }
		public required bool CastleBK { get; set; }
		public required bool CastleBQ { get; set; }

		public required bool WhiteInCheck { get; set; }
		public required bool BlackInCheck { get; set; }
		public string Event { get; set; } = RoutingKey.GameUpdated;

		[SetsRequiredMembers] //esto sirve para que el compilador sepa que las propiedades required estan siendo inicializadas
		public GameUpdatedDto(Game game)
		{
			Id = game.Id;
			Player1Id = (int)game.PlayerId1!;
			Player2Id = (int)game.PlayerId2!;
			State = game.State;
			EndGameState = game.EndGameState;
			BoardState = game.BoardState;
			EnPassant = game.EnPassant;
			EnPassantTargetColor = game.EnPassantTargetColor;
			HalfMoves = game.HalfMoves;
			CastleWK = game.CastleWK;
			CastleWQ = game.CastleWQ;
			CastleBK = game.CastleBK;
			CastleBQ = game.CastleBQ;
			WhiteInCheck = game.WhiteInCheck;
			BlackInCheck = game.BlackInCheck;
		}
	}
}
