using UnityEngine;

public static class FenUtility
{
    public const string StartPosFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static void LoadPositionFromFen(string fen)
    {
        Board.ClearBoard();

        string[] parts = fen.Split(' ');

        LoadPieces(parts[0]);
        LoadColorToMove(parts[1][0]);
        LoadCastleRights(parts[2]);
        LoadEnPassantFile(parts[3]);

        Board.SaveGameState();

        //TODO: Move counters
    }

    private static void LoadPieces(string pieceString)
    {
        int file = 0;
        int rank = 7;

        foreach (char symbol in pieceString)
        {
            if (symbol == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if (char.IsDigit(symbol)) file += (int)char.GetNumericValue(symbol);
                else
                {
                    int pieceColor = char.IsUpper(symbol) ? Piece.White : Piece.Black;
                    int pieceType = 0;

                    switch (char.ToLower(symbol))
                    {
                        case 'k':
                            pieceType = Piece.King;
                            break;
                        case 'p':
                            pieceType = Piece.Pawn;
                            break;
                        case 'n':
                            pieceType = Piece.Knight;
                            break;
                        case 'b':
                            pieceType = Piece.Bishop;
                            break;
                        case 'r':
                            pieceType = Piece.Rook;
                            break;
                        case 'q':
                            pieceType = Piece.Queen;
                            break;
                        default:
                            continue;
                    }

                    //Board.Squares[BoardHelper.CoordToIndex(file, rank)] = pieceType | pieceColor;
                    Board.AddPiece(BoardHelper.CoordToIndex(file, rank), pieceType | pieceColor);
                    file++;
                }
            }
        }
    }

    private static void LoadColorToMove(char colorToMoveChar)
    {
        Board.SetColorToMove(colorToMoveChar == 'w' ? Piece.White : Piece.Black);
    }

    private static void LoadCastleRights(string castleRightsString)
    {
        Board.currentGameState &= ~Board.castleRightsMask; //Inverts castle mask and and's it with the current state to only turn off all castle rights

        if (castleRightsString == "-") return;

        uint castleRights = 0;

        foreach (char symbol in castleRightsString)
        {
            switch (symbol)
            {
                case 'K':
                    castleRights |= 0b0001;
                    break;
                case 'k':
                    castleRights |= 0b0010;
                    break;
                case 'Q':
                    castleRights |= 0b0100;
                    break;
                case 'q':
                    castleRights |= 0b1000;
                    break;
                default:
                    Debug.LogError("Invalid Castle Rights in Fen");
                    continue;
            }
        }

        Board.currentGameState |= castleRights << 9;
    }

    private static void LoadEnPassantFile(string epString)
    {
        Board.currentGameState &= ~Board.epFileMask; //Inverts ep mask and turns off all ep file bits

        if (epString == "-") return;

        int file = BoardHelper.FileFromString(epString) + 1;

        Board.currentGameState |= (uint)(file << 5);
    }
}