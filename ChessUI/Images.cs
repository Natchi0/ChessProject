using ChessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ChessUI
{
	public static class Images
	{
		private static readonly Dictionary<int, ImageSource> WhiteSources = new()
		{
			{ 9, LoadImage("Assets/PawnW.png") },
			{ 10, LoadImage("Assets/KnightW.png") },
			{ 11, LoadImage("Assets/BishopW.png") },
			{ 12, LoadImage("Assets/RookW.png") },
			{ 13, LoadImage("Assets/QueenW.png") },
			{ 14, LoadImage("Assets/KingW.png") }
		};

		private static readonly Dictionary<int, ImageSource> BlackSources = new()
		{
			{ 1, LoadImage("Assets/PawnB.png") },
			{ 2, LoadImage("Assets/KnightB.png") },
			{ 3, LoadImage("Assets/BishopB.png") },
			{ 4, LoadImage("Assets/RookB.png") },
			{ 5, LoadImage("Assets/QueenB.png") },
			{ 6, LoadImage("Assets/KingB.png") }
		};

		private static ImageSource LoadImage(string filepath)
		{
			return new BitmapImage(new Uri(filepath, UriKind.Relative));
		}

		public static ImageSource GetImage(int PieceCode)
		{
			if (PieceCode == 0) { return null; }

			bool isWhite = (PieceCode & Piece.White) == Piece.White;

			var image = isWhite ? WhiteSources[PieceCode] : BlackSources[PieceCode];
			return image;
		}
	}
}
