
public static class ASCIIBoardDrawer
{
    public static void DrawBoard(Board board, ulong bitBoard = 0UL) //TODO: Add line for zobrist hash
    {
        Console.WriteLine("+---+---+---+---+---+---+---+---+");

        for (int rank = 7; rank >= 0; rank--)
        {
            string middle = "|";

            for (int file = 0; file < 8; file++)
            {
                int square = BoardHelper.CoordToIndex(file, rank);

                if (!BitBoardHelper.ContainsSquare(bitBoard, square)) middle += " " + GetPieceEmoji(board, square) + " |";
                else
                {
                    char emoji = GetPieceEmoji(board, square);
                    if (emoji == ' ') emoji = '=';

                    middle += "<" + emoji + ">|";
                }
            }
            Console.WriteLine(middle + ' ' + (rank + 1));

            Console.WriteLine("+---+---+---+---+---+---+---+---+");
        }
        Console.WriteLine("  a   b   c   d   e   f   g   h");
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