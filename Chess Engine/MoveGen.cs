using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine
{
    static internal class MoveGen
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

        // Important squares
        static readonly int A1 = 0;
        static readonly int B1 = 1;
        static readonly int C1 = 2;
        static readonly int D1 = 3;
        static readonly int E1 = 4;
        static readonly int F1 = 5;
        static readonly int G1 = 6;
        static readonly int H1 = 7;

        static readonly int A8 = 112;
        static readonly int B8 = 113;
        static readonly int C8 = 114;
        static readonly int D8 = 115;
        static readonly int E8 = 116;
        static readonly int F8 = 117;
        static readonly int G8 = 118;
        static readonly int H8 = 119;

        // The scores that should be applied to check and promotions
        static readonly int SCORE_CHECK = 25;
        static readonly int SCORE_PROMOTION = 50;

        // Attack vectors
        static readonly int[][] vector = new int[][] {
            new int[] { NW+W, NW+N, NE+N, NE+E, SW+W, SW+S, SE+S, SE+E }, // Knight
            new int[] { NE, NW, SE, SW }, // Bishop
            new int[] { N, E, S, W }, // Rook
            new int[] { N, NE, E, SE, S, SW, W, NW }, // Queen
            new int[] { N, NE, E, SE, S, SW, W, NW }, // King
        };

        // MVV-LVA
        static readonly int[][] MVV_LVA = new int[][] {
            new int[] { 0, 0, 0, 0, 0, 0, 0  }, // Victim EMPTY - Attacker E,P,N,B,R,Q,K 
            new int[] { 0, 15,14,13,12,11,10 }, // Victim PAWN - Attacker E,P,N,B,R,Q,K 
            new int[] { 0, 21,20,19,18,17,16 }, // Victim KNIGHT - Attacker E,P,N,B,R,Q,K 
            new int[] { 0, 27,26,25,24,23,22 }, // Victim BISHOP - Attacker E,P,N,B,R,Q,K 
            new int[] { 0, 33,32,31,30,29,28 }, // Victim ROOK - Attacker E,P,N,B,R,Q,K 
            new int[] { 0, 39,38,37,36,35,34 }, // Victim QUEEN - Attacker E,P,N,B,R,Q,K 
            new int[] { 0, 45,44,43,42,41,40 }, // Victim KING - Attacker E,P,N,B,R,Q,K 
        };

        static List<Move> possibleMoves = new List<Move>();

        public static Move StringToMove(string move)
        {
            // Get squares and pieces
            int from = Board.SquareToInt(move[0].ToString() + move[1].ToString());
            int to = Board.SquareToInt(move[2].ToString() + move[3].ToString().ToString());
            Piece pieceFrom = Board.pieces[from];
            Piece pieceCaptured = Board.pieces[to];
            int flags = 0;

            // Add flags by checking for en passant, double pawn pushes, and captures
            if (pieceFrom == Piece.PAWN && to == Board.ep)
            {
                flags += (int)MoveType.EN_PASSANT;
            } 
            else if (pieceFrom == Piece.PAWN && Math.Abs(from-to) == 32)
            {
                flags += (int)MoveType.PAWN_DOUBLE;
            }
            else if (Board.pieces[to] != Piece.EMPTY)
            {
                flags += (int)MoveType.CAPTURE;
            }

            // Check if the move is a castle
            if (pieceFrom == Piece.KING)
            {
                if ((from == E1 && to == G1) || (from == E1 && to == C1) || (from == E8 && to == G8) || (from == E8 && to == C8))
                {
                    flags += (int)MoveType.CASTLE;
                }
            }

            // Create the generated move
            Move generatedMove = new Move(from, to, pieceFrom, pieceCaptured, flags);

            // Add the promotion flag if the move is a promotion
            if (pieceFrom == Piece.PAWN && (Board.GetRow(to) == 1 || Board.GetRow(to) == 8))
            {
                generatedMove.Flags += (int)MoveType.PROMOTION;
                generatedMove.PieceTo = Board.asciiToPiece[move[4]];
            }

            return generatedMove;
        }
        static private void MoveGenPush(int from, int to, Piece pieceFrom, Piece pieceCaptured, int flags=0)
        {
            Move move = new Move(from, to, pieceFrom, pieceCaptured, flags);

            if (move.PieceFrom == Piece.PAWN)
            {
                // If the move is an en passant, update the flag to reflect that
                if (move.To == Board.ep)
                {
                    move.Flags = (int)MoveType.EN_PASSANT;
                }

                // If the pawn is going to the 1st or 8th rank, generate the promotion moves
                if (Board.GetRow(move.To) == 1 || Board.GetRow(move.To) == 8)
                {
                    // Create queen promotion and add
                    Move queenPromotion = new Move(from, to, pieceFrom, pieceCaptured, flags + (int)MoveType.PROMOTION);
                    queenPromotion.PieceTo = Piece.QUEEN;
                    possibleMoves.Add(queenPromotion);

                    // Create rook promotion and add
                    Move rookPromotion = new Move(from, to, pieceFrom, pieceCaptured, flags + (int)MoveType.PROMOTION);
                    rookPromotion.PieceTo = Piece.ROOK;
                    possibleMoves.Add(rookPromotion);

                    // Create bishop promotion and add
                    Move bishopPromotion = new Move(from, to, pieceFrom, pieceCaptured, flags + (int)MoveType.PROMOTION);
                    bishopPromotion.PieceTo = Piece.BISHOP;
                    possibleMoves.Add(bishopPromotion);

                    // Create knight promotion and add
                    Move knightPromotion = new Move(from, to, pieceFrom, pieceCaptured, flags + (int)MoveType.PROMOTION);
                    knightPromotion.PieceTo = Piece.KNIGHT;
                    possibleMoves.Add(knightPromotion);

                    return;
                }
            }

            possibleMoves.Add(move);
        }

        static public void SortMoves(List<Move> moves)
        {
            moves.Sort((x, y) =>
                y.Score.CompareTo(x.Score));
        }

        static private void ScoreMove(Move move)
        {
            // Add the capture score to the move if it is a capture
            if (Convert.ToBoolean(move.Flags & (int)MoveType.CAPTURE))
            {
                move.Score += MVV_LVA[(int)move.PieceCaptured][(int)move.PieceFrom];
            }

            // Add the promotion score + the value of the promoted to the move if it is a promotion
            if (Convert.ToBoolean(move.Flags & (int)MoveType.PROMOTION))
            {
                move.Score += SCORE_PROMOTION + Evaluate.pieceScore[move.PieceTo] / 10;
            }

            // Add the check score to the move if it is a check
            if (Convert.ToBoolean(move.Flags & (int)MoveType.CHECK))
            {
                move.Score += SCORE_CHECK;
            }
        }

        static private void CheckCastling(int[] castlingSquares, Colour stm)
        {
            // Check castling
            foreach (int square in castlingSquares)
            {
                // Only check if the square is empty if it is not a king square
                if (square != E1 && square != E8)
                {
                    if (Board.pieces[square] != Piece.EMPTY)
                    {
                        return;
                    }
                }

                // Check if the squares that the king moves through are attacked by the other side or not
                if (square != B1 && square != B8 && Attacks.IsAttacked(stm, square))
                {
                    return;
                }
            }

            MoveGenPush(castlingSquares[0], castlingSquares[2], Piece.KING, Piece.EMPTY, (int)MoveType.CASTLE);
        }

        static public List<Move> Generate()
        {
            // Clear possible moves
            possibleMoves.Clear();

            // To get the side to wait, get the side to move and
            // 1. Convert it to a boolean
            // 2. Invert the boolean
            // 3. Convert it to an integer
            // 4. Convert back to a colour
            Colour stw = (Colour)Convert.ToInt16(!Convert.ToBoolean(Board.stm));

            // Castling
            if (Board.stm == Colour.WHITE)
            {
                if (Convert.ToBoolean(Board.castle & (int)Castle.WHITE_KING))
                {
                    int[] castlingSquares = new int[] {E1, F1, G1};

                    CheckCastling(castlingSquares, Board.stm);
                }

                if (Convert.ToBoolean(Board.castle & (int)Castle.WHITE_QUEEN))
                {
                    int[] castlingSquares = new int[] {E1, D1, C1, B1};

                    CheckCastling(castlingSquares, Board.stm);
                }
            } 
            else
            {
                if (Convert.ToBoolean(Board.castle & (int)Castle.BLACK_KING))
                {
                    int[] castlingSquares = new int[] {E8, F8, G8};

                    CheckCastling(castlingSquares, Board.stm);
                }

                if (Convert.ToBoolean(Board.castle & (int)Castle.BLACK_QUEEN))
                {
                    int[] castlingSquares = new int[] {E8, D8, C8, B8};

                    CheckCastling(castlingSquares, Board.stm);
                }
            }

            // Pieces and pawns
            for (int square = 0; square < 120; square++)
            {
                // If the piece isn't owned by the side that it moving, it isn't allowed to be moved
                if (Board.stm != Board.colours[square])
                {
                    continue;
                }

                // Pawn moves
                if (Board.pieces[square] == Piece.PAWN)
                {
                    if (Board.stm == Colour.WHITE)
                    {
                        // Pawn pushes
                        if (Board.ValidSquare(square + N) && Board.pieces[square + N] == Piece.EMPTY)
                        {
                            MoveGenPush(square, square + N, Piece.PAWN, Piece.EMPTY, (int)MoveType.NORMAL);

                            // If the pawn is on the second row and the piece two above it is empty,
                            // the double move is legal!
                            if (Board.ValidSquare(square + N + N) && Board.GetRow(square) == 2 && Board.pieces[square + N + N] == Piece.EMPTY)
                            {
                                MoveGenPush(square, square + N + N, Piece.PAWN, Piece.EMPTY, (int)MoveType.PAWN_DOUBLE);
                            }
                        }

                        // Pawn captures
                        // If the possible capture square is equal to the enpassant square, capture enpassant!
                        // If the possible capture square has the opposite colour, there must
                        // be an enemy piece there (It's colour would be EMPTY otherwise). Capture!
                        if (Board.ValidSquare(square + NW) && square + NW == Board.ep)
                        {
                            MoveGenPush(square, square + NW, Piece.PAWN, Board.pieces[square + NW], (int)MoveType.EN_PASSANT);
                        }
                        if (Board.ValidSquare(square + NW) && Board.colours[square + NW] == Colour.BLACK)
                        {
                            MoveGenPush(square, square + NW, Piece.PAWN, Board.pieces[square + NW], (int)MoveType.CAPTURE);
                        }
                        if (Board.ValidSquare(square + NE) && square + NE == Board.ep)
                        {
                            MoveGenPush(square, square + NE, Piece.PAWN, Board.pieces[square + NE], (int)MoveType.EN_PASSANT);
                        }
                        if (Board.ValidSquare(square + NE) && Board.colours[square + NE] == Colour.BLACK)
                        {
                            MoveGenPush(square, square + NE, Piece.PAWN, Board.pieces[square + NE], (int)MoveType.CAPTURE);
                        }
                    }
                    else
                    {
                        // Pawn pushes
                        if (Board.ValidSquare(square + S) && Board.pieces[square + S] == Piece.EMPTY)
                        {
                            MoveGenPush(square, square + S, Piece.PAWN, Piece.EMPTY, (int)MoveType.NORMAL);

                            // If the pawn is on the second row and the piece two above it is empty,
                            // the double move is legal!
                            if (Board.ValidSquare(square + S + S) && Board.GetRow(square) == 7 && Board.pieces[square + S + S] == Piece.EMPTY)
                            {
                                MoveGenPush(square, square + S + S, Piece.PAWN, Piece.EMPTY, (int)MoveType.PAWN_DOUBLE);
                            }
                        }

                        // Pawn captures
                        // If the possible capture square is equal to the enpassant square, capture enpassant!
                        // If the possible capture square has the opposite colour, there must
                        // be an enemy piece there (It would be COLOUR_EMPTY otherwise). Capture!
                        if (Board.ValidSquare(square + SW) && square + SW == Board.ep)
                        {
                            MoveGenPush(square, square + SW, Piece.PAWN, Board.pieces[square + SW], (int)MoveType.EN_PASSANT);
                        }
                        if (Board.ValidSquare(square + SW) && Board.colours[square + SW] == Colour.WHITE)
                        {
                            MoveGenPush(square, square + SW, Piece.PAWN, Board.pieces[square + SW], (int)MoveType.CAPTURE);
                        }
                        if (Board.ValidSquare(square + SE) && square + SE == Board.ep)
                        {
                            MoveGenPush(square, square + SE, Piece.PAWN, Board.pieces[square + SE], (int)MoveType.EN_PASSANT);
                        }
                        if (Board.ValidSquare(square + SE) && Board.colours[square + SE] == Colour.WHITE)
                        {
                            MoveGenPush(square, square + SE, Piece.PAWN, Board.pieces[square + SE], (int)MoveType.CAPTURE);
                        }
                    }
                    continue;
                }

                // Other
                foreach (int i in vector[(int)Board.pieces[square] - 2])
                {
                    int pos = square + i;

                    while (Board.ValidSquare(pos))
                    {
                        if (Board.pieces[pos] == Piece.EMPTY)
                        {
                            MoveGenPush(square, pos, Board.pieces[square], Piece.EMPTY, (int)MoveType.NORMAL);
                        }
                        else
                        {
                            // If the piece on the new square is not the same colour as
                            // the side to move, it is an enemy piece and can be taken
                            if (Board.stm != Board.colours[pos])
                            {
                                MoveGenPush(square, pos, Board.pieces[square], Board.pieces[pos], (int)MoveType.CAPTURE);
                            }

                            // Running into another piece results in the piece not being able to
                            // continue in that direction, so break the loop!
                            break;
                        }

                        // Knights and kings aren't sliding pieces and can only go in a direction once
                        // compared to pieces like bishops which can go further in certain direction
                        if (Board.pieces[square] == Piece.KNIGHT || Board.pieces[square] == Piece.KING)
                        {
                            break;
                        }

                        // Continue to move in the direction
                        pos += i;
                    }
                }
            }

            List<Move> legalMoves = new List<Move>();

            foreach (Move move in possibleMoves)
            {
                Board.MakeMove(move);

                // Update side to wait as the stw has now swapped
                // due to a move being made
                stw = (Colour)Convert.ToInt16(!Convert.ToBoolean(Board.stm));

                // Is the king of the side that just moved in check?
                if (!Board.IsKingAttacked(stw))
                {
                    // If the current side's king is attacked (MakeMove flips the side), flag the move as a check
                    if (Board.IsKingAttacked(Board.stm))
                    {
                        move.Flags += (int)MoveType.CHECK;
                    }

                    ScoreMove(move);

                    // Add the move as the move isn't illegal
                    legalMoves.Add(move);
                }
                    
                Board.UnmakeMove(move);
            }

            return legalMoves;
        }
    }
}