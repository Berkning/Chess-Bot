
public static class ASCIIBoardDrawer
{
    public static void DrawBoard(Board board)
    {
        Console.WriteLine("+---+---+---+---+---+---+---+---+");

        for (int rank = 7; rank >= 0; rank--)
        {
            string middle = "|";

            for (int file = 0; file < 8; file++)
            {
                middle += " " + GetPieceEmoji(board, BoardHelper.CoordToIndex(file, rank)) + " |";
            }
            Console.WriteLine(middle);

            Console.WriteLine("+---+---+---+---+---+---+---+---+");
        }
    }

    private static char GetPieceEmoji(Board board, int square)
    {
        int piece = board.Squares[square];

        if (Piece.IsNone(piece)) return ' ';

        int type = Piece.Type(piece);

        if (Piece.Color(piece) == Piece.White)
        {
            switch (type)
            {
                case Piece.King:
                    return '♚';
                case Piece.Pawn:
                    return '♟';
                case Piece.Knight:
                    return '♞';
                case Piece.Bishop:
                    return '♝';
                case Piece.Rook:
                    return '♜';
                case Piece.Queen:
                    return '♛';
            }
        }
        else
        {
            switch (type)
            {
                case Piece.King:
                    return '♔';
                case Piece.Pawn:
                    return '♙';
                case Piece.Knight:
                    return '♘';
                case Piece.Bishop:
                    return '♗';
                case Piece.Rook:
                    return '♖';
                case Piece.Queen:
                    return '♕';
            }
        }

        return 'Ø';
    }
}