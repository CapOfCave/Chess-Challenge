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

        (int eval, Move bestMove) = MiniMax(board, depth, int.MinValue, int.MaxValue, playerIsWhite);

        Console.WriteLine($"Current eval: {eval}; number of evaluations: " + evals);
        return bestMove;
    }

    private Move[] OrderMoves(Board board, Move[] legalMoves)
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
    private (int, Move) MiniMax(Board board, int depth, int alpha, int beta, bool maximize)
    {
        //Console.WriteLine(string.Concat(Enumerable.Repeat("  ", 5 - depth)) + "minmax a=" + alpha + " b=" + beta);
        Move[] moves = board.GetLegalMoves();
        if (depth == 0 || moves.Length == 0)
        {
            return (Evaluate(board), Move.NullMove);
        }

        OrderMoves(board, moves);
        Move bestMove = moves[0];

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            (int moveValue, _) = MiniMax(board, depth - 1, alpha, beta, !maximize);
            if (maximize && moveValue > alpha)
            {
                alpha = moveValue;
                bestMove = move;

            }
            else if (!maximize && moveValue < beta)
            {
                beta = moveValue;
                bestMove = move;
            }
            board.UndoMove(move);
            if (beta <= alpha)
            {
                // we don't need to consider this branch as the other player would have chosen a better one before 
                break;
            }
        }
        return (maximize ? alpha : beta, bestMove);

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