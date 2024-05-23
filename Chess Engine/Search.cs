using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine
{
    static internal class Search
    {
        public static int whiteTime = 0;
        public static int blackTime = 0;
        public static bool stop = false;
        public static Move? PVMove = null;

        private static int maxDepth = 1;
        private static int positions = 0;
        private static int MATEVALUE = 99999; // Make the mate value extremely large

        public static void GetMoves()
        {
            string? playedMove;
            while (true)
            {
                Console.Write("What move was played? ");
                playedMove = Console.ReadLine();

                if (playedMove == " " || playedMove == null)
                {
                    continue;
                }

                break;
            }

            Board.MakeMove(MoveGen.StringToMove(playedMove.ToLower()));
        }
        private static int Perft(int depth)
        {
            if (depth == 0)
            {
                positions++;
                return 1;
            }

            int count = 0;
            List<Move> moves = MoveGen.Generate();
            for (int i = 0; i < moves.Count; i++)
            {
                Board.MakeMove(moves[i]);
                count += Perft(depth - 1);
                Board.UnmakeMove(moves[i]);
            }

            return count;
        }
        public static void PerftRoot(int depth)
        {
            positions = 0;

            List<Move> moves = MoveGen.Generate();

            foreach (Move move in moves)
            {
                Board.MakeMove(move);

                int moveCount = Perft(depth - 1);

                Console.Write(Board.GetMoveString(move) + ": " + moveCount + "\n");

                Board.UnmakeMove(move);
            }

            Console.WriteLine("\nNodes Searched: " + positions + "\n");
        }
        private static int AlphaBeta(int alpha, int beta, int depth, int ply, bool isWhite)
        {
            List<Move> moves = MoveGen.Generate();

            MoveGen.SortMoves(moves);

            if (moves.Count == 0)
            {
                if (Board.IsKingAttacked(Board.stm))
                {
                    if (Board.stm == Colour.WHITE)
                    {
                        return -MATEVALUE + ply;
                    }
                    else
                    {
                        return MATEVALUE - ply;
                    }
                }
                else
                {
                    return 0;
                }
            }

            // Evaluate the position if the depth is zero or the search has stopped
            if (depth == 0 || stop)
            {
                positions++;
                return Evaluate.GetEvaluation();
            }

            int moveCount = 0;

            // Alpha-beta pruning works by holding 2 values, an alpha (lower bound), and a beta (top bound)

            // Assume that the higher a score is, the more the position favours white and that the lower a 
            // score is, the more the position favours black

            // The alpha keeps track of the lowest possible score for a position. If a value ends up being smaller than
            // the alpha, then the move is so bad that we would never play it assuming we are white
            // The beta keeps track of the highest possible score for a position. If a value ends up being bigger than
            // the beta, then the move is so good that the opponent would never allow us to play it assuming we are white

            // Assuming a perfect evaluation function, the true value would fall between the alpha score
            // and the beta score.
            if (isWhite)
            {
                foreach (Move move in moves)
                {
                    moveCount++;

                    int reduction = 0;

                    if (moveCount > 3 && depth >= 2 && !Convert.ToBoolean(move.Flags & (int)MoveType.CAPTURE) && !Convert.ToBoolean(move.Flags & (int)MoveType.CHECK))
                    {
                        reduction++;
                    }

                    Board.MakeMove(move);

                    // Do the same search but lower the depth by 1 + the amount of reduction
                    int score = AlphaBeta(alpha, beta, Math.Max(0, depth - reduction - 1), ply + 1, false);

                    Board.UnmakeMove(move);

                    alpha = Math.Max(score, alpha);

                    // (Assuming we are white) If the score is higher or equal to beta, the opponent
                    // would never allow us to play this move. This means we no longer have to keep
                    // searching as the opponent has a better move
                    if (score >= beta)
                    {
                        return beta;
                    }
                }

                return alpha;
            } 
            else
            {
                foreach (Move move in moves)
                {
                    moveCount++;

                    int reduction = 0;

                    if (moveCount > 3 && depth >= 2 && !Convert.ToBoolean(move.Flags & (int)MoveType.CAPTURE) && !Convert.ToBoolean(move.Flags & (int)MoveType.CHECK))
                    {
                        reduction++;
                    }

                    Board.MakeMove(move);

                    // Do the same search but lower the depth by 1 + the amount of reduction
                    int score = AlphaBeta(alpha, beta, Math.Max(0, depth - reduction - 1), ply + 1, true);

                    Board.UnmakeMove(move);

                    beta = Math.Min(score, beta);

                    // (Assuming we are white) If the score is lower or equal to alpha, we
                    // would never play this move. This means we no longer have to keep
                    // searching as we have a better move
                    if (score <= alpha)
                    {
                        return alpha;
                    }
                }

                return beta;
            }
        }

        public static void SearchRoot(int depth, bool makeMove)
        {
            positions = 0;
            int bestEvaluation;

            if (Board.stm == Colour.WHITE)
            {
                bestEvaluation = -MATEVALUE;
            }
            else
            {
                bestEvaluation = MATEVALUE;
            }

            List<Move> moves = MoveGen.Generate();

            MoveGen.SortMoves(moves);

            Move? bestMove = null;

            // Loop through each move
            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int evaluation = AlphaBeta(-MATEVALUE, MATEVALUE, depth - 1, 1, Board.stm == Colour.WHITE);
                Board.UnmakeMove(move);

                // If the evaluation is more than the best evaluation and the
                // side to move is white, update the best evaluation and best move
                if (evaluation > bestEvaluation && Board.stm == Colour.WHITE)
                {
                    bestEvaluation = evaluation;
                    bestMove = move;
                }

                // If the evaluation is less than the best evaluation and the
                // side to move is black, update the best evaluation and best move
                else if (evaluation < bestEvaluation && Board.stm == Colour.BLACK)
                {
                    bestEvaluation = evaluation;
                    bestMove = move;
                }
            }

            if (moves.Count == 0)
            {
                if (Board.IsKingAttacked(Board.stm))
                {
                    if (Board.stm == Colour.WHITE)
                    {
                        bestEvaluation = -MATEVALUE;
                    }
                    else
                    {
                        bestEvaluation = MATEVALUE;
                    }
                }
                else
                {
                    bestEvaluation = 0;
                }
            }

            // If the search has been stopped in the middle of this
            // function don't display the depth information by returning early
            if (stop)
            {
                return;
            }

            int mateDepth = MATEVALUE - Math.Abs(bestEvaluation);

            // Convert mateDepth from plies to whole moves
            mateDepth = (mateDepth / 2) + 1;

            // Display score information
            Console.Write("info depth " + depth);
            if (Math.Abs(bestEvaluation) < 50000)
            {
                Console.Write(" score cp " + bestEvaluation);
            } 
            else
            {
                Console.Write(" score mate " + mateDepth);
            }

            Console.Write(" nodes " + positions);

            if (bestMove == null)
            {
                return;
            }

            Console.Write(" pv " + Board.GetMoveString(bestMove) + "\n");

            PVMove = bestMove;

            return;
        }

        public static void Iterate(string[] command, bool makeMove = false)
        {
            // Traverse through command and update any parameters
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == "depth")
                {
                    maxDepth = int.Parse(command[++i]);
                    continue;
                }
            }

            // Start at depth 1
            for (int depth = 1; depth < maxDepth + 1; depth++)
            {
                SearchRoot(depth, makeMove);

                if (stop)
                {
                    break;
                }
            }

            // Display bestmove information
            if (PVMove == null)
            {
                Console.WriteLine("bestmove (none)");
                return;
            }
            
            // Display best move
            Console.WriteLine("bestmove " + Board.GetMoveString(PVMove));

            if (makeMove)
            {
                Board.MakeMove(PVMove);
            }

            // Set stop to false in case it has been set to true
            stop = false;
        }
    }
}