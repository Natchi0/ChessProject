using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
	enum EState
	{
		Menu,
		White,
		Black,
	}

	public class GameState
	{
		private static EState State = EState.White;
		public static int Turn = 0;
		private static string ActualFern = string.Empty;
		public static int? EnPassant { get; set; } = null;
		public static int HalfMoves = 0;
		public static bool castleWK = true;
		public static bool castleWQ = true;
		public static bool castleBK = true;
		public static bool castleBQ = true;
		public static bool WhiteInCheck = false;
		public static bool BlackInCheck = false;

		public static void InitGame()
		{
			State = EState.White;
			Turn = 0;
			Board.LoadStartingBoard();
		}

		public static void ChangeTurn()
		{
			if (State == EState.White)
			{
				State = EState.Black;
			}
			else
			{
				State = EState.White;
			}
			Turn++;

			ActualFern = Board.GetActualFern();
		}

		public static int getState()
		{
			if(State == EState.White)
			{
				return 8;
			}
			else if (State == EState.Black)
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
