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
        { PieceType.King, 0 }
    };

    public Move Think(Board board, Timer timer)
    {
        evals = 0;
        int depth = 5;
        bool playerIsWhite = board.IsWhiteToMove;

        Move[] legalMoves = board.GetLegalMoves();
        Move[] orderedMoves = orderMoves(board, legalMoves);
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
            board.MakeMove(move1);
            bool isInCheckAfterMove1 = board.IsInCheck();
            board.UndoMove(move1);

            board.MakeMove(move2);
            bool isInCheckAfterMove2 = board.IsInCheck();
            board.UndoMove(move2);

            // Prioritize moves that do not result in check
            if (isInCheckAfterMove1 && !isInCheckAfterMove2)
                return 1;
            if (!isInCheckAfterMove1 && isInCheckAfterMove2)
                return -1;

            // Prioritize capturing moves
            Piece capturedPiece1 = board.GetPiece(move1.TargetSquare);
            Piece capturedPiece2 = board.GetPiece(move2.TargetSquare);

            if (!capturedPiece1.IsNull && capturedPiece2.IsNull)
                return -1;
            if (capturedPiece1.IsNull && !capturedPiece2.IsNull)
                return 1;

            // If both moves capture, prioritize capturing higher value pieces
            if (!capturedPiece1.IsNull && !capturedPiece2.IsNull)
            {
                int valueDifference = chessPieceValues[capturedPiece1.PieceType] - chessPieceValues[capturedPiece2.PieceType];
                if (valueDifference != 0)
                    return -valueDifference;
            }

            // If no difference so far, use arbitrary but deterministic order
            return move1.ToString().CompareTo(move2.ToString());
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