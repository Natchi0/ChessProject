using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUtilsLib
{
	//enum del tipo de pieza
	public class PieceType
	{
		public const int None = 0;
		public const int Pawn = 1;
		public const int Knight = 2;
		public const int Bishop = 3;
		public const int Rook = 4;
		public const int Queen = 5;
		public const int King = 6;
	}

	//mascara para extraer tipo y color
	public static class PieceMasks
	{
		public const int PieceMask = 7;
		public const int ColorMask = 8;
	}

	//colores
	public class PieceColor
	{
		public const int Black = 0 << 3; // 0
		public const int White = 1 << 3;  // 8
	}

	public class PieceUtils
	{
	}
}
