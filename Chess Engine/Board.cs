using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine
{
    static internal class Board
    {
        public static Colour stm = Colour.WHITE;
        public static Piece[] pieces = new Piece[128];
        public static Colour[] colours = new Colour[128];
        public static int[] kingPositions = new int[2];

        public static int ep = -1;
        public static int castle = 0;

        // How many total halfmoves there are
        public static int ply = 0;

        // Resets when capture or pawn move (or both) if it reaches 100 (50 full moves), it's a draw
        public static int halfMoveClock = 0;

        public static readonly Dictionary<char, Piece> asciiToPiece = new Dictionary<char, Piece>() 
        {
            { 'e', Piece.EMPTY },
            { 'p', Piece.PAWN },
            { 'n', Piece.KNIGHT },
            { 'b', Piece.BISHOP },
            { 'r', Piece.ROOK },
            { 'q', Piece.QUEEN },
            { 'k', Piece.KING },
        };

        private static readonly Dictionary<char, int> letters = new Dictionary<char, int>()
        {
            { 'a', 0 },
            { 'b', 1 },
            { 'c', 2 },
            { 'd', 3 },
            { 'e', 4 },
            { 'f', 5 },
            { 'g', 6 },
            { 'h', 7 },
        };

        public static readonly string[] numToPieceName = new string[] {"e","p","n","b","r","q","k"};

        public static readonly string[] numToSquareName = new string[] {
            "A1","B1","C1","D1","E1","F1","G1","H1","x","x","x","x","x","x","x","x",
            "A2","B2","C2","D2","E2","F2","G2","H2","x","x","x","x","x","x","x","x",
            "A3","B3","C3","D3","E3","F3","G3","H3","x","x","x","x","x","x","x","x",
            "A4","B4","C4","D4","E4","F4","G4","H4","x","x","x","x","x","x","x","x",
            "A5","B5","C5","D5","E5","F5","G5","H5","x","x","x","x","x","x","x","x",
            "A6","B6","C6","D6","E6","F6","G6","H6","x","x","x","x","x","x","x","x",
            "A7","B7","C7","D7","E7","F7","G7","H7","x","x","x","x","x","x","x","x",
            "A8","B8","C8","D8","E8","F8","G8","H8","x","x","x","x","x","x","x","x"
        };

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

        public static string GetMoveString(Move move) 
        {
            string moveString = numToSquareName[move.From].ToLower() + numToSquareName[move.To].ToLower();

            if (Convert.ToBoolean(move.Flags & (int)MoveType.PROMOTION))
            {
                moveString += numToPieceName[(int)move.PieceTo];
            }

            return moveString;
        }
        public static void ResetBoard()
        {
            stm = Colour.WHITE;

            // Reset pieces
            for (int i = 0; i < 128; i++)
            {
                pieces[i] = Piece.EMPTY;
                colours[i] = Colour.EMPTY;
            }

            kingPositions = new int[2];

            ep = 0;
            castle = 0;
            ply = 0;
            halfMoveClock = 0;
        }

        static Board()
        {
            ResetBoard();
        }

        public static bool IsKingAttacked(Colour colour)
        {
            int kingSquare = kingPositions[(int)colour];

            if (Attacks.IsAttacked(colour, kingSquare))
            {
                return true;
            }

            return false;
        }

        public static void MakeMove(Move move)
        {
            // Swap colour
            Colour stw = stm;
            stm = (Colour)Convert.ToInt16(!Convert.ToBoolean(stm));

            // Clear "to" square if the move is a capture
            if (pieces[move.To] != Piece.EMPTY)
            {
                ClearSquare(move.To);
            }

            // Clear "from" square
            ClearSquare(move.From);

            // The colour of the moved piece must be the current side and 
            // we have already swapped sides above so we use the swapped version
            FillSquare(move.PieceTo, stw, move.To);

            // Check if the kings move
            if (move.From == E1 || move.To == E1)
            {
                castle &= ~((int)Castle.WHITE_KING | (int)Castle.WHITE_QUEEN);
            }
            else if (move.From == E8 || move.To == E8)
            {
                castle &= ~((int)Castle.BLACK_KING | (int)Castle.BLACK_QUEEN);
            }

            // Check if the rooks move
            if (move.From == H1 || move.To == H1)
            {
                castle &= ~(int)Castle.WHITE_KING;
            }  
            if (move.From == A1 || move.To == A1)
            {
                castle &= ~(int)Castle.WHITE_QUEEN;
            }
            if (move.From == H8 || move.To == H8)
            {
                castle &= ~(int)Castle.BLACK_KING;
            }   
            if (move.From == A8 || move.To == A8)
            {
                castle &= ~(int)Castle.BLACK_QUEEN;
            } 

            if (Convert.ToBoolean(move.Flags & (int)MoveType.CASTLE))
            {
                // Move rook
                if (move.To == G1)
                {
                    ClearSquare(H1);
                    FillSquare(Piece.ROOK, Colour.WHITE, F1);
                }
                else if (move.To == C1)
                {
                    ClearSquare(A1);
                    FillSquare(Piece.ROOK, Colour.WHITE, D1);
                }
                else if (move.To == G8)
                {
                    ClearSquare(H8);
                    FillSquare(Piece.ROOK, Colour.BLACK, F8);
                }
                else if (move.To == C8)
                {
                    ClearSquare(A8);
                    FillSquare(Piece.ROOK, Colour.BLACK, D8);
                }
            }

            if (ep != -1)
            {
                ep = -1;
            }

            // Setting ep square
            if (move.PieceFrom == Piece.PAWN)
            {
                if (Convert.ToBoolean(move.Flags & (int)MoveType.PAWN_DOUBLE))
                {
                    // If the pawn was a double pawn push, it was
                    // moved 32 squares as each row is 16 long
                    // Get distance from the two squares, and
                    // divide by two to get the middle.
                    // This means we don't have to calculate whether
                    // the pawn moved 16 up or 16 down!
                    ep = (move.From + move.To) / 2;
                }
            }

            // Update king positions
            if (move.PieceFrom == Piece.KING)
            {
                if (stw == Colour.WHITE)
                {
                    kingPositions[1] = move.To;
                } 
                else
                {
                    kingPositions[0] = move.To;
                }
            }

            if (Convert.ToBoolean(move.Flags & (int)MoveType.EN_PASSANT))
            {
                if (stw == Colour.WHITE)
                {
                    ClearSquare(move.To - 16);
                } else
                {
                    ClearSquare(move.To + 16);
                }
            }  
        }

        public static void UnmakeMove(Move move)
        {
            // Swap colour
            Colour stw = stm;
            stm = (Colour)Convert.ToInt16(!Convert.ToBoolean(stm));

            // Clear the new square and put the piece back
            ClearSquare(move.To);
            FillSquare(move.PieceFrom, stm, move.From);

            if (Convert.ToBoolean(move.Flags & (int)MoveType.CAPTURE))
            {
                pieces[move.To] = move.PieceCaptured;

                // If the 'to' square is empty, don't give it a colour otherwise
                // set it to the opposite colour
                if (move.PieceCaptured == Piece.EMPTY)
                {
                    colours[move.To] = Colour.EMPTY;
                }
                else
                {
                    colours[move.To] = stw;
                }
            }

            if (Convert.ToBoolean(move.Flags & (int)MoveType.CASTLE))
            {
                // Unmove rook
                if (move.To == G1)
                {
                    ClearSquare(F1);
                    FillSquare(Piece.ROOK, Colour.WHITE, H1);
                }
                else if (move.To == C1)
                {
                    ClearSquare(D1);
                    FillSquare(Piece.ROOK, Colour.WHITE, A1);
                }
                else if (move.To == G8)
                {
                    ClearSquare(F8);
                    FillSquare(Piece.ROOK, Colour.BLACK, H8);
                }
                else if (move.To == C8)
                {
                    ClearSquare(D8);
                    FillSquare(Piece.ROOK, Colour.BLACK, A8);
                }
            }

            // Update king positions
            if (move.PieceFrom == Piece.KING)
            {
                if (stm == Colour.WHITE)
                {
                    kingPositions[1] = move.From;
                }
                else
                {
                    kingPositions[0] = move.From;
                }
            }

            if (Convert.ToBoolean(move.Flags & (int)MoveType.EN_PASSANT))
            {
                if (stm == Colour.WHITE)
                {
                    FillSquare(Piece.PAWN, Colour.BLACK, move.To - 16);
                }
                else
                {
                    FillSquare(Piece.PAWN, Colour.WHITE, move.To + 16);
                }  
            }

            // Setting board info to move info
            ep = move.Ep;
            castle = move.Castle;
            ply = move.Ply;
            halfMoveClock = move.HalfMoveClock;
        }

        // Board Updating
        public static void ClearSquare(int square)
        {
            pieces[square] = Piece.EMPTY;
            colours[square] = Colour.EMPTY;
        }

        public static void FillSquare(Piece piece, Colour colour, int square)
        {
            pieces[square] = piece;
            colours[square] = colour;
        }

        public static int GetRow(int square)
        {
            // Lets say that the square is 49 (B4) which is 00110001

            // If we shift this number 4 bits to the right, we get 00000011 or 3 in
            // decimal which is the correct row

            // This works because 00010000 in binary is equal to 16 which is the
            // number of columns in a 0x88 board representation
            // When we shift the number 4 bits to the right, we are essentially
            // only leaving the multiples of 16 left as a bit shift
            // to the right is equivalent to dividing by 2, and doing this four
            // times results in a division of 2^4 = 16.

            // Lastly, we add 1 to the row number as the rows should start at 1
            // instead of 0. We now have our row!
            return (square >> 4) + 1;
        }

        public static bool ValidSquare(int square)
        {
            return !Convert.ToBoolean(square & 0x88);
        }

        // Inverted due to fen strings being formed from top to bottom
        // row 8 > row 7 > row 6...
        private static int PosToInt(int row, int col)
        {
            return ((7 - row) * 16) + col;
        }

        public static int SquareToInt(string square)
        {
            if (square.Length < 2)
            {
                return 0;
            }

            int col = letters[square[0]];
            int row = int.Parse(square[1].ToString());

            return (row - 1) * 16 + col;
        }

        public static void Show(bool simple = false)
        {
            Console.WriteLine("");

            if (!simple) { Console.WriteLine("=============================="); }

            // Show stats
            if (stm == Colour.WHITE)
            {
                if (!simple) { Console.WriteLine("Side to move: White"); }
            } 
            else
            {
                if (!simple) { Console.WriteLine("Side to move: Black"); }
            }

            // Show king positions
            if (!simple) { Console.WriteLine("White King: " + numToSquareName[kingPositions[1]]); }
            
            if (!simple) { Console.WriteLine("Black King: " + numToSquareName[kingPositions[0]]); }

            // Check castling
            string castling = "";

            if (Convert.ToBoolean(castle & (int)Castle.WHITE_KING)) 
            {
                castling += "K";
            }    
            if (Convert.ToBoolean(castle & (int)Castle.WHITE_QUEEN))
            {
                castling += "Q";
            } 
            if (Convert.ToBoolean(castle & (int) Castle.BLACK_KING))
            {
                castling += "k";
            }
            if (Convert.ToBoolean(castle & (int) Castle.BLACK_QUEEN))
            {
                castling += "q";
            }

            // Show castling
            if (castle != 0)
            {
                if (!simple) { Console.WriteLine("Castling: " + castling); }
            }  
            else
            {
                if (!simple) { Console.WriteLine("Castling: None"); }
            }

            // Show en passant
            if (ep > 0)
            {
                if (!simple) { Console.WriteLine("En Passant: " + numToSquareName[ep]); }
            }
            else
            {
                if (!simple) { Console.WriteLine("En Passant: None"); }
            }
            
            if (!simple) { Console.WriteLine(""); }

            // Show pieces
            bool showNum = false;

            for (int row = 0; row < 8; row++)
            {
                string rowPrint = Convert.ToString(8 - row) + "  ";
                for (int col = 0; col < 16; col++)
                {
                    int square = PosToInt(row, col);

                    if (ValidSquare(square))
                    {
                        if (showNum == true)
                        {
                            rowPrint += Convert.ToString(pieces[square]) + " ";
                            continue;
                        }

                        // Add piece
                        if (colours[square] == Colour.WHITE)
                        {
                            rowPrint += numToPieceName[(int)pieces[square]].ToUpper() + " ";
                        }
                        else if (colours[square] == Colour.BLACK)
                        {
                            rowPrint += numToPieceName[(int)pieces[square]] + " ";
                        }
                        else
                        {
                            rowPrint += "  ";
                        }
                    }
                    else
                    {
                        if (!simple) { rowPrint += "x "; }
                    }
                }

                Console.WriteLine(rowPrint);
            }
            
            Console.WriteLine("\n   a b c d e f g h");

            if (!simple) { Console.WriteLine("=============================="); }

            Console.WriteLine("");
        }

        public static void LoadFEN(string fen)
        {
            ResetBoard();

            int row = 0;
            int col = 0;
            int pos = 0;

            // Position reading
            while (fen[pos] != ' ') {

                if (char.IsDigit(fen[pos]))
                {
                    col += int.Parse(fen[pos].ToString());
                    pos += 1;
                    continue;
                }

                // The character is a "/" so we increase the row, reset the column, and increase the position
                // the column should be equal to 8 as there are 8 columns in each row
                if (fen[pos] == '/') {
                    if (col != 8)
                    {
                        throw new Exception("Row doesn't add up to 8");
                    }

                    row += 1;
                    col = 0;
                    pos += 1;
                    continue;
                }
                    
                // If the column is more or equal to 8, we reset the column and increase the position as it is impossible to have more than 8 characters in a row
                // The next character should be a "/"
                if (col >= 8)
                {
                    pos += 1;
                    continue;
                }

                int square = PosToInt(row, col);

                pieces[square] = asciiToPiece[char.ToLower(fen[pos])];

                if (fen[pos] == char.ToUpper(fen[pos]))
                {
                    // Update king positions
                    if (char.ToUpper(fen[pos]) == 'K')
                    {
                        kingPositions[1] = square;
                    }
                    colours[square] = Colour.WHITE;
                }
                else
                {
                    if (char.ToUpper(fen[pos]) == 'K')
                    {
                        kingPositions[0] = square;
                    }
                    colours[square] = Colour.BLACK;
                }

                col += 1;
                pos += 1;
            }

            pos += 1;

            // Side to move
            if (fen[pos] == 'w')
            {
                stm = Colour.WHITE;
            }
            else
            {
                stm = Colour.BLACK;
            }

            // Castling
            pos += 2;

            while (pos < fen.Length && fen[pos] != ' ')
            {
                if (fen[pos] == 'K') 
                {
                    castle += (int)Castle.WHITE_KING;
                }
                else if (fen[pos] == 'Q')
                {
                    castle += (int)Castle.WHITE_QUEEN;
                }   
                else if (fen[pos] == 'k')
                {
                    castle += (int)Castle.BLACK_KING;
                }
                else if (fen[pos] == 'q')
                {
                    castle += (int)Castle.BLACK_QUEEN;
                }   
                else if (fen[pos] == '-')
                {
                    pos += 1;
                    break;
                }
                pos += 1;
            }

            // En passant
            pos += 1;

            if (pos < fen.Length && fen[pos] != '-')
            {
                ep = SquareToInt(fen[pos].ToString() + fen[pos + 1].ToString());
            }

            while (pos < fen.Length && fen[pos] != ' ')
            {
                pos += 1;
            }

            pos += 1;

            // Exit if the end of the fen has been reached
            // This happens if the fen string doesn't include
            // ply or halfmove clock information
            if (pos >= fen.Length)
            {
                return;
            }

            // Halfmove clock
            int halfMove = 0;

            if (fen[pos] != '-' && char.IsDigit(fen[pos]))
            {
                while (true) 
                {
                    if (fen[pos] == ' ')
                    {
                        break;
                    }

                    // If the next character is a digit then we multiply the halfmove
                    // by 10 to move the digits one place over if it isn't zero
                    // and then add the second digit

                    // Example:
                    // halfmove = 0
                    // fen = "27"

                    // Pass 1:
                    // halfmove * 10 = 0
                    // halfmove + 2 = 2

                    // Pass 2:
                    // halfmove * 10 = 20
                    // halfmove + 7 = 27
                    // >>> halfmove = 27 <<<

                    halfMove *= 10;
                    halfMove += int.Parse(fen[pos].ToString());
                    pos += 1;
                }

                halfMoveClock = halfMove;
            }

            while (fen[pos] != ' ')
            {
                pos += 1;
            }

            // Full moves
            pos += 1;

            int fullmove = 0;

            // Same process as halfmove
            if (fen[pos] != '-' && char.IsDigit(fen[pos]))
            {
                while (true)
                {
                    if (pos >= fen.Length)
                    {
                        break;
                    }

                    if (fen[pos] == ' ')
                    {
                        break;
                    }

                    fullmove *= 10;
                    fullmove += int.Parse(fen[pos].ToString());
                    pos += 1;
                }   
            }   
        }
    }
}