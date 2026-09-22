
//TODO: make this a struct instead - not sure if even benefitial?
public class PieceList
{
    private Board board;

    public int[] occupiedSquares; //Long as count
    public ulong[] attackMaps; //Semi-legal attack maps //Long as count //ONLY used for knights, bishops, rooks and queens //Overlaps both friendly and enemy pieces as if they were all captureable

    private int[] indexMap; //64 length
    private int numPieces;

    public ulong bitboard;
    public ulong attackMap; //Combined map of all pieces attacks in the piecelist //ONLY used for knights, bishops, rooks and queens //Overlaps both friendly and enemy pieces as if they were all captureable


    public int Count { get { return numPieces; } }

    public int this[int index] => occupiedSquares[index];


    private bool updateAttackMaps;
    private int pieceType; //TODO: Can store as byte
    private bool isWhite;



    public PieceList(Board _board, int _piece, int maxPieceCount = 10) //One side can never have more than 10 of the same piece-type
    {
        board = _board;
        occupiedSquares = new int[maxPieceCount];
        attackMaps = new ulong[maxPieceCount];
        indexMap = new int[64];
        numPieces = 0;
        bitboard = 0;
        attackMap = 0;
        pieceType = Piece.Type(_piece);
        updateAttackMaps = Piece.IsSlidingPiece(pieceType);
        isWhite = Piece.Color(_piece) == Piece.White;
    }

    public void AddPieceAtSquare(int square)
    {
        occupiedSquares[numPieces] = square;
        indexMap[square] = numPieces;
        bitboard ^= 1UL << square;

        if (updateAttackMaps) UpdateAttackMap(numPieces);

        numPieces++;
    }

    public void RemovePieceAtSquare(int square)
    {
        numPieces--;
        int removedPieceIndex = indexMap[square];
        occupiedSquares[removedPieceIndex] = occupiedSquares[numPieces];
        indexMap[occupiedSquares[removedPieceIndex]] = removedPieceIndex;
        bitboard ^= 1UL << square;

        if (updateAttackMaps)
        {
            attackMaps[removedPieceIndex] = attackMaps[numPieces];
            RecreateCombinedAttackMap();
        }

    }

    public void Clear()
    {
        while (numPieces > 0)
        {
            int square = occupiedSquares[0];
            RemovePieceAtSquare(square);
        }

        attackMap = 0UL; //Not technically necessary but for good measure ig
    }

    public void MovePiece(int startSquare, int targetSquare)
    {
        int index = indexMap[startSquare];
        occupiedSquares[index] = targetSquare;
        indexMap[targetSquare] = index;
        bitboard ^= (1UL << startSquare) | (1UL << targetSquare);

        /*if (updateAttackMaps)
        {
            UpdateAttackMap(index);
            RecreateCombinedAttackMap();
        }*/
    }


    public void UpdateAttackMap(int index)
    {
        ulong attackBoard = 0;

        ulong blockerBoard = board.allPieceBoard;

        //Remove enemy king from blockerlist (because allows passthrough rays so king doesn't walk backwards while still in view of a sliding piece)
        //TODO: move into switch bc doesn't apply to horses
        //This also isn't necessary at all if using pseudo-legal movegen, bc uses separate "in-check" check
        if (isWhite) blockerBoard ^= 1UL << board.blackKingSquare;
        else blockerBoard ^= 1UL << board.whiteKingSquare;

        switch (pieceType)
        {
            case Piece.Queen:
                attackBoard = MagicData.GetRookMoveBoard(blockerBoard, occupiedSquares[index]);
                attackBoard |= MagicData.GetBishopMoveBoard(blockerBoard, occupiedSquares[index]);
                break;
            case Piece.Rook:
                attackBoard = MagicData.GetRookMoveBoard(blockerBoard, occupiedSquares[index]);
                break;
            case Piece.Bishop:
                attackBoard = MagicData.GetBishopMoveBoard(blockerBoard, occupiedSquares[index]);
                break;
            default:
                Console.WriteLine("UpdateAttackMap wrongfully called in PieceList on piece of type: " + pieceType);
                break;
        }

        attackMaps[index] = attackBoard;
        attackMap |= attackBoard;
    }

    public void RecreateCombinedAttackMap()
    {
        attackMap = 0UL;

        for (int i = 0; i < numPieces; i++)
        {
            attackMap |= attackMaps[i];
        }
    }
}