using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUtilsLib
{
	public class MoveContext
	{
		public int From { get; set; }
		public int To { get; set; }
		public int PieceColor { get; set; }
		public int PieceType { get; set; }
		public int? EnPassant { get; set; }
		public int? EnPassantTargetColor { get; set; }
		public bool CastleWK { get; set; }
		public bool CastleWQ { get; set; }
		public bool CastleBK { get; set; }
		public bool CastleBQ { get; set; }
		public int HalfMove { get; set; }
		public int Turns { get; set; }
	}

	public class MoveResult
	{
		public int[] NewBoardState { get; set; } = new int[0];
		public int? NewEnPassant { get; set; }
		public int? NewEnPassantTargetColor { get; set; }
		public bool CastleWK { get; set; }
		public bool CastleWQ { get; set; }
		public bool CastleBK { get; set; }
		public bool CastleBQ { get; set; }
		public int NewHalfMove { get; set; }
		public int NewTurns { get; set; }
	}
}
