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
		public int Turns { get; set; } = 0; //TODO: ver si es necesario, en esta version no se usa

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
			Turns = 0;
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

		public void HandlePieceMovement(int actualIndex, int newIndex, int playerColor)
		{
			//chequear que la pieza seleccionada sea del color correspondiente
			var (pieceCode, pieceColor, pieceType) = Board.GetPieceAt(actualIndex);
			int enemyColor = pieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

			if (playerColor != getState())
			{
				throw new Exception("No es tu turno");
			}

			if(playerColor != pieceColor)
			{
				throw new Exception("No puedes mover una pieza que no es tuya");
			}

			//chequear que la nueva posicion sea valida
			bool isValid = IsMoveValid(actualIndex, newIndex, pieceType);
			if (!isValid)
			{
				throw new Exception("El movimiento es invalido");
			}

			//verifico no haber llegado a los 50 movimientos
			if (HalfMoves >= 50)
			{
				throw new Exception("Se ha alcanzado el limite de 50 movimientos sin capturas o movimientos de peon");
			}

			//verifico que el movimiento no deje al rey en jaque
			if (CheckChecker(pieceColor, actualIndex, newIndex, pieceType))
			{
				throw new Exception("Movimiento invalido, el rey quedaria en jaque");
			}

			/*
				A PARTIR DE ESTE PUNTO EL MOVIMIENTO ES VALIDO
			 */

			MoveContext moveContext = new MoveContext
			{
				From = actualIndex,
				To = newIndex,
				PieceColor = pieceColor,
				PieceType = pieceType,
				EnPassant = this.EnPassant,
				EnPassantTargetColor = EnPassantTargetColor,
				CastleWK = this.CastleWK,
				CastleWQ = this.CastleWQ,
				CastleBK = this.CastleBK,
				CastleBQ = this.CastleBQ,
				HalfMove = this.HalfMoves,
				Turns = this.Turns
			};

			try
			{

				MoveResult result = Board.MovePiece(moveContext);
				//actualizo el estado del juego
				BoardState = result.NewBoardState;
				EnPassant = result.NewEnPassant;
				EnPassantTargetColor = result.NewEnPassantTargetColor;
				CastleWK = result.CastleWK;
				CastleWQ = result.CastleWQ;
				CastleBK = result.CastleBK;
				CastleBQ = result.CastleBQ;
				HalfMoves = result.NewHalfMove;
				Turns = result.NewTurns;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al mover la pieza en Board", ex);
			}

			//verifico el jaque
			//color es el color del rey que puede estar en jaque, si blanco acaba de mover entonces color negro
			if (CheckChecker(enemyColor))
			{
				//chequeo si es mate
				if (MateChecker(enemyColor))
				{
					//TODO:manejar como mostrar el jaquemate
					Console.WriteLine("JAQUE MATE");
				}

				//si no es jaque mate, actualizo y aviso
				if (enemyColor == PieceColor.White)
				{
					if (!WhiteInCheck)
						Console.WriteLine("JAQUE BLANCO");
					WhiteInCheck = true;
				}
				else if (enemyColor == PieceColor.Black)
				{
					if (!BlackInCheck)
						Console.WriteLine("JAQUE NEGRO");
					BlackInCheck = true;
				}
			}
			else
			{
				if (pieceColor == PieceColor.White)
					WhiteInCheck = false;
				else
					BlackInCheck = false;
			}

			//cambio de turno
			ChangeTurn();
		}

		public bool IsMoveValid(int actualIndex, int newIndex, int? pieceType = null)
		{
			List<int> moves = GetPossiblePieceMovements(actualIndex, null, true);

			foreach (int move in moves)
			{
				if (move == newIndex)
					return true;
			}
			return false;
		}

		private List<int> GetPossiblePieceMovements(int square, int[]? squaresSource = null, bool withCastle = false)
		{
			//si squareSource es null, se usa el array de squares original
			squaresSource ??= Board.GetSquares();

			var (pieceCode, pieceColor, pieceType) = Board.GetPieceAt(square);

			if (pieceType == PieceType.Pawn)
			{
				//los movimientos del peon son muy particulares, lo muevo a una funcion aparte
				return GetPossiblePawnMovements(square, pieceColor, squaresSource);
			}

			// Reemplazar la línea problemática con una inicialización válida para la lista
			List<int> possibleMoves = new List<int>();
			int[] moves = ChessUtils.offset[pieceType - 1]; //resto 1 porque el array de PieceType empieza en 1, 0 es empty

			int valPos64 = ChessUtils.mailbox64[square];

			foreach (int move in moves)
			{
				int aux = valPos64 + move;
				int targetPosition = ChessUtils.mailbox[aux];

				if (targetPosition == -1)
					continue; //posicion fuera del tablero

				if (ChessUtils.slide[pieceType - 1])
				{
					while (targetPosition != -1 && (squaresSource[targetPosition] == 0 || (squaresSource[targetPosition] & PieceMasks.ColorMask) != pieceColor))
					{
						possibleMoves.Add(targetPosition);

						if (squaresSource[targetPosition] != 0)
							break; //si hay una pieza, no puedo seguir moviendo, me voy

						aux += move;
						targetPosition = ChessUtils.mailbox[aux];
					}
				}
				else
				{
					if (squaresSource[targetPosition] == 0 || (squaresSource[targetPosition] & PieceMasks.ColorMask) != pieceColor)
					{
						possibleMoves.Add(targetPosition);
					}
				}
			}

			if (pieceType == PieceType.King && withCastle)
			{
				List<int> castlingMoves = GetCastlingMoves(pieceColor);

				possibleMoves.AddRange(castlingMoves);
			}

			return possibleMoves;
		}

		private List<int> GetPossiblePawnMovements(int square, int pieceColor, int[] squaresSource)
		{
			List<int> possibleMoves = new List<int>();
			int colorMultiplier = pieceColor == PieceColor.White ? -1 : 1; //asigno todos los valores y multiplico segun el color de la pieza

			int oneStep = 10 * colorMultiplier;
			int leftCapture = 9 * colorMultiplier;
			int rightCapture = 11 * colorMultiplier;

			int valPos64 = ChessUtils.mailbox64[square];

			int oneStepIndex = valPos64 + oneStep;
			int oneStepTarget = ChessUtils.mailbox[oneStepIndex];
			bool canAdvanceOne = oneStepTarget != -1 && squaresSource[oneStepTarget] == 0;

			if (canAdvanceOne)
			{
				possibleMoves.Add(oneStepTarget);

				//movimiento de 2 casillas si esta en fila inicial
				bool isOnStartRank = (pieceColor == PieceColor.White && square >= 48 && square <= 55) || (pieceColor == PieceColor.Black && square >= 8 && square <= 15);

				if (isOnStartRank)
				{
					int twoStep = 20 * colorMultiplier;
					int twoStepIndex = valPos64 + twoStep;
					int twoStepTarget = ChessUtils.mailbox[twoStepIndex];

					if (twoStepTarget != -1 && squaresSource[twoStepTarget] == 0)
					{
						possibleMoves.Add(twoStepTarget);
					}
				}
			}

			//chequeo las capturas
			foreach (int capture in new[] { leftCapture, rightCapture })
			{
				int captureIndex = valPos64 + capture;
				int captureTarget = ChessUtils.mailbox[captureIndex];

				if (captureTarget != -1 && ((squaresSource[captureTarget] != 0 && (squaresSource[captureTarget] & PieceMasks.ColorMask) != pieceColor) || (pieceColor != EnPassantTargetColor && captureTarget == EnPassant)))
				{
					possibleMoves.Add(captureTarget);
				}
			}

			return possibleMoves;
		}

		private List<int> GetCastlingMoves(int pieceColor)
		{
			int[] squares = Board.GetSquares();

			List<int> castlingMoves = new List<int>();
			int enemyColor = pieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

			//precomputar los ataques enemigos
			List<int> enemyAttackedSquares = GetAttackedSquares(enemyColor);

			//la pieza es blanca
			if (pieceColor == PieceColor.White && !WhiteInCheck)
			{
				//verificar enroque corto
				if (CastleWK && squares[61] == 0 && squares[62] == 0 && squares[63] == (PieceColor.White | PieceType.Rook) && !enemyAttackedSquares.Contains(61) && !enemyAttackedSquares.Contains(62))
					castlingMoves.Add(62);
				//verificar enroque largo
				if (CastleWQ && squares[56] == (PieceColor.White | PieceType.Rook) && squares[57] == 0 && squares[58] == 0 && squares[59] == 0 && !enemyAttackedSquares.Contains(57) && !enemyAttackedSquares.Contains(58) && !enemyAttackedSquares.Contains(59))
					castlingMoves.Add(58);
			}
			else if (pieceColor == PieceColor.Black && !BlackInCheck)
			{
				//verificar enroque corto
				if (CastleBK && squares[5] == 0 && squares[6] == 0 && squares[7] == (PieceColor.Black | PieceType.Rook) && !enemyAttackedSquares.Contains(5) && !enemyAttackedSquares.Contains(6))
					castlingMoves.Add(6);
				//verificar enroque largo
				if (CastleBQ && squares[0] == (PieceColor.Black | PieceType.Rook) && squares[1] == 0 && squares[2] == 0 && squares[3] == 0 && !enemyAttackedSquares.Contains(1) && !enemyAttackedSquares.Contains(2) && !enemyAttackedSquares.Contains(3))
					castlingMoves.Add(2);
			}

			return castlingMoves;
		}

		private List<int> GetAttackedSquares(int pieceColor, int[]? squaresSource = null)
		{
			squaresSource ??= Board.GetSquares();
			List<int> attackedSquares = new List<int>();

			for (int i = 0; i < 64; i++)
			{
				if (squaresSource[i] != 0 && (squaresSource[i] & PieceMasks.ColorMask) == pieceColor)
				{
					List<int> moves = GetPossiblePieceMovements(i, squaresSource);
					attackedSquares.AddRange(moves);
				}
			}

			return attackedSquares;
		}

		//color es el color del rey a chequear
		private bool CheckChecker(int color, int? actualIndex = null, int? newIndex = null, int? pieceType = null)
		{
			int[] squares = Board.GetSquares();

			int[] squaresAux = Array.Empty<int>();
			int enemyColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;

			//en caso de que el movimiento no haya sido hecho lo imito en un aux
			if (actualIndex != null && newIndex != null)
			{
				squaresAux = (int[])squares.Clone();

				//imito el movimiento
				squaresAux[(int)newIndex] = squaresAux[(int)actualIndex];
				squaresAux[(int)actualIndex] = 0;

				if (pieceType == PieceType.Pawn)
				{
					//verifico que se esté intentando capturar en passant
					if (newIndex == EnPassant)
					{
						//captura en passant
						squaresAux[(int)newIndex + (color == PieceColor.White ? 8 : -8)] = 0;
					}
				}
			}
			else
			{
				squaresAux = Board.GetSquares();
			}

			//recorrer todos los cuadros
			for (int i = 0; i < 64; i++)
			{
				//si no esta vacio
				if (squaresAux[i] != 0 && (squaresAux[i] & PieceMasks.ColorMask) == enemyColor)
				{
					//obtener los posibles movimientos de la pieza
					List<int> moves = GetPossiblePieceMovements(i, squaresAux);

					//recorrer todos los posibles movimientos
					foreach (int move in moves)
					{
						//si alguno de los movimientos corresponde al resy del color buscado
						if (squaresAux[move] == (color | PieceType.King))
						{
							return true; //el rey está en jaque
						}
					}
				}
			}
			return false;
		}

		private bool MateChecker(int color)
		{
			//recorrer los cuadros
			for (int i = 0; i < 64; i++)
			{
				//si no esta vacio
				if (BoardState[i] != 0 && (BoardState[i] & PieceMasks.ColorMask) == color)
				{
					//obtener los posibles movimientos de la pieza
					List<int> moves = GetPossiblePieceMovements(i);
					//recorrer todos los posibles movimientos
					foreach (var move in moves)
					{
						if(!CheckChecker(color, i, move))
						{
							//el movimiento deja al rey en jaque mate
							return false;
						}
					}
				}
			}
			return true;
		}
	}

}
