

public static class OpeningBook
{
    public static string bookPath = @"/home/berkning/Documents/OpeningBook.bin";

    private static Entry[] bookEntries;
    private static Random random = new Random();

    private static bool isInitialized = false;


    public static Move GetMove(ulong zobrist) //TODO: http://hgm.nubati.net/book_format.html  "The entries are ordered according to key. Lowest key first. " - Could speed up search an insane amount - doesn't seem to be the case with current book
    {
        int startIndex = random.Next() % bookEntries.Length; //Random starting index to search from
        int direction = random.Next() % 2 == 1 ? -1 : 1; //Randomly picks whether to search up or down from starting index, based on whether the random number turns out even or odd

        int checkedEntryCount = 0;

        int tries = 0;

        while (tries < 2)
        {
            tries++;

            for (int i = startIndex; i >= 0 && i < bookEntries.Length; i += direction)
            {
                if (bookEntries[i].key == zobrist)
                {
                    if (bookEntries[i].move == 0)
                    {
                        Console.WriteLine("info string Invalid move in book");
                        continue;
                    }

                    Console.WriteLine("info string Found book move after checking " + checkedEntryCount + " entries");
                    return TranslatePolyglotMove(bookEntries[i].move);
                }

                checkedEntryCount++;
            }

            direction *= -1; //Flip direction
        }

        Console.WriteLine("info string Couldn't find book move after checking " + checkedEntryCount + " entries");
        return Move.nullMove;
    }

    private static Move TranslatePolyglotMove(ushort move)
    {
        int toFile = move & 0b111;
        int toRank = (move & 0b111000) >> 3;

        int fromFile = (move & 0b111000000) >> 6;
        int fromRank = (move & 0b111000000000) >> 9;

        int startSquare = BoardHelper.CoordToIndex(fromFile, fromRank);
        int targetSquare = BoardHelper.CoordToIndex(toFile, toRank);

        int movedPiece = Engine.mainBoard.Squares[startSquare];

        if (Piece.Type(movedPiece) != Piece.Pawn)
        {
            if (Piece.Type(movedPiece) != Piece.King) return new Move(startSquare, targetSquare);

            if (Math.Abs(toFile - fromFile) < 2) return new Move(startSquare, targetSquare); //Not a castling move

            switch (targetSquare)
            {
                case BoardHelper.h1: //White Short
                    return new Move(startSquare, BoardHelper.g1, Move.Flag.Castling);
                case BoardHelper.a1: //White Long
                    return new Move(startSquare, BoardHelper.c1, Move.Flag.Castling);
                case BoardHelper.h8: //Black Short
                    return new Move(startSquare, BoardHelper.g8, Move.Flag.Castling);
                case BoardHelper.a8: //Black Long
                    return new Move(startSquare, BoardHelper.c8, Move.Flag.Castling);
            }

            Console.WriteLine("Error translating polyglot move - assumed was castling move but wasn't -> " + move);
            return Move.nullMove;
        }

        int promotionPiece = (move & 0b111000000000000) >> 12;

        if (promotionPiece == 0)
        {
            if (Math.Abs(fromRank - toRank) == 2) return new Move(startSquare, targetSquare, Move.Flag.PawnTwoForward); //If pawn two forward

            if (fromFile - toFile != 0 && Piece.IsNone(Engine.mainBoard.Squares[targetSquare])) return new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture); //If we moved diagonally, and the target square is empty - must be en passant
        }

        switch (promotionPiece)
        {
            case 1:
                return new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight);
            case 2:
                return new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop);
            case 3:
                return new Move(startSquare, targetSquare, Move.Flag.PromoteToRook);
            case 4:
                return new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen);
        }

        Console.WriteLine("Error translating polyglot move - assumed was promotion but wasn't -> " + move);

        return Move.nullMove;
    }


    public static void Initialize()
    {
        if (isInitialized) return;

        byte[] book = File.ReadAllBytes(bookPath);

        if (book.Length % 16 != 0) Console.WriteLine("Opening book size is irregular");

        int entryCount = book.Length / 16; //Each entry is 16 bytes

        bookEntries = new Entry[entryCount];


        for (int i = 0; i < entryCount; i++)
        {
            int entryIndex = i * 16;

            //ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(book, entryIndex, 8);

            ulong key = ((ulong)book[entryIndex]) << 56 | ((ulong)book[entryIndex + 1]) << 48 | ((ulong)book[entryIndex + 2]) << 40 | ((ulong)book[entryIndex + 3]) << 32 | ((ulong)book[entryIndex + 4]) << 24 | ((ulong)book[entryIndex + 5]) << 16 | ((ulong)book[entryIndex + 6]) << 8 | ((ulong)book[entryIndex + 7]);//BitConverter.ToUInt64(span);
            ushort move = (ushort)((book[entryIndex + 8] << 8) | book[entryIndex + 9]);

            Entry entry = new Entry(key, move);

            bookEntries[i] = entry;
        }

        isInitialized = true;
    }


    public struct Entry
    {
        public ulong key;
        public ushort move;

        public Entry(ulong _key, ushort _move)
        {
            key = _key;
            move = _move;
        }
    }
}