using ChessUtilsLib;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
	public class Game
	{
		public int Id { get; set; }

		public int? PlayerId1 { get; set; }

		public int? PlayerId2 { get; set; }

		[ForeignKey("PlayerId1")]
		public Player? Player1 { get; set; }

		[ForeignKey("PlayerId2")]
		public Player? Player2 { get; set; }

		public EState State { get; set; }

		public EEndGameState? EndGameState { get; set; } = null;

		public DateTime StartedAt { get; set; }

		public int[] BoardState { get; set; } = new int[64];

		[NotMapped]
		public Board Board { get; set; } = new Board();

		public int? EnPassant { get; set; }

		public int? EnPassantTargetColor { get; set; }

		public int HalfMoves { get; set; } = 0;

		public bool CastleWK { get; set; } = true;
		public bool CastleWQ { get; set; } = true;
		public bool CastleBK { get; set; } = true;
		public bool CastleBQ { get; set; } = true;

		public bool WhiteInCheck { get; set; } = false;
		public bool BlackInCheck { get; set; } = false;

		public void InitGame()
		{
			State = EState.WhiteTurn;
			HalfMoves = 0;
			Board.InitBoard();
			BoardState = Board.GetSquares();
		}

		public void ChangeTurn()
		{
			//Turn++; //esto no existe en esta version pero por si acaso

			State = State == EState.WhiteTurn ? EState.BlackTurn : EState.WhiteTurn;

			//ActualFern = Board.GetActualFern(); //TODO: ver como/si hacer esto
		}

		public int getState()
		{
			if (State == EState.WhiteTurn)
			{
				return 8;
			}
			else if (State == EState.BlackTurn)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
	}
}
