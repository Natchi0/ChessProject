using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace ChessLogic
{
    public class Board
    {
        private static int[] mailbox = [
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

        private static int[] mailbox64 = [
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
        private static bool[] slide = { false, false, true, true, true, false };
        private static int[][] offset = [
            [ 0, 0, 0, 0, 0, 0, 0, 0 ],
            [ -21, -19, -12, -8, 8, 12, 19, 21 ], //caballo
            [ -11, -9, 9, 11, 0, 0, 0, 0 ], //alfil
            [ -10, -1, 1, 10, 0, 0, 0, 0 ], //torre
            [ -11, -10, -9, -1, 1, 9, 10, 11 ], //reina
            [ -11, -10, -9, -1, 1, 9, 10, 11 ] //rey
        ];

        public static int[] squares = new int[64];

		//evento para mandar mensajes al UI
		public static event Action<string>? OnInfoMessage;

		static Board()
        {
            LoadStartingBoard("rnbqkbnr/pppp1ppp/B7/4p3/4P3/8/PPPP1PPP/RNBQK1NR b KQkq -0 1");
        }

        public static void LoadStartingBoard(string? fen = null)
        {
            if(fen != null)
			{
				LoadPositionFromFen(fen);
				return;
			}
            else
            {
                string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -0 1";
                LoadPositionFromFen(startFen);
            }
        }

        public static void LoadPositionFromFen(string fen)
        {
            //(pawn = "P", knight = "N", bishop = "B", rook = "R", queen = "Q" and king = "K")
            var piecesFromLetter = new Dictionary<char, int>
            {
                { 'P', Piece.White | Piece.Pawn },
                { 'N', Piece.White | Piece.Knight },
                { 'B', Piece.White | Piece.Bishop },
                { 'R', Piece.White | Piece.Rook },
                { 'Q', Piece.White | Piece.Queen },
                { 'K', Piece.White | Piece.King },
                { 'p', Piece.Black | Piece.Pawn },
                { 'n', Piece.Black | Piece.Knight },
                { 'b', Piece.Black | Piece.Bishop },
                { 'r', Piece.Black | Piece.Rook },
                { 'q', Piece.Black | Piece.Queen },
                { 'k', Piece.Black | Piece.King },
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
                        squares[pos] = piecesFromLetter[character];
                        file++;
                    }
                }
            }
        }

        // TODO: Implementar la función para obtener la posición actual en formato FEN
        public static string GetActualFern()
        {
            return "";
        }

        public static void PrintBoard()
        {
            // Unicode para piezas de ajedrez (♔ ♕ ♖ ♗ ♘ ♙ ♚ ♛ ♜ ♝ ♞ ♟)
            Dictionary<int, char> pieceSymbols = new Dictionary<int, char>
            {
                { 0, '.' },       // Vacío
                { 9, '♙' },       // Peón blanco
                { 10, '♘' },       // Caballo blanco
                { 11, '♗' },       // Alfil blanco
                { 12, '♖' },       // Torre blanca
                { 13, '♕' },       // Reina blanca
                { 14, '♔' },       // Rey blanco
                { 1, '♟' },      // Peón negro
                { 2, '♞' },      // Caballo negro
                { 3, '♝' },      // Alfil negro
                { 4, '♜' },      // Torre negra
                { 5, '♛' },      // Reina negra
                { 6, '♚' }       // Rey negro
            };

            Console.WriteLine("    0 1 2 3 4 5 6 7");  // Letras de las columnas
            Console.WriteLine("  +-----------------+");

            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    Console.WriteLine();
                    Console.Write($"{i / 8} | ");
                }

                int piece = squares[i];
                char symbol = pieceSymbols.ContainsKey(piece) ? pieceSymbols[piece] : '.';

                Console.Write($"{piece} ");

            }

            Console.WriteLine();
            Console.WriteLine("  +-----------------+");
        }

        public static void HandlePieceMovement(int IndexActual, int NewIndex)//true si se movió la pieza
        {
            //chequeo que la pieza seleccionada sea del color correspondiente
            int pieceCode = squares[IndexActual];
            int pieceColor = pieceCode & Piece.ColorMask;
            int pieceType = pieceCode & Piece.PieceMask;
			int? PossibleEnPassant = null;
            bool? isOnLastRank = null;
			int colorEnemigo = pieceColor == Piece.White ? Piece.Black : Piece.White;

			//if (pieceColor != GameState.getState())
			//         {
			//             throw new InvalidOperationException($"No es el turno de {(pieceColor == 8 ? "Blanco" : "Negro")}");
			//         }

			//chequeo que la nueva posición sea válida
			if (!ValidMoveCheck(IndexActual, NewIndex))
            {
                return;
            }

			//verifico no haver llegado a los 50 movimientos
			if (GameState.HalfMoves >= 50)
			{
				throw new InvalidOperationException("Se ha alcanzado el límite de 50 movimientos sin capturas o movimientos de peones.");
			}

            //verifico que el movimiento no deje al rey en jaque
            if (CheckChecker(pieceColor, IndexActual, NewIndex, pieceType))
            {
				throw new InvalidOperationException("Movimiento Inválido. Rey en jaque.");
			}


			/*
             * 
             * A PARTIR DE ESTE PUNTO EL MOVIMIENTO ES VALIDO
             * 
             */

			//la posicion es valida, primero verifico en caso de que sea un peon para validar los movimientos en passant
			if (pieceType == Piece.Pawn)
            {
                //verifico que se esté intentando campturar en passant
                if (NewIndex == GameState.EnPassant)
                {
                    //captura en passant
                    squares[NewIndex + (pieceColor == Piece.White ? 8 : -8)] = 0;
                }

                //verifico si está moviendo dos espacios
                if (Math.Abs(NewIndex - IndexActual) == 16)
                {
                    PossibleEnPassant = IndexActual + (pieceColor == Piece.White ? -8 : 8);
                }

                //el peon llego a la ultima fila
                isOnLastRank = (pieceColor == Piece.White && NewIndex >= 0 && NewIndex <= 7) ||
                                     (pieceColor == Piece.Black && NewIndex >= 56 && NewIndex <= 63);
			}
			//setear el en passant
            GameState.EnPassant = PossibleEnPassant;

            //Manejo los enrroques en funcion aparte
            CastlingManager(pieceType, pieceColor, IndexActual, NewIndex);

			//muevo la pieza
			squares[(int)NewIndex] = squares[IndexActual];
            squares[IndexActual] = 0;

			//en caso de capturar una pieza o mover un peon, actualizo los movimientos
            if (squares[NewIndex] != 0 || pieceType == Piece.Pawn)
			{
				GameState.HalfMoves = 0;
			}
			else
			{
				GameState.HalfMoves++;
			}

			if (isOnLastRank == true)
			{
				//cambio el peon por una reina 
				//TODO: cambiar por la pieza que elija el jugador
				squares[NewIndex] = pieceColor | Piece.Queen;
			}

			//verifico el jaque
			//color es el color del rey que puede estar en jaque, si blanco acaba de mover entonces color negro
			if (CheckChecker(pieceColor == Piece.White ? Piece.Black : Piece.White))
            {
                //chequeo si es mate
                if(MateChecker(pieceColor == Piece.White ? Piece.Black : Piece.White))
                {
                    string color = pieceColor == Piece.White ? "Blanco" : "Negro";
					OnInfoMessage?.Invoke($"¡JAQUE MATE! \n {color} gana");
				}

				//si no es mate, aviso que hay jaque
				if (colorEnemigo == Piece.White)
                {
                    if (!GameState.WhiteInCheck)
				        OnInfoMessage?.Invoke($"¡{colorEnemigo} está en Jaque!");

                    GameState.WhiteInCheck = true;
                }
                else
                {
					if (!GameState.BlackInCheck)
						OnInfoMessage?.Invoke($"¡{colorEnemigo} está en Jaque!");

					GameState.BlackInCheck = true;
                }
			}
            else
            {
				if (colorEnemigo == Piece.White)
				{
					GameState.WhiteInCheck = false;
				}
				else
				{
					GameState.BlackInCheck = false;
				}
			}

			GameState.ChangeTurn();

        }

		//se le puede pasar un array de squares para chequear o en su defecto se chequea el array de squares original
		public static int[] GetPosiblePieceMovements(int square, int[]? squaresSource = null, bool withCastle = false)
        {
			squaresSource ??= squares;

			int pieceCode = squaresSource[square];
            int pieceType = pieceCode & Piece.PieceMask;
            int pieceColor = pieceCode & Piece.ColorMask;

            if (pieceType == Piece.Pawn) //los movimientos del peon son muy particular, lo muevo a otra funcion para que quede mas limpio
            {
                return GetPosiblePawnMovements(square, pieceCode, pieceType, pieceColor, squaresSource);
			}

            List<int> posibleMovements = new List<int>();

            int[] moves = offset[pieceType - 1];//resto uno ya que las piezas empiezan en 1

            int valPos64 = mailbox64[square];

            foreach (int move in moves)
            {
                var aux = valPos64 + move;
                var targetPosition = mailbox[aux];

                if (targetPosition == -1)
                    continue;

                if (slide[pieceType - 1])
                {
					while (targetPosition != -1 && (squaresSource[targetPosition] == 0 || ((squaresSource[targetPosition] & Piece.ColorMask) != pieceColor)))
                    {
                        posibleMovements.Add(targetPosition);

						if (squaresSource[targetPosition] != 0) // si hay una pieza en la posición destino me voy despues de agregarla
							break;

						aux = aux + move;
                        targetPosition = mailbox[aux];
                    }

                }
                else
                {
                    if ( squaresSource[targetPosition] == 0 || ((squaresSource[targetPosition] & Piece.ColorMask) != pieceColor) )
                    {
                        posibleMovements.Add(targetPosition);
                    }
                }
            }

            if(pieceType == Piece.King && withCastle)
            {
                List<int> castlingMoves = new List<int>();
                castlingMoves = GetCastlingMoves(pieceColor);

                posibleMovements.AddRange(castlingMoves);
			}

            return posibleMovements.ToArray();
        }

        private static int[] GetPosiblePawnMovements(int square, int pieceCode, int pieceType, int pieceColor, int[] squaresSource)
        {
			List<int> posibleMovements = new List<int>();
			int colorMultiplier = pieceColor == Piece.ColorMask ? -1 : 1; //asigno todos los valores y multiplico segun el color de la pieza

			int oneStep = 10 * colorMultiplier;
			int leftCapture = 9 * colorMultiplier;
			int rightCapture = 11 * colorMultiplier;

			int valPos64 = mailbox64[square];


			//chequeo el movimiento hacia adelante de la primera casilla
			int oneStepIndex = valPos64 + oneStep;
			int oneStepTarget = mailbox[oneStepIndex];
			bool canAdvanceOne = oneStepTarget != -1 && squaresSource[oneStepTarget] == 0;

            if (canAdvanceOne)
            {
                posibleMovements.Add(oneStepTarget);

                //movimiento de 2 casillas si está en la fila inicial
                bool isOnStartRank = (pieceColor == Piece.White && square >= 48 && square <= 55) ||
                                     (pieceColor == Piece.Black && square >= 8 && square <= 15);

                if (isOnStartRank)
                {
					int twoStep = 20 * colorMultiplier;
					int twoStepIndex = valPos64 + twoStep;
					int twoStepTarget = mailbox[twoStepIndex];

					if (twoStepTarget != -1 && squaresSource[twoStepTarget] == 0)
					{
						posibleMovements.Add(twoStepTarget);
					}
				}
			}

			// chequeo las apturas
			foreach (int capture in new[] { leftCapture, rightCapture })
			{
				int captureIndex = valPos64 + capture;
				int captureTarget = mailbox[captureIndex];

				if (captureTarget != -1 && ((squaresSource[captureTarget] != 0 && (squaresSource[captureTarget] & Piece.ColorMask) != pieceColor) || captureTarget == GameState.EnPassant))
				{
					posibleMovements.Add(captureTarget);
				}
			}

			return posibleMovements.ToArray();
		}

		private static HashSet<int> GetAttackedSquares(int pieceColor, int[]? squaresSource = null)
		{
			squaresSource ??= squares;
			HashSet<int> attackedSquares = new HashSet<int>();

			for (int i = 0; i < 64; i++)
			{
				if (squaresSource[i] != 0 && (squaresSource[i] & Piece.ColorMask) == pieceColor)
				{
					int[] moves = GetPosiblePieceMovements(i, squaresSource);
					foreach (int move in moves)
					{
						attackedSquares.Add(move);
					}
				}
			}

			return attackedSquares;
		}

		private static bool IsSquareAttacked(int square, int pieceColor, HashSet<int> attackedSquares)
		{
			return attackedSquares.Contains(square);
		}

		private static List<int> GetCastlingMoves(int pieceColor)
		{
			List<int> castlingMoves = new List<int>();
			int colorEnemigo = pieceColor == Piece.White ? Piece.Black : Piece.White;

			//precomputar los ataques del enemigo
			HashSet<int> enemyAttackedSquares = GetAttackedSquares(colorEnemigo);

			if (pieceColor == Piece.White && !GameState.WhiteInCheck)//la pieza es blanca
			{
				if (GameState.castleWK) //verifico KinkSide
				{
					bool isValid = squares[63] == (Piece.White | Piece.Rook) &&
						squares[61] == 0 && squares[62] == 0 &&
						!enemyAttackedSquares.Contains(61) &&
						!enemyAttackedSquares.Contains(62);

					if (isValid)
						castlingMoves.Add(62);
				}
				if (GameState.castleWQ) //verifico QueenSide
				{
					bool isValid = squares[56] == (Piece.White | Piece.Rook) &&
						squares[57] == 0 && squares[58] == 0 && squares[59] == 0 &&
						!enemyAttackedSquares.Contains(57) &&
						!enemyAttackedSquares.Contains(58) &&
						!enemyAttackedSquares.Contains(59);

					if (isValid) castlingMoves.Add(58);
				}
			}
			else if (pieceColor == Piece.Black && !GameState.BlackInCheck)
			{
				if (GameState.castleBK)
				{
					bool isValid = squares[7] == (Piece.Black | Piece.Rook) &&
						squares[5] == 0 && squares[6] == 0 &&
						!enemyAttackedSquares.Contains(5) &&
						!enemyAttackedSquares.Contains(6);

					if (isValid) castlingMoves.Add(6);
				}
				if (GameState.castleBQ)
				{
					bool isValid = squares[0] == (Piece.Black | Piece.Rook) &&
						squares[1] == 0 && squares[2] == 0 && squares[3] == 0 &&
						!enemyAttackedSquares.Contains(1) &&
						!enemyAttackedSquares.Contains(2) &&
						!enemyAttackedSquares.Contains(3);

					if (isValid) castlingMoves.Add(2);
				}
			}
			return castlingMoves;
		}

		private static bool ValidMoveCheck(int IndexActual, int NewIndex)
        {
            int[] moves = GetPosiblePieceMovements(IndexActual);
			foreach (int move in moves)
			{
				if (move == NewIndex)
				{
					return true;
				}
			}
			return false;
        }

        private static void CastlingManager(int pieceType, int pieceColor, int IndexActual, int NewIndex)
        {
			//Desactivo los enrroques segun corresponda
			if (pieceType == Piece.King || pieceType == Piece.Rook)
			{
				//verifico si se movió el rey o la torre
				if (pieceType == Piece.King)
				{
					if (pieceColor == Piece.White)
					{
                        if(GameState.castleWK && NewIndex == 62)
                        {
                            //enrroque KingSide, muevo la torre
                            squares[63] = 0;
							squares[61] = Piece.White | Piece.Rook;
						}
                        else if (GameState.castleWQ && NewIndex == 58)
						{
                            squares[56] = 0;
                            squares[59] = Piece.White | Piece.Rook;
						}

						GameState.castleWK = false;
						GameState.castleWQ = false;
					}
					else
					{
						if (GameState.castleBK && NewIndex == 6)
						{
							squares[7] = 0;
							squares[5] = Piece.Black | Piece.Rook;
						}
						else if (GameState.castleBQ && NewIndex == 2)
						{
							squares[0] = 0;
							squares[3] = Piece.Black | Piece.Rook;
						}

						GameState.castleBK = false;
						GameState.castleBQ = false;
					}
				}
				else
				{
					if (pieceColor == Piece.White)
					{
						if (IndexActual == 56)
						{
							GameState.castleWQ = false;
						}
						else if (IndexActual == 63)
						{
							GameState.castleWK = false;
						}
					}
					else
					{
						if (IndexActual == 0)
						{
							GameState.castleBQ = false;
						}
						else if (IndexActual == 7)
						{
							GameState.castleBK = false;
						}
					}
				}
			}


		}

        //color es el color del rey a chequear
        private static bool CheckChecker(int color, int? IndexActual = null, int? NewIndex = null, int? pieceType = null)
        {
            int[] squaresAux = [];
			int colorEnemigo = color == Piece.White ? Piece.Black : Piece.White;

			//en caso de que el movimiento no haya sido hecho lo imito en un aux
			if (IndexActual != null && NewIndex != null)
            { 
                squaresAux = (int[])squares.Clone();

			    //imito el movimiento
			    squaresAux[(int)NewIndex] = squaresAux[(int)IndexActual];
			    squaresAux[(int)IndexActual] = 0;

				if (pieceType == Piece.Pawn)
				{
					//verifico que se esté intentando campturar en passant
					if (NewIndex == GameState.EnPassant)
					{
						//captura en passant
						squaresAux[(int)NewIndex + (color == Piece.White ? 8 : -8)] = 0;
					}
				}
			}
            else
            {
                squaresAux = squares;
			}


			//recorrer todos los cuadros
			for (int i = 0; i < 64; i++)
            {
				//si no esta vacio
				if (squaresAux[i] != 0 && (squaresAux[i] & Piece.ColorMask) == colorEnemigo)
                {
                    //obtener los posibles movimientos de la pieza
                    int[] moves = GetPosiblePieceMovements(i, squaresAux, false);

                    //recorrer todos los posibles movimientos
                    foreach (int move in moves)
                    {
                        //si alguno de los movimientos corresponde al rey del color buscado
                        if (squaresAux[move] == (color | Piece.King))
                        {
                            return true;
						}
                    }
                }
            }

            return false;
		}

        private static bool MateChecker(int color)
		{
			//recorrer todos los cuadros
			for (int i = 0; i < 64; i++)
			{
				//si no esta vacio
				if (squares[i] != 0 && (squares[i] & Piece.ColorMask) == color)
				{
					//obtener los posibles movimientos de la pieza
					int[] moves = GetPosiblePieceMovements(i);
					//recorrer todos los posibles movimientos
					foreach (int move in moves)
					{
						if (!CheckChecker(color, i, move))
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}
}
