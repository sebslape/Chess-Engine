using System;
using System.Threading;

namespace Chess_Engine
{
    public enum Castle
    {
        WHITE_KING = 1,
        WHITE_QUEEN = 2,
        BLACK_KING = 4,
        BLACK_QUEEN = 8
    }
    public enum Piece
    {
        EMPTY = 0,
        PAWN = 1,
        KNIGHT = 2,
        BISHOP = 3,
        ROOK = 4,
        QUEEN = 5,
        KING = 6
    }
    public enum Colour
    {
        BLACK = 0,
        WHITE = 1,
        EMPTY = 2
    }
    public enum MoveType
    {
        NORMAL = 0,
        CAPTURE = 1,
        PAWN_DOUBLE = 2, // Double pawn push
        EN_PASSANT = 4, // Capture enpassant
        CASTLE = 8,
        PROMOTION = 16,
        CHECK = 32
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Board.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            while (true)
            {
                string? command = Console.ReadLine();

                // If the command is null, continue
                if (command == null)
                {
                    continue;
                }

                if (command == "uci")
                {
                    Console.WriteLine("id name Cool Engine");
                    Console.WriteLine("id author Sebastian Slape");
                    Console.WriteLine("uciok");
                    continue;
                }

                if (command == "stop")
                {
                    Search.stop = true;
                    continue;
                }

                if (command == "show")
                {
                    Board.Show();
                    continue;
                }

                if (command.Length >= 8 && command.Substring(0,8) == "position")
                {
                    if (command.Length >= 12 && command.Substring(9,3) == "fen")
                    {
                        Board.LoadFEN(command.Substring(13));
                    } 
                    else if (command.Length >= 17 && command.Substring(9,8) == "startpos")
                    {
                        Board.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                    }

                    int movesLocation = command.IndexOf("moves");

                    if (movesLocation == -1)
                    {
                        continue;
                    }

                    string[] moveList = command.Substring(movesLocation + 6).Split(' ');

                    foreach (string move in moveList)
                    {
                        Board.MakeMove(MoveGen.StringToMove(move));
                    }

                    continue;
                }

                if (command.Length >= 2 && command.Substring(0,2) == "go")
                {
                    if (command.Length >= 8 && command.Substring(3, 5) == "perft")
                    {
                        Search.PerftRoot(int.Parse(command.Substring(9)));
                        continue;
                    }

                    var task = new Task(() => Search.Iterate(command.Substring(2).Split(' ')));

                    task.Start();
                    continue;
                }

                if (command.Length >= 8 && command.Substring(0, 8) == "evaluate")
                {
                    List<Move> moves = MoveGen.Generate();

                    Console.WriteLine("Evaluation: " + Evaluate.GetEvaluation());
                    Board.Show(true);
                    continue;
                }

                if (command.Length >= 4 && command.Substring(0, 4) == "play")
                {
                    Board.Show(true);

                    while (true)
                    {
                        Console.Write("What depth should the engine search? ");
                        string? depth = "depth " + Console.ReadLine();

                        if (depth == "depth 0")
                        {
                            break;
                        }

                        Search.Iterate(depth.Split(' '), true);

                        Board.Show(true);

                        Search.GetMoves();

                        Board.Show(true);
                    }                    
                    continue;
                }

                Console.WriteLine("'" + command + "' is an unknown command.");
            }
        }
    }
}
