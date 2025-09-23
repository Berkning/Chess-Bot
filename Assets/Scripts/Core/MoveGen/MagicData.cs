using System.Collections.Generic;

public static class MagicData
{
    public static readonly ulong[] rookMagics = {
    12387163943325714576, 8060135981791801306, 6838921365805030679, 15327074671092356062,
    8948049086171909015, 8646981809803877574, 7545642128380474600, 7980450526490072013,
    11715410778596147490, 7479458305361812616, 1871454031738769090, 13573286386930301440,
    7724946211198932102, 7645319557910108211, 16160041566520750640, 10880133790728062420,
    6107617767090892192, 12880232060367790047, 8087069952142450221, 4395840891053523456,
    288760341569608192, 13059676328911965845, 14168768631245647152, 939117070694551172,
    9887882603584639365, 12820027657840925338, 14791953760045788456, 14745971759249649679,
    9985835543436918851, 7622819899625189296, 17590682685604044304, 2613055963992949093,
    1819843831251075605, 17743828777381919070, 5345844784323891426, 12342045819866175293,
    3150228046763968727, 12836279452350456922, 2818846827342729224, 11103359662790022796,
    1217168234682744834, 6349368110768507596, 9353215517810256462, 6700970598568427528,
    425543942691356986, 299690853396682071, 13760457498652573720, 13630614960350035969,
    6346277791530697216, 4621779016838939136, 3450295435665920, 3460666676999631360,
    7074447992047099392, 15809724798878328364, 5131972077828451328, 7914948840787461632,
    5636942678198345987, 254911904328319154, 12451059890057788723, 5262949047800561702,
    5199969201000851458, 14073467533597081611, 5601119559307888676, 3350305420536564742
    };
    public static readonly int[] rookShifts = {
    51, 52, 52, 52, 52, 52, 52, 51, 53, 53, 53, 53, 53, 53, 54, 53, 52, 53, 53, 54, 54,
    53, 54, 53, 52, 53, 53, 53, 53, 53, 54, 53, 52, 53, 54, 53, 53, 53, 54, 53, 53, 53,
    53, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 53, 54, 53, 52, 53, 53, 53, 53, 53, 53, 52
    };

    public static readonly ulong[] bishopMagics = {
    5966178808168023810, 1842125191758349089, 10533941675521962910, 9901199631436947968,
    5536055207521974789, 4869240562412309456, 8927944998205334323, 5743712825235349534,
    723969995097844231, 14725601650039456263, 5682012216124052366, 15986731778647950440,
    9733487229979584873, 15770196356148759555, 16834172866873483990, 11165088435909188145,
    7255507254347642886, 11061971705323072512, 14720579517449241344, 7662883957219795827,
    4595362170602786997, 16881180817504870478, 6162059056108144657, 15191983729024316245,
    4906707387531136297, 4268621503690577936, 12577656573411000833, 11121929648843064057,
    5890427939323920397, 15970826419924206100, 3920385987947401848, 15324633179252427794,
    17399428745270395996, 12496979153468266758, 7446158344242857794, 15388553670485147917,
    18157388948709441666, 2495000289734755331, 12150298413881953285, 15890599869170189824,
    2667430466346561820, 881595124139848741, 6161489863012218883, 8575034697900496906,
    13943565624287562752, 7979535262013206017, 3544384028310590463, 6932772747103955723,
    11297575502875002659, 13754640625634916409, 6309474944029296685, 16458134704655708198,
    5693202423122890502, 9538388098138112232, 12601085158815763393, 12404118608191032759,
    1316744904022118414, 8461130387919212577, 4664472141520903201, 16549360510536091673,
    14017898399109613073, 7798943797451169548, 16400764796920005654, 98340380231467648
    };
    public static readonly int[] bishopShifts = {
    58, 59, 59, 59, 59, 59, 59, 58, 59, 59, 59, 59, 59, 59, 59, 59, 59, 59, 57, 57, 57,
    57, 59, 59, 59, 59, 57, 54, 55, 57, 59, 59, 59, 59, 57, 55, 55, 57, 59, 59, 59, 59,
    57, 57, 57, 57, 59, 59, 59, 59, 59, 59, 59, 59, 59, 59, 58, 59, 59, 59, 59, 59, 59, 58
    };



    public static readonly ulong[] rookMasks;
    public static readonly ulong[][] rookMoveBitboards = new ulong[64][];

    private static readonly ulong[][] rookBlockerArrangements = new ulong[64][];


    public static readonly ulong[] bishopMasks;
    public static readonly ulong[][] bishopMoveBitboards = new ulong[64][];

    private static readonly ulong[][] bishopBlockerArrangements = new ulong[64][];



    static MagicData()
    {
        rookMasks = new ulong[64];
        bishopMasks = new ulong[64];

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            for (int i = 0; i < 4; i++)
            {
                int direction = PrecomputedData.DirectionOffsets[i];

                for (int n = 0; n < PrecomputedData.NumSquaresToEdge[startSquare][i] - 1; n++)
                {
                    int targetSquare = startSquare + direction * (n + 1);

                    rookMasks[startSquare] |= 1UL << targetSquare;
                }
            }

            for (int i = 4; i < 8; i++)
            {
                int direction = PrecomputedData.DirectionOffsets[i];

                for (int n = 0; n < PrecomputedData.NumSquaresToEdge[startSquare][i] - 1; n++)
                {
                    int targetSquare = startSquare + direction * (n + 1);

                    bishopMasks[startSquare] |= 1UL << targetSquare;
                }
            }
        }



        for (int rookSquare = 0; rookSquare < 64; rookSquare++)
        {
            ulong mask = rookMasks[rookSquare];

            List<int> bitIndices = new List<int>(); //The indexes of each bit that is turned on in the mask
            for (int bitIndex = 0; bitIndex < 64; bitIndex++)
            {
                if ((mask & (1UL << bitIndex)) != 0) //If this index is turned on
                {
                    bitIndices.Add(bitIndex);
                }
            }

            int numPatterns = 1 << bitIndices.Count; //Same as 2^bitIndices
            ulong[] blockerBoardsForSquare = new ulong[numPatterns];

            for (int i = 0; i < numPatterns; i++)
            {
                for (int bitIndex = 0; bitIndex < bitIndices.Count; bitIndex++)
                {
                    int bit = (i >> bitIndex) & 1;
                    blockerBoardsForSquare[i] |= (ulong)bit << bitIndices[bitIndex];
                }
            }

            rookBlockerArrangements[rookSquare] = blockerBoardsForSquare;

            rookMoveBitboards[rookSquare] = GenerateRookMoves(rookSquare);
        }



        for (int bishopSquare = 0; bishopSquare < 64; bishopSquare++)
        {
            ulong mask = bishopMasks[bishopSquare];

            List<int> bitIndices = new List<int>(); //The indexes of each bit that is turned on in the mask
            for (int bitIndex = 0; bitIndex < 64; bitIndex++)
            {
                if ((mask & (1UL << bitIndex)) != 0) //If this index is turned on
                {
                    bitIndices.Add(bitIndex);
                }
            }

            int numPatterns = 1 << bitIndices.Count; //Same as 2^bitIndices
            ulong[] blockerBoardsForSquare = new ulong[numPatterns];

            for (int i = 0; i < numPatterns; i++)
            {
                for (int bitIndex = 0; bitIndex < bitIndices.Count; bitIndex++)
                {
                    int bit = (i >> bitIndex) & 1;
                    blockerBoardsForSquare[i] |= (ulong)bit << bitIndices[bitIndex];
                }
            }

            bishopBlockerArrangements[bishopSquare] = blockerBoardsForSquare;

            bishopMoveBitboards[bishopSquare] = GenerateBishopMoves(bishopSquare);
        }
    }

    private static ulong[] GenerateRookMoves(int square)
    {
        int bits = 64 - rookShifts[square];
        int length = 1 << bits;

        ulong[] moves = new ulong[length];


        for (int i = 0; i < rookBlockerArrangements[square].Length; i++)
        {
            ulong blockerArrangement = rookBlockerArrangements[square][i];
            ulong moveBoard = 0;
            ulong index = (blockerArrangement * rookMagics[square]) >> rookShifts[square];

            for (int dirIndex = 0; dirIndex < 4; dirIndex++)
            {
                int direction = PrecomputedData.DirectionOffsets[dirIndex];
                int squaresToEdge = PrecomputedData.NumSquaresToEdge[square][dirIndex];

                for (int n = 0; n < squaresToEdge; n++)
                {
                    int targetSquare = square + direction * (n + 1);

                    moveBoard = BitBoardHelper.AddSquare(moveBoard, targetSquare);

                    if (BitBoardHelper.ContainsSquare(blockerArrangement, targetSquare)) break;//If theres a piece on the target square we cant move any further in this direction
                }
            }

            moves[index] = moveBoard;
        }


        return moves;
    }

    private static ulong[] GenerateBishopMoves(int square)
    {
        int bits = 64 - bishopShifts[square];
        int length = 1 << bits;

        ulong[] moves = new ulong[length];


        for (int i = 0; i < bishopBlockerArrangements[square].Length; i++)
        {
            ulong blockerArrangement = bishopBlockerArrangements[square][i];
            ulong moveBoard = 0;
            ulong index = (blockerArrangement * bishopMagics[square]) >> bishopShifts[square];

            for (int dirIndex = 4; dirIndex < 8; dirIndex++)
            {
                int direction = PrecomputedData.DirectionOffsets[dirIndex];
                int squaresToEdge = PrecomputedData.NumSquaresToEdge[square][dirIndex];

                for (int n = 0; n < squaresToEdge; n++)
                {
                    int targetSquare = square + direction * (n + 1);

                    moveBoard = BitBoardHelper.AddSquare(moveBoard, targetSquare);

                    if (BitBoardHelper.ContainsSquare(blockerArrangement, targetSquare)) break;//If theres a piece on the target square we cant move any further in this direction
                }
            }

            moves[index] = moveBoard;
        }


        return moves;
    }
}