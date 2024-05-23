using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine
{
    static internal class Evaluate
    {

        public static readonly Dictionary<Piece, int> pieceScore = new Dictionary<Piece, int>()
        {
            { Piece.PAWN, 100 },
            { Piece.KNIGHT, 300 },
            { Piece.BISHOP, 300 },
            { Piece.ROOK, 500 },
            { Piece.QUEEN, 900 },
            { Piece.KING, 30000 },
        };

        private static readonly int[] pawnTable = new int[]
        {
             0,   0,   0,   0,   0,   0,   0,   0,
             40,  40,  40,  40,  40,  40,  40,  40,
             30,  30,  30,  30,  30,  30,  30,  30,
             0,   0,   15,  20,  20,  15,  0,   0,
             0,   0,   0,   15,  10,  0,   0,   0,
             0,   0,   0,   0,   0,   0,   0,   0,
             10,  10,  10, -20, -20,  10,  10,  10,
             0,   0,   0,   0,   0,   0,   0,   0,
        };

        private static readonly int[] knightTable = new int[]
        {
            -40, -30, -20, -20, -20, -20, -30, -40,
            -30, -20,  0,   0,   0,   0,   0,  -30,
            -20,  0,   0,   10,  10,  0,   0,  -20,
            -20,  0,   10,  20,  20,  10,  0,  -20,
            -20,  0,   10,  20,  20,  10,  0,  -20,
            -20,  0,   0,   10,  10,  0,   0,  -20,
            -30, -20,  0,   0,   0,   0,  -20, -30,
            -40, -30, -20, -20, -20, -20, -30, -40,
        };

        private static readonly int[] bishopTable = new int[]
        {
            -30, -20, -20, -20, -20, -20, -20, -30,
            -20,  0,   0,   0,   0,   0,   0,  -20,
            -20,  0,   0,   10,  10,  0,   0,  -20,
            -20,  0,   10,  10,  10,  10,  0,  -20,
            -20,  0,   10,  10,  10,  10,  0,  -20,
            -20,  0,   10,  10,  10,  10,  0,  -20,
            -20,  0,   0,   0,   0,   0,   0,  -20,
            -30, -20, -20, -20, -20, -20, -20, -30,
        };

        private static readonly int[] rookTable = new int[]
        {
             0,   0,   0,   0,   0,   0,   0,   0,
             0,   10,  10,  10,  10,  10,  10,  0,
            -10,  0,   0,   0,   0,   0,   0,  -10,
            -10,  0,   0,   0,   0,   0,   0,  -10,
            -10,  0,   0,   0,   0,   0,   0,  -10,
            -10,  0,   0,   0,   0,   0,   0,  -10,
            -10,  0,   0,   0,   0,   0,   0,  -10,
            -10,  0,   0,   10,  10,  0,   0,  -10,
        };

        private static readonly int[] queenTable = new int[]
        {
             0,   0,   0,   0,   0,   0,   0,   0,
             0,   10,  10,  10,  10,  10,  10,  0,
            -10,  0,   0,   10,  10,  0,   0,  -10,
            -10,  0,   10,  10,  10,  10,  0,  -10,
            -10,  0,   10,  10,  10,  10,  0,  -10,
            -10,  0,   10,  10,  10,  10,  0,  -10,
            -10,  0,   0,   0,   0,   0,   0,  -10,
            -10,  0,   0,   10,  10,  0,   0,  -10,
        };

        private static int ConvertTo64(int square)
        {
            // Get the remainder of the row
            int newSquare = square % 8;

            // Get the row the square should be on
            int extraRows = square / 16;

            // Multiply the extra rows by the row length of the 0x88 board
            newSquare += (extraRows * 8);

            return newSquare;
        }

        private static int GetPSQTIndex(int square, bool isWhite)
        {
            int newSquare = ConvertTo64(square);

            // Extract row and column
            int row = newSquare / 8;
            int col = newSquare % 8;

            // Flip the row if the side using the PSQT is not white (black)
            if (!isWhite)
            {
                row = 7 - row;
            }

            // Return piece square table index
            return (row * 8) + col;
        }

        public static int GetEvaluation()
        {
            // If the evaluation is positive, that means white is winning
            // If the evaluation is negative, that means black is winning
            // If the evaluation is zero, no side is winning
            int evaluation = 0;

            for (int i = 0; i < 120; i++)
            {
                // Continue if the square isn't empty
                if (Board.pieces[i] == Piece.EMPTY)
                {
                    continue;
                }
                if (Board.colours[i] == Colour.WHITE)
                {
                    evaluation += pieceScore[Board.pieces[i]];

                    int index = GetPSQTIndex(i, true);

                    switch (Board.pieces[i])
                    {
                        case Piece.PAWN:
                            evaluation += pawnTable[index];
                            break;
                        case Piece.KNIGHT:
                            evaluation += knightTable[index];
                            break;
                        case Piece.BISHOP:
                            evaluation += bishopTable[index];
                            break;
                        case Piece.ROOK:
                            evaluation += rookTable[index];
                            break;
                        case Piece.QUEEN:
                            evaluation += queenTable[index];
                            break;
                    }
                }
                else
                {
                    evaluation -= pieceScore[Board.pieces[i]];

                    int index = GetPSQTIndex(i, false);

                    switch (Board.pieces[i])
                    {
                        case Piece.PAWN:
                            evaluation -= pawnTable[index];
                            break;
                        case Piece.KNIGHT:
                            evaluation -= knightTable[index];
                            break;
                        case Piece.BISHOP:
                            evaluation -= bishopTable[index];
                            break;
                        case Piece.ROOK:
                            evaluation -= rookTable[index];
                            break;
                        case Piece.QUEEN:
                            evaluation -= queenTable[index];
                            break;
                    }
                }
            }

            return evaluation;
        }
    }
}