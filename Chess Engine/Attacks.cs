using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine
{
    static internal class Attacks
    {
        // Directions
        static readonly int N = 16;
        static readonly int E = 1;
        static readonly int S = -16;
        static readonly int W = -1;

        static readonly int NE = N + E;
        static readonly int NW = N + W;
        static readonly int SE = S + E;
        static readonly int SW = S + W;

        // Attack vectors
        static readonly int[][] vector = new int[][] {
            new int[] { NW+W, NW+N, NE+N, NE+E, SW+W, SW+S, SE+S, SE+E }, // Knight
            new int[] { N, E, S, W }, // Straights
            new int[] { NE, NW, SE, SW }, // Diagonals
            new int[] { N, NE, E, SE, S, SW, W, NW } // King
        };
        private static bool SlideAttacks(Colour stm, int square)
        {
            foreach (int i in vector[1])
            {
                int pos = square + i;

                while (Board.ValidSquare(pos))
                {
                    if (Board.pieces[pos] == Piece.ROOK || Board.pieces[pos] == Piece.QUEEN)
                    {
                        if (Board.colours[pos] != stm)
                        {
                            return true;
                        }
                    }  

                    if (Board.pieces[pos] != Piece.EMPTY)
                    {
                        break;
                    }
                        
                    pos += i;
                }
            }

            foreach (int i in vector[2])
            {
                int pos = square + i;

                while (Board.ValidSquare(pos)) {
                    if (Board.pieces[pos] == Piece.BISHOP || Board.pieces[pos] == Piece.QUEEN) {
                        if (Board.colours[pos] != stm)
                        {
                            return true;
                        }
                    }

                    if (Board.pieces[pos] != Piece.EMPTY)
                    {
                        break;
                    }

                    pos += i;
                }
            }

            return false;
        }
        public static bool IsAttacked(Colour stm, int square)
        {
            // Knights
            foreach (int i in vector[0]) {
                int pos = square + i;

                if (!Board.ValidSquare(pos))
                {
                    continue;
                }

                if (Board.pieces[pos] == Piece.KNIGHT && Board.colours[pos] != stm)
                {
                    return true;
                }
            } 

            // Kings
            foreach (int i in vector[3])
            {
                int pos = square + i;

                if (!Board.ValidSquare(pos))
                {
                    continue;
                }
                if (Board.pieces[pos] == Piece.KING && Board.colours[pos] != stm) 
                {
                    return true;
                }    
            }

            if (SlideAttacks(stm, square))
            {
                return true;
            }

            // Pawns
            if (stm == Colour.WHITE)
            {
                if (Board.ValidSquare(square + NW) && Board.pieces[square + NW] == Piece.PAWN && Board.colours[square + NW] != stm)
                {
                    return true;
                } 
                if (Board.ValidSquare(square + NE) && Board.pieces[square + NE] == Piece.PAWN && Board.colours[square + NE] != stm)
                {
                    return true;
                }
            } 
            else
            {
                if (Board.ValidSquare(square + SW) && Board.pieces[square + SW] == Piece.PAWN && Board.colours[square + SW] != stm)
                {
                    return true;
                }
                if (Board.ValidSquare(square + SE) && Board.pieces[square + SE] == Piece.PAWN && Board.colours[square + SE] != stm)
                {
                    return true;
                }
            }

            return false;
        }
    }
}