using System;

public static class FenUtility
{
    public const string StartPosFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";


    #region FenToBoard
    public static void LoadPositionFromFen(Board board, string fen)
    {
        if (fen == "startpos") fen = StartPosFen;

        board.ResetBoard(); //TODOnt: Can prob always assume board is already reset?

        string[] parts = fen.Split(' ');

        LoadPieces(board, parts[0]);
        LoadColorToMove(board, parts[1][0]);
        LoadCastleRights(board, parts[2]);
        LoadEnPassantFile(board, parts[3]);

        board.currentZobrist = Zobrist.Hash(board);

        board.SaveGameState();

        //TODO: Move counters
    }

    private static void LoadPieces(Board board, string pieceString)
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
                    board.AddPiece(BoardHelper.CoordToIndex(file, rank), pieceType | pieceColor);
                    file++;
                }
            }
        }
    }

    private static void LoadColorToMove(Board board, char colorToMoveChar)
    {
        board.SetColorToMove(colorToMoveChar == 'w' ? Piece.White : Piece.Black);
    }

    private static void LoadCastleRights(Board board, string castleRightsString)
    {
        board.currentGameState &= ~Board.castleRightsMask; //Inverts castle mask and and's it with the current state to only turn off all castle rights

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
                    Console.WriteLine("Invalid Castle Rights in Fen");
                    continue;
            }
        }

        board.currentGameState |= castleRights << 9;
    }

    private static void LoadEnPassantFile(Board board, string epString)
    {
        board.currentGameState &= ~Board.epFileMask; //Inverts ep mask and turns off all ep file bits

        if (epString == "-") return;

        int file = BoardHelper.FileFromString(epString) + 1;

        board.currentGameState |= (uint)(file << 5);
    }
    #endregion


    #region BoardToFen
    public static string GetCurrentFen(Board board)
    {
        string fen = "";
        for (int rank = 7; rank >= 0; rank--)
        {
            int numEmptyFiles = 0;
            for (int file = 0; file < 8; file++)
            {
                int i = rank * 8 + file;
                int piece = board.Squares[i];
                if (piece != 0)
                {
                    if (numEmptyFiles != 0)
                    {
                        fen += numEmptyFiles;
                        numEmptyFiles = 0;
                    }

                    bool isBlack = Piece.Color(piece) == Piece.Black;

                    int pieceType = Piece.Type(piece);
                    char pieceChar = ' ';

                    switch (pieceType)
                    {
                        case Piece.Rook:
                            pieceChar = 'R';
                            break;
                        case Piece.Knight:
                            pieceChar = 'N';
                            break;
                        case Piece.Bishop:
                            pieceChar = 'B';
                            break;
                        case Piece.Queen:
                            pieceChar = 'Q';
                            break;
                        case Piece.King:
                            pieceChar = 'K';
                            break;
                        case Piece.Pawn:
                            pieceChar = 'P';
                            break;
                    }
                    fen += (isBlack) ? pieceChar.ToString().ToLower() : pieceChar.ToString();
                }
                else
                {
                    numEmptyFiles++;
                }

            }
            if (numEmptyFiles != 0)
            {
                fen += numEmptyFiles;
            }
            if (rank != 0)
            {
                fen += '/';
            }
        }

        // Side to move
        fen += ' ';
        fen += (board.colorToMove == Piece.White) ? 'w' : 'b';

        // Castling
        uint castleRight = (board.currentGameState & Board.castleRightsMask) >> 9;

        bool whiteKingside = (castleRight & 0b0001) != 0;
        bool blackKingside = (castleRight & 0b0010) != 0;
        bool whiteQueenside = (castleRight & 0b0100) != 0;
        bool blackQueenside = (castleRight & 0b1000) != 0;
        fen += ' ';
        fen += (whiteKingside) ? "K" : "";
        fen += (whiteQueenside) ? "Q" : "";
        fen += (blackKingside) ? "k" : "";
        fen += (blackQueenside) ? "q" : "";
        fen += (castleRight == 0) ? "-" : "";

        // En-passant
        fen += ' ';
        int epFile = (int)(board.currentGameState & Board.epFileMask)>>5;
        if (epFile == 0)
        {
            fen += '-';
        }
        else
        {
            string fileName = BoardHelper.fileNames[epFile - 1].ToString();
            int epRank = (board.colorToMove == Piece.White) ? 6 : 3;
            fen += fileName + epRank;
        }

        // 50 move counter
        fen += ' ';
        fen += 1;//board.fiftyMoveCounter; TODO: Fifty move counter

        // Full-move count (should be one at start, and increase after each move by black)
        fen += ' ';
        fen += 1; //(board.plyCount / 2) + 1; TODO: This maybe

        return fen;
    }
    #endregion
}