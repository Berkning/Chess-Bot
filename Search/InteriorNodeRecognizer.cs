
public static class InteriorNodeRecognizer
{
    public static bool IsDraw(Board board)
    {
        //If there are any pawns left we cannot assume the position is a draw purely by material count
        if (board.GetPieceList(Piece.Pawn, 0).Count != 0 || board.GetPieceList(Piece.Pawn, 1).Count != 0) return false;

        //Same thing with rooks
        if (board.GetPieceList(Piece.Rook, 0).Count != 0 || board.GetPieceList(Piece.Rook, 1).Count != 0) return false;

        //Same thing with queens
        if (board.GetPieceList(Piece.Queen, 0).Count != 0 || board.GetPieceList(Piece.Queen, 1).Count != 0) return false;


        //Following this: https://lichess.org/study/qndf6LOy/stthmsBq

        int whiteKnightCount = board.GetPieceList(Piece.Knight, 0).Count;
        int blackKnightCount = board.GetPieceList(Piece.Knight, 1).Count;

        int whiteBishopCount = board.GetPieceList(Piece.Bishop, 0).Count;
        int blackBishopCount = board.GetPieceList(Piece.Bishop, 1).Count;

        int totalKnights = whiteKnightCount + blackKnightCount;
        int totalBishops = whiteBishopCount + blackBishopCount;

        int totalCount = totalKnights + totalBishops;

        if (totalCount > 2) return false;


        if (whiteBishopCount == 2 || blackBishopCount == 2) return false;
        if ((whiteBishopCount == 1 && blackKnightCount == 1) || (whiteKnightCount == 1 && blackBishopCount == 1)) return false;

        return true;
    }
}