using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    // int oppScore;
    // int myScore;
    public Move Think(Board board, Timer timer)
    {
        int Evaluate() {
            int whiteEval = CountMaterial(true);
            int blackEval = CountMaterial(false);

            int evaluation = whiteEval - blackEval;

            int perspective = (board.IsWhiteToMove) ? 1 : -1;
            return evaluation * perspective;
        }

        int CountMaterial(bool color) {
            int material = 0;
            material += board.GetPieceList(PieceType.Pawn, color).Count * pieceValues[1];
            material += board.GetPieceList(PieceType.Knight, color).Count * pieceValues[2];
            material += board.GetPieceList(PieceType.Bishop, color).Count * pieceValues[3];
            material += board.GetPieceList(PieceType.Rook, color).Count * pieceValues[4];
            material += board.GetPieceList(PieceType.Queen, color).Count * pieceValues[5];
            return material;
        }


        int Search (int depth, int alpha, int beta){
            if(depth == ) {
                return Evaluate();
            }

            Move[] moves = board.GetLegalMoves();
            if(moves.Count() == 0) {
                if(board.IsInCheck()) {
                    return int.MinValue;
                }
                return 0;
            }

            int bestEvaluation = int.MinValue;

            foreach(Move move in moves) {
                board.MakeMove(move);
                int evaluation = -Search(depth - 1, -beta, -alpha);
                board.UndoMove(move);
                if(evaluation >= beta) {
                    return beta;
                }
                alpha = Max(alpha, evaluation);
            }
            return alpha;
        }

































        // PieceList[] currentPieces = board.GetAllPieceLists();
        // int count = 0;
        // foreach(PieceList pieceList in currentPieces) {
        //     if(board.IsWhiteToMove) {
        //         count++;
        //         myScore += pieceValues[(int)pieceList.GetPiece(count).PieceType];
        //         Console.WriteLine("My score: " + myScore);
        //     } else {
        //         count++;
        //         oppScore += pieceValues[(int)pieceList.GetPiece(count).PieceType];
        //         Console.WriteLine("Opponent's score: " + oppScore);
        //     }
        // }
        
        // int highestValueCapture = 0;

        // int alpha = int.MinValue;
        // int beta = int.MaxValue;

        // Move bestMove = board.GetLegalMoves()[0];
        // int depth = 5;
        // Move[] moveList = new Move[depth];
        // Board newBoard;

        // while (depth > 0) {
        //     Move[] moves = board.GetLegalMoves();
        //     foreach (Move move in moves) {
        //         Piece capturedPiece = board.GetPiece(move.TargetSquare);
        //         int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
        //         if (capturedPieceValue > highestValueCapture) {
        //             bestMove = move;
        //             highestValueCapture = capturedPieceValue;
        //         }
        //         Console.WriteLine(bestMove);
        //     }
        //     moveList.Append(bestMove).ToArray();
        //     board.MakeMove(bestMove);
        //     board.MakeMove(board.GetLegalMoves()[0]);
        //     Console.WriteLine(moveList[0]);
        //     depth--;
        // }

        // return moveList[0];

        // void Estimate(Board board, Move move) {
        //     if(board.IsInCheckmate()) {
        //         return;
        //     }

        //     Move[] moves = board.GetLegalMoves();

        //     board.MakeMove(move)
        // }

        // Move moveToPlay = board.GetLegalMoves()[0];

        // int evaluate(Board board) {
        //     Move[] allMoves = board.GetLegalMoves();

        //     int highestValueCapture = 0;

        //     if(board.IsInCheckmate() && board.IsWhiteToMove) {
        //         return -1000;
        //     }
        //     if(board.IsInCheckmate() && !board.IsWhiteToMove) {
        //         return 1000;
        //     }
        //     if(board.IsDraw()) {
        //         return 0;
        //     }

        //     foreach (Move move in allMoves) {
        //         Piece capturedPiece = board.GetPiece(move.TargetSquare);
        //         int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

        //         if (capturedPieceValue > highestValueCapture)
        //         {
        //             highestValueCapture = capturedPieceValue;
        //         }
        //     }

        //     return highestValueCapture;
        // }

        // int score = 0;

        // int maxi(int depth) {
        //     if (depth == 0) {
        //         return evaluate(board);
        //     }
        //     int max = int.MinValue;
        //     foreach (Move move in board.GetLegalMoves()) {
        //         score = mini(depth - 1);
        //         if(score > max) {
        //             max = score;
        //         }
        //     }
        //     return max;
        // }

        // int mini(int depth) {
        //     if (depth == 0) {
        //         return -evaluate(board);
        //     }
        //     int min = int.MaxValue;
        //     foreach (Move move in board.GetLegalMoves()) {
        //         score = maxi(depth - 1);
        //         if(score < min) {
        //             min = score;
        //         }
        //     }
        //     return min;
        // }
        
        // return moveToPlay;
    }
}