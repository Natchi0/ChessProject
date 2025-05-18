using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUtilsLib
{
	public class Board
	{
		private int[] squares = new int[64];

		public Board()
		{
			//Inicializar con cuadrados vacios
			for (int i = 0; i < squares.Length; i++)
			{
				squares[i] = 0;
			}
		}

		public void InitBoard(string fen = null)
		{
			squares = ChessUtils.LoadStartingBoard(fen);
		}

		public int[] GetSquares()
		{
			return squares;
		}

		public void SetSquares(int[] state)
		{
			if (state.Length != 64)
				throw new ArgumentException("Board state must have 64 elements");

			squares = state;
		}

		public void HandlePieceMovement(int actualIndex, int nextIndex)
		{
			squares[nextIndex] = squares[actualIndex];
			squares[actualIndex] = 0;
		}
	}
}
