
//TODO: make this a struct instead
public class PieceList
{
    public int[] occupiedSquares; //Long as count

    private int[] indexMap; //64 length
    private int numPieces;

    public ulong bitboard;


    public int Count { get { return numPieces; } }

    public int this[int index] => occupiedSquares[index];



    public PieceList(int maxPieceCount = 10) //Dont currently see how one side could have more than 10 of a single piece type
    {
        occupiedSquares = new int[maxPieceCount];
        indexMap = new int[64];
        numPieces = 0;
        bitboard = 0;
    }

    public void AddPieceAtSquare(int square)
    {
        occupiedSquares[numPieces] = square;
        indexMap[square] = numPieces;
        bitboard ^= 1UL << square;
        numPieces++;
    }

    public void RemovePieceAtSquare(int square)
    {
        int removedPieceIndex = indexMap[square];
        occupiedSquares[removedPieceIndex] = occupiedSquares[numPieces - 1];
        indexMap[occupiedSquares[removedPieceIndex]] = removedPieceIndex;
        bitboard ^= 1UL << square;
        numPieces--;
    }

    public void Clear()
    {
        while (numPieces > 0)
        {
            int square = occupiedSquares[0];
            RemovePieceAtSquare(square);
        }
    }

    public void MovePiece(int startSquare, int targetSquare)
    {
        int index = indexMap[startSquare];
        occupiedSquares[index] = targetSquare;
        indexMap[targetSquare] = index;
        bitboard ^= (1UL << startSquare) | (1UL << targetSquare);
    }
}