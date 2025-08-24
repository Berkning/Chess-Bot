using System;
using System.Collections.Generic;
using UnityEngine;

public static class Board
{
    public static int[] Squares { get; private set; }

    public static int colorToMove { get; private set; } = Piece.White;

    public static int friendlyColor { get; private set; } = Piece.White;
    public static int enemyColor { get; private set; } = Piece.Black;

    //0-5 = Captured Piece //TODO: Could be pretty easily reduced by removing the two color bits and infering the color based on colorToMove
    //6-9 = EP file
    //10-13 = Castling Rights - 10 = Wshort, 11 = Bshort, 12 = Wlong, 13 = Blong
    private static Stack<uint> gameStateHistory = new Stack<uint>();
    public static uint currentGameState = 0;//0b1111000000000; //Castles allowed by default 

    public static uint capturedPieceMask = 0b11111;
    public static uint epFileMask = 0b111100000;
    public static uint castleRightsMask = 0b1111000000000;

    //Piece lists
    public static PieceList[] pawnList;
    public static PieceList[] knightList;
    public static PieceList[] bishopList;
    public static PieceList[] rookList;
    public static PieceList[] queenList;

    public static PieceList[] allPieceList;

    public static int whiteKingSquare;
    public static int blackKingSquare;

    public static int opponentColorBit;
    public static int friendlyColorBit;




    private static PieceList GetPieceList(int type, int colorBit)
    {
        return allPieceList[type - 2 + colorBit * 5];
    }

    public static void AddPiece(int square, int piece)
    {
        Squares[square] = piece;

        int type = Piece.Type(piece);

        if (type == Piece.King)
        {
            int color = Piece.Color(piece);
            if (color == Piece.White) whiteKingSquare = square;
            else blackKingSquare = square;

            return;
        }
        else if (type == Piece.None) return; //TODO: prob remove somehow, bc performance

        GetPieceList(Piece.Type(piece), Piece.ColorBit(piece)).AddPieceAtSquare(square);
    }

    public static void MovePiece(int startSquare, int targetSquare) //WARNING: Target square has to be empty!!!!
    {
        int piece = Squares[startSquare];

        GetPieceList(Piece.Type(piece), Piece.ColorBit(piece)).MovePiece(startSquare, targetSquare);

        Squares[targetSquare] = piece;
        Squares[startSquare] = Piece.None;
    }

    public static void RemovePiece(int square)
    {
        int piece = Squares[square];
        Squares[square] = Piece.None;

        GetPieceList(Piece.Type(piece), Piece.ColorBit(piece)).RemovePieceAtSquare(square);
    }



    public static void SetColorToMove(int color)
    {
        colorToMove = color;
        friendlyColor = color;
        enemyColor = Piece.OppositeColor(color);
        friendlyColorBit = Piece.ColorBit(friendlyColor);
        opponentColorBit = friendlyColorBit ^ 1;
    }

    static Board()
    {
        Squares = new int[64];
        gameStateHistory.Push(currentGameState);

        pawnList = new PieceList[2] { new PieceList(8), new PieceList(8) };
        knightList = new PieceList[2] { new PieceList(10), new PieceList(10) };
        bishopList = new PieceList[2] { new PieceList(10), new PieceList(10) };
        rookList = new PieceList[2] { new PieceList(10), new PieceList(10) };
        queenList = new PieceList[2] { new PieceList(9), new PieceList(9) };

        allPieceList = new PieceList[10]
        {
            pawnList[0],
            knightList[0],
            bishopList[0],
            rookList[0],
            queenList[0],
            pawnList[1],
            knightList[1],
            bishopList[1],
            rookList[1],
            queenList[1],
        };
    }



    public static void ClearBoard()
    {
        Array.Clear(Squares, 0, 64);

        foreach (PieceList pieceList in allPieceList)
        {
            pieceList.Clear();
        }
    }

    public static void SaveGameState()
    {
        gameStateHistory.Push(currentGameState);
    }



    public static void MakeMove(Move move)
    {
        if (Squares[move.startSquare] == Piece.None) Debug.Log("Tried to move null piece " + BoardHelper.NameMove(move) + " " + move.flag);

        uint prevGameState = currentGameState;
        uint prevCastleRights = (prevGameState & castleRightsMask) >> 9;
        currentGameState = 0;

        SetColorToMove(Piece.OppositeColor(colorToMove));

        int movedPieceType = Piece.Type(Squares[move.startSquare]);



        if (move.flag == Move.Flag.PawnTwoForward)
        {
            int file = BoardHelper.IndexToFile(move.targetSquare) + 1;
            currentGameState |= (ushort)(file << 5);
        }
        else if (move.flag == Move.Flag.Castling)
        {
            switch (move.targetSquare)
            {
                case BoardHelper.g1:
                    //White Shortcastle
                    prevCastleRights ^= 0b0001;
                    MovePiece(BoardHelper.h1, BoardHelper.f1);
                    break;
                case BoardHelper.g8:
                    //Black Shortcastle
                    prevCastleRights ^= 0b0010;
                    MovePiece(BoardHelper.h8, BoardHelper.f8);
                    break;
                case BoardHelper.c1:
                    //White Longcastle
                    prevCastleRights ^= 0b0100;
                    MovePiece(BoardHelper.a1, BoardHelper.d1);
                    break;
                case BoardHelper.c8:
                    //Black Longcastle
                    prevCastleRights ^= 0b1000;
                    MovePiece(BoardHelper.a8, BoardHelper.d8);
                    break;
            }

        }

        //Add captured piece to gamestate - depends on whether the captured piece is on the square we landed on, or if ep
        if (move.flag == Move.Flag.EnPassantCapture)
        {
            int epFile = (int)((prevGameState & epFileMask) >> 5) - 1;
            int capturedPawnColor = colorToMove; //The color of the captured pawn is the color whos turn it is to move now

            int capturedPawnRank = capturedPawnColor == Piece.White ? 3 : 4;

            int capturedPawnIndex = BoardHelper.CoordToIndex(epFile, capturedPawnRank);

            currentGameState |= (ushort)Squares[capturedPawnIndex]; //Add captured pawn as the captured piece in the gamestate
            RemovePiece(capturedPawnIndex);
        }
        else if (Squares[move.targetSquare] != Piece.None)
        {
            int capturedPiece = Squares[move.targetSquare];

            currentGameState |= (ushort)Squares[move.targetSquare];
            RemovePiece(move.targetSquare);

            if (Piece.Type(capturedPiece) == Piece.Rook)
            {
                if (move.targetSquare == BoardHelper.h1)
                {
                    prevCastleRights &= 0b1110; //Disable white shortcastle
                }
                else if (move.targetSquare == BoardHelper.h8)
                {
                    prevCastleRights &= 0b1101; //Disable black shortcastle
                }
                else if (move.targetSquare == BoardHelper.a1)
                {
                    prevCastleRights &= 0b1011; //Disable white longCastle
                }
                else if (move.targetSquare == BoardHelper.a8)
                {
                    prevCastleRights &= 0b0111; //Disable black longCastle
                }
            }
        }

        currentGameState |= prevCastleRights << 9;


        //Move the piece
        if (movedPieceType == Piece.King)
        {
            Squares[move.targetSquare] = Squares[move.startSquare];
            Squares[move.startSquare] = Piece.None;

            int pieceColor = Piece.Color(Squares[move.targetSquare]);
            if (pieceColor == Piece.White)
            {
                whiteKingSquare = move.targetSquare;
                currentGameState &= ~(0b0101U << 9); //Turn off white castling bc king moved
            }
            else
            {
                blackKingSquare = move.targetSquare;
                currentGameState &= ~(0b1010U << 9); //Turn off black castling bc king moved
            }
        }
        else if (movedPieceType == Piece.Rook)
        {
            //TODO: Could maybe check if castling is even allowed to avoid unnecessary stuff
            if (enemyColor == Piece.White) //Means white played the move
            {
                if (move.startSquare == BoardHelper.h1) //White shortcastle rook
                {
                    //Debug.Log("WShort Disabled");
                    currentGameState &= ~(1U << 9);
                }
                else if (move.startSquare == BoardHelper.a1) //White longcastle rook
                {
                    //Debug.Log("WLong Disabled");
                    currentGameState &= ~(1U << 11);
                }
            }
            else //Means black played the move
            {
                if (move.startSquare == BoardHelper.h8) //Black shortcastle rook
                {
                    //Debug.Log("BShort Disabled");
                    currentGameState &= ~(1U << 10);
                }
                else if (move.startSquare == BoardHelper.a8) //Black longcastle rook
                {
                    //Debug.Log("BLong Disabled");
                    currentGameState &= ~(1U << 12);
                }
            }

            MovePiece(move.startSquare, move.targetSquare);
        }
        else if (move.IsPromotion())
        {
            //pawnList[opponentColorBit].RemovePieceAtSquare(move.startSquare); //Color changed so its the enemys pawn technically which should prob be changed
            RemovePiece(move.startSquare);

            switch (move.flag)
            {
                case Move.Flag.PromoteToQueen:
                    //queenList[opponentColorBit].AddPieceAtSquare(move.targetSquare);
                    AddPiece(move.targetSquare, enemyColor | Piece.Queen);
                    break;
                case Move.Flag.PromoteToKnight:
                    //knightList[opponentColorBit].AddPieceAtSquare(move.targetSquare);
                    AddPiece(move.targetSquare, enemyColor | Piece.Knight);
                    break;
                case Move.Flag.PromoteToBishop:
                    //bishopList[opponentColorBit].AddPieceAtSquare(move.targetSquare);
                    AddPiece(move.targetSquare, enemyColor | Piece.Bishop);
                    break;
                case Move.Flag.PromoteToRook:
                    //rookList[opponentColorBit].AddPieceAtSquare(move.targetSquare);
                    AddPiece(move.targetSquare, enemyColor | Piece.Rook);
                    break;
            }
        }
        else { MovePiece(move.startSquare, move.targetSquare); }

        gameStateHistory.Push(currentGameState);
        //Debug.Log(Convert.ToString(currentGameState, 2));
    }

    public static void UnMakeMove(Move move)
    {
        SetColorToMove(Piece.OppositeColor(colorToMove));
        int movedPieceType = Piece.Type(Squares[move.targetSquare]);

        //Move piece back
        if (movedPieceType == Piece.King)
        {
            Squares[move.startSquare] = Squares[move.targetSquare];

            int pieceColor = Piece.Color(Squares[move.startSquare]);
            if (pieceColor == Piece.White) whiteKingSquare = move.startSquare;
            else blackKingSquare = move.startSquare;
        }
        else if (move.IsPromotion())
        {
            switch (move.flag)
            {
                case Move.Flag.PromoteToQueen:
                    //queenList[friendlyColorBit].RemovePieceAtSquare(move.targetSquare);
                    break;
                case Move.Flag.PromoteToKnight:
                    //knightList[friendlyColorBit].RemovePieceAtSquare(move.targetSquare);
                    break;
                case Move.Flag.PromoteToBishop:
                    //bishopList[friendlyColorBit].RemovePieceAtSquare(move.targetSquare);
                    break;
                case Move.Flag.PromoteToRook:
                    //rookList[friendlyColorBit].RemovePieceAtSquare(move.targetSquare);
                    break;
            }

            RemovePiece(move.targetSquare);

            //pawnList[friendlyColorBit].AddPieceAtSquare(move.startSquare);
            AddPiece(move.startSquare, Piece.Pawn | friendlyColor);
        }
        else MovePiece(move.targetSquare, move.startSquare);
        //Squares[move.startSquare] = Squares[move.targetSquare];

        if (move.flag == Move.Flag.Castling)
        {
            switch (move.targetSquare)
            {
                case BoardHelper.g1:
                    MovePiece(BoardHelper.f1, BoardHelper.h1);
                    break;
                case BoardHelper.g8:
                    MovePiece(BoardHelper.f8, BoardHelper.h8);
                    break;
                case BoardHelper.c1:
                    MovePiece(BoardHelper.d1, BoardHelper.a1);
                    break;
                case BoardHelper.c8:
                    MovePiece(BoardHelper.d8, BoardHelper.a8);
                    break;
            }

        }



        int capturedPiece = (int)(currentGameState & capturedPieceMask);

        if (move.flag == Move.Flag.EnPassantCapture)
        {
            int capturedPieceDirection = colorToMove == Piece.White ? -8 : 8; //Whether the captured pawn is below or above the pawn capturing it (depends on if black/white played the move)

            AddPiece(move.targetSquare + capturedPieceDirection, capturedPiece);
            //Squares[move.targetSquare + capturedPieceDirection] = capturedPiece;

            //Squares[move.targetSquare] = Piece.None;
        }
        else
        {
            AddPiece(move.targetSquare, capturedPiece);
            //Squares[move.targetSquare] = (int)(currentGameState & capturedPieceMask);
        }


        gameStateHistory.Pop();
        currentGameState = gameStateHistory.Peek();
    }
}

public class Piece
{
    public const int None = 0;
    public const int King = 1;
    public const int Pawn = 2;
    public const int Knight = 3;
    public const int Bishop = 4;
    public const int Rook = 5;
    public const int Queen = 6;

    public const int White = 8;
    public const int Black = 16;

    public const int ColorMask = 0b11000;
    public const int TypeMask = 0b00111;

    public static int Color(int piece)
    {
        return piece & ColorMask;
    }

    public static int OppositeColor(int piece)
    {
        return (~piece) & ColorMask;
    }

    public static int ColorBit(int piece)//Returns 0 if white, 1 if black
    {
        return piece >> 4;
    }

    public static int Type(int piece)
    {
        return piece & TypeMask;
    }

    public static bool IsSlidingPiece(int piece)
    {
        return (piece & TypeMask) > 3;
    }

    public static bool IsNone(int piece)
    {
        return (piece & TypeMask) == None;
    }

    public static bool IsBishopOrQueen(int piece)
    {
        //return ((piece ^ 0b001) & 0b100) == 0b101;
        int test = piece & TypeMask;
        return test == Bishop || test == Queen;
    }

    public static bool IsRookOrQueen(int piece)
    {
        return (piece & TypeMask) > 4;
    }
}