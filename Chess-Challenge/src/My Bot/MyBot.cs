using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    // int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    // // int oppScore;
    // // int myScore;
    // public Move Think(Board board, Timer timer)
    // {
    //     int Evaluate() {
    //         int whiteEval = CountMaterial(true);
    //         int blackEval = CountMaterial(false);

    //         int evaluation = whiteEval - blackEval;

    //         int perspective = (board.IsWhiteToMove) ? 1 : -1;

    //         return evaluation * perspective;
    //     }

    //     int CountMaterial(bool color) {
    //         int material = 0;
    //         material += board.GetPieceList(PieceType.Pawn, color).Count * pieceValues[1];
    //         material += board.GetPieceList(PieceType.Knight, color).Count * pieceValues[2];
    //         material += board.GetPieceList(PieceType.Bishop, color).Count * pieceValues[3];
    //         material += board.GetPieceList(PieceType.Rook, color).Count * pieceValues[4];
    //         material += board.GetPieceList(PieceType.Queen, color).Count * pieceValues[5];
    //         return material;
    //     }


    //     int Search (int depth, int alpha, int beta){
    //         if(depth == 0) {
    //             return SearchAllCaptures(alpha, beta);
    //         }

    //         Move[] moves = board.GetLegalMoves();
    //         if(moves.Count() == 0) {
    //             if(board.IsInCheck()) {
    //                 return int.MinValue;
    //             }
    //             return 0;
    //         }

    //         foreach(Move move in moves) {
    //             board.MakeMove(move);
    //             int evaluation = -Search(depth - 1, -beta, -alpha);
    //             board.UndoMove(move);
    //             if(evaluation >= beta) {
    //                 return beta;
    //             }
    //             alpha = Math.Max(alpha, evaluation);
    //         }
    //         return alpha;
    //     }

    //     void OrderMoves(Move[] moves) {
    //         foreach(Move move in moves) {
    //             int moveScoreGuess = 0;
    //             int movePieceType = (int)board.GetPiece(move.StartSquare).PieceType;
    //             int capturedPieceType = (int)board.GetPiece(move.TargetSquare).PieceType;

    //             if(capturedPieceType != (int)PieceType.None) {
    //                 moveScoreGuess = 10 * pieceValues[capturedPieceType] - pieceValues[movePieceType];
    //             }

    //             if(move.IsPromotion) {
    //                 moveScoreGuess += pieceValues[(int)move.PromotionPieceType];
    //             }

    //             if(board.SquareIsAttackedByOpponent(move.TargetSquare)) {
    //                 moveScoreGuess -= pieceValues[(int)move.MovePieceType];
    //             }
    //         }
    //     }

    //     int SearchAllCaptures(int alpha, int beta) {
    //         int evaluation = Evaluate();
    //         if(evaluation >= beta) {
    //             return beta;
    //         }
    //         alpha = Math.Max(alpha, evaluation);

    //         Move[] captureMoves = board.GetLegalMoves();
    //         OrderMoves(captureMoves);

    //         foreach(Move captureMove in captureMoves) {
    //             if(captureMove.IsCapture && board.IsWhiteToMove) {
    //                 board.MakeMove(captureMove);
    //                 evaluation = -SearchAllCaptures(-beta, -alpha);
    //                 board.UndoMove(captureMove);

    //                 if(evaluation >= beta)
    //                     return beta;
    //                 alpha = Math.Max(alpha, evaluation);
    //             }
    //         }
    //         return alpha;
    //     }

    //     int max = int.MinValue;

    //     Move[] allMoves = board.GetLegalMoves();
    //     Move bestMove = allMoves[0];
    //     foreach(Move move in allMoves) {
    //         board.MakeMove(move);
    //         int value = Search(1, int.MinValue, int.MaxValue);
    //         board.UndoMove(move);
    //         if(value > max) {
    //             Console.WriteLine("hit");
    //             max = value;
    //             bestMove = move;
    //         }
    //     }

    //     return bestMove;

    // Piece values: nothing, pawn, knight, bishop, castle, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    // Evaluation weights for different factors
    double materialWeight = 1.0, 
    mobilityWeight = 0.5, 
    kingSafetyWeight = 0.3, 
    capturingWeight = 0.6, 
    distanceWeight = 0.4, 
    repetitionWeight = 0.5;

    Queue<Move> previousMoves = new Queue<Move>();

    public void AddMove(Move move)
    {
        previousMoves.Enqueue(move);
        if (previousMoves.Count > 2) previousMoves.Dequeue();
    }

    public Move Think(Board board, Timer timer)
    {
        int maxDepth = 3;
        
        Move bestMove = Move.NullMove;
        int bestScore = int.MinValue;

        List<Move> orderedMoves = OrderMoves(board.GetLegalMoves());

        // Iterative Deepening
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            foreach (Move move in orderedMoves)
            {
                // Play immediate checkmate if possible
                if (board.IsInCheckmate())
                {
                    AddMove(move);
                    return move;
                }

                // Update evaluation weights based on the game phase
                double weight = board.GetAllPieceLists().Sum(pl => pl.Count) <= 12 ? kingSafetyWeight : (board.GetAllPieceLists().Sum(pl => pl.Count) <= 24 ? mobilityWeight : materialWeight);

                // Evaluate the move using the advanced evaluation function and minimax
                board.MakeMove(move);
                int score = (int)(EvaluatePiece(board, board.GetPiece(move.TargetSquare)) * weight) + Minimax(board, depth - 1, false); // Use minimax for deeper evaluations
                board.UndoMove(move);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
        }
        AddMove(bestMove);
        return bestMove;
    }

    private (Move, int) AlphaBeta(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        if (depth == 0 || board.IsInCheckmate())
        {
            int score = 0;

            foreach (Piece piece in GetPieces(board))
            {
                score += EvaluatePiece(board, piece);
            }

            return (Move.NullMove, score);
        }

        Move bestMove = Move.NullMove;
        int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;
        List<Move> moves = OrderMoves(board.GetLegalMoves());

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = AlphaBeta(board, depth - 1, alpha, beta, !isMaximizingPlayer).Item2;
            board.UndoMove(move);

            if (isMaximizingPlayer)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, score);
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                beta = Math.Min(beta, score);
            }

            if (beta <= alpha)
                break;
        }

        return (bestMove, bestScore);
    }


    private List<Move> OrderMoves(Move[] moves)
    {
        // Basic move ordering (captures first)
        List<Move> orderedMoves = new List<Move>();
        foreach (Move move in moves)
        {
            if (move.IsCapture)
            {
                orderedMoves.Insert(0, move); // Captures at the beginning
            }
            else
            {
                orderedMoves.Add(move); // Non-captures at the end
            }
        }

        return orderedMoves;
    }

    private int Minimax(Board board, int depth, bool isMaximizingPlayer)
    {
        if (depth == 0 || board.IsInCheckmate())
        {
            int score = 0;
            
            foreach (Piece piece in GetPieces(board))
            {
                score += EvaluatePiece(board, piece);
            }
            return score;
        }

        int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;

        foreach (Move move in OrderMoves(board.GetLegalMoves()))
        {
            board.MakeMove(move);
            int score = Minimax(board, depth - 1, !isMaximizingPlayer);
            board.UndoMove(move);

            if (isMaximizingPlayer)
            {
                bestScore = System.Math.Max(bestScore, score);
            }
            else
            {
                bestScore = System.Math.Min(bestScore, score);
            }
        }

        return bestScore;
    }

    private int EvaluatePiece(Board board, Piece piece)
    {
        int materialScore = 0;
        int mobilityScore = 0;
        int kingSafetyScore = 0;
        int capturingScore = 0;
        int distanceScore = 0;
        int repetitionScore = 0;

        // Evaluate material count
        int pieceValue = pieceValues[(int)piece.PieceType];
        materialScore += GetPieceCount(board, piece.PieceType, true) * pieceValue;
        materialScore -= GetPieceCount(board, piece.PieceType, false) * pieceValue;

        // For each piece, calculate the number of legal moves it can make
        // and add a mobility bonus based on the number of moves
        foreach (Move move in board.GetLegalMoves())
        {
            if (move.StartSquare == piece.Square)
            {
                mobilityScore += (piece.IsWhite ? 1 : -1) * pieceValue;
            }
        }

        // Evaluate king safety
        if (board.IsInCheck())
        {
            // Penalize the side whose king is in check
            kingSafetyScore += board.IsWhiteToMove ? -1 : 1;
        }

        // Evaluate capture score
        foreach (Move move in board.GetLegalMoves())
        {
            if (move.IsCapture && move.StartSquare == piece.Square)
            {
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
                capturingScore += capturedPieceValue;
            }
        }

        // Evaluate distance score
        foreach (Move move in board.GetLegalMoves())
        {
            if (move.StartSquare == piece.Square)
            {
                int distance = Math.Abs(move.StartSquare.File - move.TargetSquare.File) + Math.Abs(move.StartSquare.Rank - move.TargetSquare.Rank);
                distanceScore += distance;
            }
        }

        // Evaluate repetition score
        foreach (Move previousMove in previousMoves)
        {
            if (previousMove.StartSquare == piece.Square)
            {
                repetitionScore++;
            }
        }

        // Combine all the scores with the corresponding weights
        int finalScore = (int)(materialWeight * materialScore +
                              mobilityWeight * mobilityScore +
                              kingSafetyWeight * kingSafetyScore +
                              capturingWeight * capturingScore +
                              distanceWeight * distanceScore +
                              repetitionWeight * repetitionScore);

        return finalScore;
    }

    // Helper method to get the count of pieces of a given type and color
    private int GetPieceCount(Board board, PieceType pieceType, bool isWhite)
    {
        return GetPieces(board).Count(piece => piece.IsWhite == isWhite && piece.PieceType == pieceType);
    }

    private IEnumerable<Piece> GetPieces(Board board)
    {
        return board.GetAllPieceLists()
                    .SelectMany(pieceList => pieceList)
                    .Where(piece => piece != null);
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
    // }
}