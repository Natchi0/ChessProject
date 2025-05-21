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

		//uso tuplas para devolver toda la info deconstruida
		public (int pieceCode, int pieceColor, int pieceType) GetPieceAt(int index)
		{
			if (index < 0 || index >= squares.Length)
				throw new Exception("Index fuera de los limites");

			int pieceCode = squares[index];
			int color = pieceCode & PieceMasks.ColorMask;
			int type = pieceCode & PieceMasks.TypeMask;
			return (pieceCode, color, type);
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
				throw new ArgumentException("El arreglo debe tener exatamente 64 lugares");

			squares = state;
		}

		public MoveResult MovePiece(MoveContext context)
		{
			MoveResult result = new MoveResult();

			int pieceColor = context.PieceColor;
			int pieceType = context.PieceType;
			int actualIndex = context.From;
			int newIndex = context.To;

			int? possibleEnPassant = null;
			bool? isOnLastRank = null;

			//primero verifico en caso de que sea un peon validar los movimientos en passant
			if (pieceType == PieceType.Pawn)
			{
				//vaerifico que se este intentando capturar en passant
				if (pieceColor != context.EnPassantTargetColor && newIndex == context.EnPassant)
				{
					//captura en passant
					squares[newIndex + (pieceColor == PieceColor.White ? 8 : -8)] = 0;
				}

				//verifico si esta moviendo dos casillas
				if(Math.Abs(actualIndex - newIndex) == 16)
				{
					possibleEnPassant = newIndex + (pieceColor == PieceColor.White ? 8 : -8);
				}

				//el peon llega a la ultima fila
				isOnLastRank = (pieceColor == PieceColor.White && newIndex >= 0 && newIndex <= 7) ||
							   (pieceColor == PieceColor.Black && newIndex >= 56 && newIndex <= 63);
			}
			//setear el enpassant
			result.NewEnPassant = possibleEnPassant;
			result.NewEnPassantTargetColor = pieceColor;

			//TODO: manejo los enroques aparte, en el Game

			//en caso de capturar una pieza o mover un peon, actualizo los movimientos
			if (squares[newIndex] != 0 || pieceType == PieceType.Pawn)
			{
				result.NewHalfMove = 0;
			}
			else
			{
				result.NewHalfMove = context.HalfMove + 1;
			}

			//actualizar los enrroques
			//Esta funcion actualiza los valores del enroque directamente en context, cuando regresa de la funcion ya tiene los nuevos valores
			CastlingManager(pieceType, pieceColor, actualIndex, newIndex, context);

			//muevo la pieza
			squares[newIndex] = squares[actualIndex];
			squares[actualIndex] = 0;

			if (isOnLastRank == true)
			{
				//promocion de peon
				//TODO: hacer que el usuario elija la pieza a la que se va a promocionar
				squares[newIndex] = pieceColor | PieceType.Queen;
			}

			//guardo los ultimos valores del result
			result.NewBoardState = squares;
			result.CastleWK = context.CastleWK;
			result.CastleWQ = context.CastleWQ;
			result.CastleBK = context.CastleBK;
			result.CastleBQ = context.CastleBQ;
			result.NewTurns = context.Turns + 1;

			return result;
		}

		private void CastlingManager(int type, int color, int actualIndex, int newIndex, MoveContext context)
		{
			//si no es el rey o torre me voy
			if (type != PieceType.King && type != PieceType.Rook)
				return;

			//Manejar el enroque
			if (type == PieceType.King)
			{
				if (color == PieceColor.White)
				{	
					//cheuqeo blanco
					if (newIndex == 62 && context.CastleWK)
					{
						//enroque corto blanco, muevo torre
						squares[63] = 0;
						squares[61] = PieceColor.White | PieceType.Rook;
					}
					else if (newIndex == 58 && context.CastleWQ)
					{
						// Enroque largo
						squares[56] = 0;
						squares[59] = PieceColor.White | PieceType.Rook;
					}
					//desactivo el enroque
					context.CastleWK = false;
					context.CastleWQ = false;
				}
				else
				{
					//chequeo negro
					if (newIndex == 6 && context.CastleBK)
					{
						squares[7] = 0;
						squares[5] = PieceColor.Black | PieceType.Rook;
					}
					else if (newIndex == 2 && context.CastleBQ)
					{
						squares[0] = 0;
						squares[3] = PieceColor.Black | PieceType.Rook;
					}
					//desactivo el enroque
					context.CastleBK = false;
					context.CastleBQ = false;
				}
			}
			else//es la torre
			{
				if (color == PieceColor.White)
				{
					if(actualIndex == 56)
						context.CastleWQ = false;
					else if (actualIndex == 63)
						context.CastleWK = false;
				}
				else
				{
					if (actualIndex == 0)
						context.CastleBQ = false;
					else if (actualIndex == 7)
						context.CastleBK = false;
				}
			}

			//como contexto es una clase, se pasa por referencia por lo que ya está actualizado
		}

	}
}
