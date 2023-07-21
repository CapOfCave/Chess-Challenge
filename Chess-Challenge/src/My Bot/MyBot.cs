using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
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
        int depth = 3;
        bool playerIsWhite = board.IsWhiteToMove;

        int bestValue = int.MinValue;
        Move[] legalMoves = board.GetLegalMoves();
        Move bestMove = legalMoves[0];
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            int score = minmax(board, !playerIsWhite, depth - 1) * (playerIsWhite ? 1 : -1);

            if (score > bestValue)
            {
                bestMove = move;
                bestValue = score;
            }
            board.UndoMove(move);
        }

        return bestMove;
    }
    
    private int minmax(Board board, bool maximize, int depth)
    {
        if (depth == 0)
        {
            return Evaluate(board);
        }
        Move[] moves = board.GetLegalMoves();

        int value = maximize ? int.MinValue : int.MaxValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);

            int moveValue = minmax(board, !maximize, depth - 1);

            if (maximize)
            {
                value = Math.Max(value, moveValue);
            } else
            {
                value = Math.Min(value, moveValue);
            }
            board.UndoMove(move);

        }
        return value;        

    }

    /**
     * Evaluate the position. Positive: White advantage, Negative: Black advantage
     */
    private static int Evaluate(Board board)
    {
        Board newBoard = new Board(new ChessChallenge.Chess.Board(board.board));
        PieceList[] pieceLists = newBoard.GetAllPieceLists();
        int netScore = pieceLists.Sum(p => (p.IsWhitePieceList ? 1 : -1) * chessPieceValues[p.TypeOfPieceInList] * p.Count);
        return netScore;
    }
}