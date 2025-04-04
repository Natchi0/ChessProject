using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{

	/*
	
	Interetar los codigos mediante bits
	primeros 3 bits: tipo de pieza
	ultimo bit: color de la pieza

	Casilla vacía: 0000 - decimal 0
	Peon blanco: 0001 - decimal 1
	Peon Negro: 1001 - decimal 9
	Caballo blanco:	0010 - decimal 2
	Caballo negro: 1010	- decimal 10

	*/
	enum EPieceCode //leer como bits
	{
		epc_empty = 0,     // Casilla vacía
		epc_wpawn = 1,     // Peón blanco
		epc_wknight = 2,     // Caballo blanco
		epc_wbishop = 3,     // Alfil blanco
		epc_wrook = 4,     // Torre blanca
		epc_wqueen = 5,     // Reina blanca
		epc_wking = 6,     // Rey blanco

		epc_blacky = 8,     // Bit de color negro (1 << 3)
		epc_bpawn = 9,     // Peón negro
		epc_bknight = 10,    // Caballo negro
		epc_bbishop = 11,    // Alfil negro
		epc_brook = 12,    // Torre negra
		epc_bqueen = 13,    // Reina negra
		epc_bking = 14     // Rey negro
	};


	/*
	
	Rey Blanco Piece.White | Piece.King -> 1110 (14 en decimal)
	Peon Negro Piece.Black | Piece.Pawn -> 0001 ( 1 en decimal)
	 
	*/
	public class Piece
	{
		public const int None = 0;
		public const int Pawn = 1;
		public const int Knight = 2;
		public const int Bishop = 3;
		public const int Rook = 4;
		public const int Queen = 5;
		public const int King = 6;

		public const int PieceMask = 7;
		public const int ColorMask = 8;

		public const int White = 1<<3;
		public const int Black = 0<<3;

	}
}
