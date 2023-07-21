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
        bool playerIsWhite = board.IsWhiteToMove;

        Move[] moves = board.GetLegalMoves();

        int bestScore = int.MinValue;
        Move bestMove = moves[0];

        foreach (Move move in moves)
        {
            
            board.MakeMove(move);
            Board newBoard = new(board.board);

            // evaluate position
            PieceList[] pieceLists = newBoard.GetAllPieceLists();
            int netScore = pieceLists.Sum(p => (p.IsWhitePieceList == playerIsWhite ? 1 : -1) * chessPieceValues[p.TypeOfPieceInList] * p.Count);
            Console.WriteLine(string.Join(",", pieceLists.Select(p => chessPieceValues[p.TypeOfPieceInList])));

            Console.WriteLine(move.ToString() + " " + netScore);
            if (netScore > bestScore)
            {
                bestScore = netScore;
                bestMove = move;
            }

            board.UndoMove(move);
        }
        Console.WriteLine("----------");

        return bestMove;
    }
}