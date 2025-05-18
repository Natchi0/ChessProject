using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUtilsLib
{
	public class ChessUtils
	{
		public static int[] mailbox = [
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1,  0,  1,  2,  3,  4,  5,  6,  7, -1,
			-1,  8,  9, 10, 11, 12, 13, 14, 15, -1,
			-1, 16, 17, 18, 19, 20, 21, 22, 23, -1,
			-1, 24, 25, 26, 27, 28, 29, 30, 31, -1,
			-1, 32, 33, 34, 35, 36, 37, 38, 39, -1,
			-1, 40, 41, 42, 43, 44, 45, 46, 47, -1,
			-1, 48, 49, 50, 51, 52, 53, 54, 55, -1,
			-1, 56, 57, 58, 59, 60, 61, 62, 63, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1
		];

		public static int[] mailbox64 = [
			21, 22, 23, 24, 25, 26, 27, 28,
			31, 32, 33, 34, 35, 36, 37, 38,
			41, 42, 43, 44, 45, 46, 47, 48,
			51, 52, 53, 54, 55, 56, 57, 58,
			61, 62, 63, 64, 65, 66, 67, 68,
			71, 72, 73, 74, 75, 76, 77, 78,
			81, 82, 83, 84, 85, 86, 87, 88,
			91, 92, 93, 94, 95, 96, 97, 98
		];

		//                                     caballo alfil torre reina rey  
		public static bool[] slide = { false, false, true, true, true, false };
		public static int[][] offset = [
			[ 0, 0, 0, 0, 0, 0, 0, 0 ],
			[ -21, -19, -12, -8, 8, 12, 19, 21 ], //caballo
            [ -11, -9, 9, 11, 0, 0, 0, 0 ], //alfil
            [ -10, -1, 1, 10, 0, 0, 0, 0 ], //torre
            [ -11, -10, -9, -1, 1, 9, 10, 11 ], //reina
            [ -11, -10, -9, -1, 1, 9, 10, 11 ] //rey
        ];

		private static int[] LoadPositionFromFen(string fen)
		{
			int[] board = new int[64];

			//(pawn = "P", knight = "N", bishop = "B", rook = "R", queen = "Q" and king = "K")
			var piecesFromLetter = new Dictionary<char, int>
			{
				{ 'P', PieceColor.White | PieceType.Pawn },
				{ 'N', PieceColor.White | PieceType.Knight },
				{ 'B', PieceColor.White | PieceType.Bishop },
				{ 'R', PieceColor.White | PieceType.Rook },
				{ 'Q', PieceColor.White | PieceType.Queen },
				{ 'K', PieceColor.White | PieceType.King },
				{ 'p', PieceColor.Black | PieceType.Pawn },
				{ 'n', PieceColor.Black | PieceType.Knight },
				{ 'b', PieceColor.Black | PieceType.Bishop },
				{ 'r', PieceColor.Black | PieceType.Rook },
				{ 'q', PieceColor.Black | PieceType.Queen },
				{ 'k', PieceColor.Black | PieceType.King },
			};

			string boardString = fen.Split(' ')[0];
			int file = 0, rank = 0;

			foreach (char character in boardString)
			{
				if (character == '/')
				{
					file = 0;
					rank++;
					continue;
				}
				else
				{
					if (Char.IsDigit(character))
					{
						file += (int)Char.GetNumericValue(character);
						continue;
					}
					else
					{
						var pos = rank * 8 + file;
						board[pos] = piecesFromLetter[character];
						file++;
					}
				}
			}

			return board;
		}

		public static int[] LoadStartingBoard(string? fen = null)
		{
			try
			{
				if (fen != null)
				{
					return LoadPositionFromFen(fen);
				}
				else
				{
					string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -0 1";
					return LoadPositionFromFen(startFen);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading board: {ex.Message}");
				return new int[64];
			}
		}
	}
}
