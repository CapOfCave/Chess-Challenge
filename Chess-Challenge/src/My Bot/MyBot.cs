using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    static int evals = 0;
    static readonly Dictionary<PieceType, int> chessPieceValues = new Dictionary<PieceType, int>
    {
        { PieceType.Pawn, 1 },
        { PieceType.Knight, 3 },
        { PieceType.Bishop, 3 },
        { PieceType.Rook, 5 },
        { PieceType.Queen, 9 },
        { PieceType.King, 0 },
        { PieceType.None, 0 }
    };

    public Move Think(Board board, Timer timer)
    {
        evals = 0;
        int depth = 7;
        bool playerIsWhite = board.IsWhiteToMove;

        Move[] legalMoves = board.GetLegalMoves();
        Move[] orderedMoves = orderMoves(board, legalMoves);
        Console.WriteLine(String.Join(Environment.NewLine, legalMoves));
        Move bestMove = legalMoves[0];

        int alpha = int.MinValue;
        int beta = int.MaxValue;

        bool maximize = playerIsWhite;

        int bestValue = maximize ? int.MinValue + 1 : int.MaxValue;

        foreach (Move move in orderedMoves)
        {
            board.MakeMove(move);
            if (maximize)
            {
                int moveValue = minmax(board, depth - 1, alpha, beta, false);
                if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    bestMove = move;
                }
                // if we're the maximizing player and we see that we can get a new highest value here...
                alpha = Math.Max(alpha, bestValue);
                // and this highest value is more than the minimum the other can enforce


                if (beta <= alpha)
                {
                    // we don't need to consider this branch as the minimizing player would have chosen a better one before
                    board.UndoMove(move);
                    break;
                }
            }
            else
            {
                int moveValue = minmax(board, depth - 1, alpha, beta, true);
                if (moveValue < bestValue)
                {
                    bestValue = moveValue;
                    bestMove = move;

                }
                beta = Math.Min(beta, bestValue);
                if (beta <= alpha)
                {
                    board.UndoMove(move);
                    break;
                }
            }
            board.UndoMove(move);
        }
        Console.WriteLine("Evals: " + evals);

        return bestMove;
    }

    private Move[] orderMoves(Board board, Move[] legalMoves)
    {
        Array.Sort(legalMoves, (move1, move2) =>
        {
            if (move1.IsCapture && !move2.IsCapture) return -1;
            if (move2.IsCapture && !move1.IsCapture) return 1;

            // Prioritize capturing moves with the most value difference
            int valueDifference1 = chessPieceValues[move1.CapturePieceType] - chessPieceValues[move1.MovePieceType];
            int valueDifference2 = chessPieceValues[move2.CapturePieceType] - chessPieceValues[move2.MovePieceType];

            // sort the moves by highest value difference (highest first)
            return valueDifference2 - valueDifference1;

        });
        return legalMoves;
    }

    /**
     * Alpha: Highest value that white can guarantee
     * Beta: Lowest value that black can guarantee
     */
    private int minmax(Board board, int depth, int alpha, int beta, bool maximize)
    {
        //Console.WriteLine(string.Concat(Enumerable.Repeat("  ", 5 - depth)) + "minmax a=" + alpha + " b=" + beta);
        Move[] moves = board.GetLegalMoves();
        if (depth == 0 || moves.Length == 0)
        {
            return Evaluate(board);
        }

        Move[] orderedMoves = orderMoves(board, moves);
        int bestValue = maximize ? int.MinValue + 1 : int.MaxValue;

        foreach (Move move in orderedMoves)
        {
            board.MakeMove(move);
            if (maximize)
            {
                int moveValue = minmax(board, depth - 1, alpha, beta, false);
                bestValue = Math.Max(bestValue, moveValue);
                // if we're the maximizing player and we see that we can get a new highest value here...
                 alpha = Math.Max(alpha, bestValue);
                // and this highest value is more than the minimum the other can enforce
                if (beta <= alpha)
                {
                    // we don't need to consider this branch as the minimizing player would have chosen a better one before
                    board.UndoMove(move);
                    break;
                }
            }
            else
            {
                int moveValue = minmax(board, depth - 1, alpha, beta, true);
                bestValue = Math.Min(bestValue, moveValue);
                beta = Math.Min(beta, bestValue);
                if (beta <= alpha)
                {
                    board.UndoMove(move);
                    break;
                }
            }
            board.UndoMove(move);
        }
        return bestValue;

    }

    /**
     * Evaluate the position. Positive: White advantage, Negative: Black advantage
     */
    private static int Evaluate(Board board)
    {
        evals++;
        if (board.IsInCheckmate())
        {
            return board.IsWhiteToMove ? int.MinValue + 1 : int.MaxValue;
        }
        PieceList[] pieceLists = board.GetAllPieceLists();
        int netScore = pieceLists.Sum(p => (p.IsWhitePieceList ? 1 : -1) * chessPieceValues[p.TypeOfPieceInList] * p.Count);
        return netScore;
    }
}