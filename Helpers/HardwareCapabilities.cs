

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class HardwareCapabilities
{
    private const int MinTime = 100; //Minimum amount of time a benchmark has to run for, before we can count it as conclusive
    private const int MaxRounds = 256;
    private const int RandomBoardCount = 65536;
    public static readonly bool UsePEXT = false;

    private static Vector128<int> bishopWeights = Vector128.Create(20, 13, 0, 0);
    private static Vector128<int> rookWeights = Vector128.Create(19, 6, 0, 0);
    private static Vector128<int> queenWeights = Vector128.Create(5, 5, 0, 0);

    static HardwareCapabilities()
    {
        if (Bmi2.IsSupported) UsePEXT = IsPEXTFaster();
    }

    public static void Initialize()
    {
        //MobilityBenchmark();
    }

    private static void MobilityBenchmark()
    {
        Console.WriteLine("Running Mobility Benchmark...");
        Console.WriteLine(Vector128.IsHardwareAccelerated);

        Span<ulong> whitePieces = stackalloc ulong[RandomBoardCount];
        Span<ulong> blackPieces = stackalloc ulong[RandomBoardCount];


        //Create random piece patterns
        for (int i = 0; i < RandomBoardCount; i++)
        {
            whitePieces[i] = (ulong)Random.Shared.NextInt64();
            blackPieces[i] = (ulong)Random.Shared.NextInt64();
        }

        //Warmup
        for (int i = 0; i < 4; i++)
        {
            RunMobilitySIMD(whitePieces[i], blackPieces[i], new int[] { 2, 4 }, new int[] { 6, 61 }, new int[] { 53, 26 }, new int[] { 55, 12 }, new int[] { 52, 24 }, new int[] { 35, 51 });
            RunMobilityScalar(whitePieces[i], blackPieces[i], new int[] { 2, 4 }, new int[] { 6, 61 }, new int[] { 53, 26 }, new int[] { 55, 12 }, new int[] { 52, 24 }, new int[] { 35, 51 });
        }


        Stopwatch simdStopWatch = new Stopwatch();
        Stopwatch scalarStopWatch = new Stopwatch();

        for (int r = 0; r < MaxRounds; r++)
        {
            if (r % 2 == 0)
            {
                scalarStopWatch.Start();
                for (int i = 0; i < RandomBoardCount; i++)
                {
                    RunMobilityScalar(whitePieces[i], blackPieces[i], new int[] { 2, 4 }, new int[] { 6, 61 }, new int[] { 53, 26 }, new int[] { 55, 12 }, new int[] { 52, 24 }, new int[] { 35, 51 });
                    RunMobilityScalar(whitePieces[i], blackPieces[i], new int[] { 7, 52 }, new int[] { 17, 54 }, new int[] { 38, 37 }, new int[] { 12, 42 }, new int[] { 43, 24 }, new int[] { 8, 53 });
                }
                scalarStopWatch.Stop();
            }
            else
            {
                simdStopWatch.Start();
                for (int i = 0; i < RandomBoardCount; i++)
                {
                    RunMobilitySIMD(whitePieces[i], blackPieces[i], new int[] { 2, 4 }, new int[] { 6, 61 }, new int[] { 53, 26 }, new int[] { 55, 12 }, new int[] { 52, 24 }, new int[] { 35, 51 });
                    RunMobilitySIMD(whitePieces[i], blackPieces[i], new int[] { 7, 52 }, new int[] { 17, 54 }, new int[] { 38, 37 }, new int[] { 12, 42 }, new int[] { 43, 24 }, new int[] { 8, 53 });
                }
                simdStopWatch.Stop();
            }
        }

        Console.WriteLine("SIMD took: " + simdStopWatch.ElapsedMilliseconds + "ms - Scalar took: " + scalarStopWatch.ElapsedMilliseconds + "ms");
    }



    private static readonly int[] Weights = {
        -33,49,37,-35,36,-15,61,33,3,17,-2,-46,-32,-8,44,32,-7,10,-16,-37,-45,-29,6,-26,-10,12,9,-22,-30,-32,-24,-51,-6,5,14,-1,-5,-2,0,-32,3,26,17,13,4,19,27,-2,-2,16,15,10,9,15,5,-7,-5,3,6,3,-2,5,4,-4,0,0,0,0,0,0,0,0,-38,-13,-22,-26,-13,8,29,-32,-37,-26,-6,-16,3,-12,14,-31,-50,-24,-12,2,5,-6,-17,-62,-36,-9,-10,-6,8,-8,-9,-52,-56,-49,-18,-63,-15,50,-22,-57,-39,-71,-47,-32,-17,-24,-37,-63,0,0,0,0,0,0,0,0,-32,45,2,29,44,36,43,-24,24,4,54,82,80,77,39,53,35,55,86,83,99,89,87,36,44,41,85,82,99,92,63,39,41,82,75,122,88,112,73,64,-4,66,81,109,127,79,94,19,-41,1,62,45,19,86,12,3,-131,-20,-23,-17,-1,-51,-12,-67,10,9,37,21,17,20,-6,19,7,48,23,21,27,37,65,24,37,30,28,8,19,33,19,36,6,14,3,21,28,2,13,4,-5,11,5,15,15,-6,21,13,-5,5,3,1,18,36,24,38,-26,5,-18,-17,7,-2,0,-40,-10,-27,-24,-22,-14,-18,-9,-20,-17,-16,6,9,16,6,-24,-1,-45,-24,-24,-13,-4,5,-1,-57,-51,-36,-21,-13,-2,-3,-1,-24,-49,-41,-40,-23,-17,-10,-2,-26,-45,-29,-4,12,-10,5,-4,-8,-15,-5,-4,10,13,26,21,11,-8,-20,21,35,28,38,13,24,2,14,-1,11,13,3,10,3,14,13,27,38,20,-5,-12,-9,0,12,20,24,29,31,20,16,-12,12,-2,5,3,11,17,7,-4,-16,-12,-15,1,1,13,3,-20,-20,-18,-20,-13,5,-1,21,-15,-17,-24,10,26,50,47,71,-6,-34,-21,-13,-21,23,-2,41,-44,-19,-11,-3,14,8,-4,4,-46,-43,-22,-7,-28,-10,-43,-72,-33,-6,13,25,25,17,-7,-30,-34,-3,17,30,33,27,8,-11,-39,-8,16,27,31,27,13,-13,-30,5,17,22,18,28,23,-3,-14,13,17,10,15,34,43,7,-26,10,7,5,10,31,33,5,-46,-26,-17,-18,-11,4,1,-21,0,0,0,0,0,0,0,0,24,9,14,7,12,8,-8,-2,15,9,-2,4,4,3,-6,0,27,15,0,-5,-5,-5,2,9,42,25,11,-4,-4,3,14,23,85,75,49,27,10,12,46,59,90,89,63,30,26,31,64,67,0,0,0,0,0,0,0,0,-51,-80,-45,-39,-54,-43,-80,-46,-51,-43,-45,-46,-45,-45,-44,-61,-56,-36,-40,-17,-23,-38,-51,-60,-48,-27,-15,-6,-13,-20,-25,-43,-41,-27,-9,-11,-7,-17,-24,-45,-56,-40,-14,-20,-41,-18,-45,-57,-59,-41,-42,-27,-38,-58,-51,-68,-77,-69,-45,-54,-48,-70,-55,-93,-30,-11,-31,-13,-13,-20,-21,-27,-12,-28,-14,-9,-6,-13,-23,-26,-21,-5,1,11,7,-6,-10,-21,-9,-1,10,9,-2,7,-11,-14,1,5,6,7,6,4,-8,-11,0,-1,5,6,-8,1,-4,-13,-15,-9,-1,-15,-6,-9,-7,-20,-16,-24,-21,-15,-16,-14,-20,-22,5,9,5,7,-1,0,5,-24,8,4,10,8,-2,-5,-6,4,10,10,1,1,-5,-10,-11,-7,17,15,18,12,6,-2,-5,-1,21,15,13,3,8,8,0,8,15,14,11,9,0,-2,5,1,18,27,16,13,3,5,10,5,21,16,21,15,15,13,11,13,-12,-25,-30,-47,-11,-18,-13,-16,-9,-16,-25,-22,-25,-32,-26,-10,-9,-38,4,-11,1,5,7,11,-4,6,2,21,8,8,13,19,-6,5,-12,8,30,28,28,30,-20,-11,5,6,28,15,5,5,-13,-3,11,24,31,23,7,6,-24,-5,2,10,10,6,-21,2,100,356,344,550,1062,52,-8,-10,-2,1,17,39,84,129,4,-3,-7,-23,160,-11,-5,2,20,0,13,8,7,10
        };
    private static int rawMGEval;
    private static int rawEGEval;

    private static void RunMobilitySIMD(ulong whiteAllPieces, ulong blackAllPieces, int[] whiteBishopList, int[] blackBishopList, int[] whiteRookList, int[] blackRookList, int[] whiteQueenList, int[] blackQueenList) //TODO: Account for mobility to squares that are attacked
    {
        int bishopDifference = 0;

        for (int i = 0; i < whiteBishopList.Length; i++)
        {
            bishopDifference += BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(whiteAllPieces, whiteBishopList[i])) >> 1;
        }

        for (int i = 0; i < blackBishopList.Length; i++)
        {
            bishopDifference -= BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(blackAllPieces, blackBishopList[i])) >> 1;
        }

        Vector128<int> results = bishopWeights * bishopDifference;

        //rawMGEval += Weights[790] * bishopDifference;
        //rawEGEval += Weights[791] * bishopDifference;



        int rookDifference = 0;

        for (int i = 0; i < whiteRookList.Length; i++)
        {
            rookDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteRookList[i])) >> 1;
        }

        for (int i = 0; i < blackRookList.Length; i++)
        {
            rookDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackRookList[i])) >> 1;
        }

        results += rookWeights * rookDifference;

        //rawMGEval += Weights[792] * rookDifference;
        //rawEGEval += Weights[793] * rookDifference;



        int queenDifference = 0;

        for (int i = 0; i < whiteQueenList.Length; i++)
        {
            queenDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteQueenList[i]) | MagicData.GetBishopMoveBoard(whiteAllPieces, whiteQueenList[i])) >> 1;
        }

        for (int i = 0; i < blackQueenList.Length; i++)
        {
            queenDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackQueenList[i]) | MagicData.GetBishopMoveBoard(blackAllPieces, blackQueenList[i])) >> 1;
        }

        results += queenWeights * queenDifference;

        //rawMGEval += Weights[794] * queenDifference;
        //rawEGEval += Weights[795] * queenDifference;

        rawMGEval += results[0];
        rawEGEval += results[1];
    }

    private static void RunMobilityScalar(ulong whiteAllPieces, ulong blackAllPieces, int[] whiteBishopList, int[] blackBishopList, int[] whiteRookList, int[] blackRookList, int[] whiteQueenList, int[] blackQueenList) //TODO: Account for mobility to squares that are attacked
    {
        int bishopDifference = 0;

        for (int i = 0; i < whiteBishopList.Length; i++)
        {
            bishopDifference += BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(whiteAllPieces, whiteBishopList[i])) >> 1;
        }

        for (int i = 0; i < blackBishopList.Length; i++)
        {
            bishopDifference -= BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(blackAllPieces, blackBishopList[i])) >> 1;
        }

        rawMGEval += Weights[790] * bishopDifference;
        rawEGEval += Weights[791] * bishopDifference;



        int rookDifference = 0;

        for (int i = 0; i < whiteRookList.Length; i++)
        {
            rookDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteRookList[i])) >> 1;
        }

        for (int i = 0; i < blackRookList.Length; i++)
        {
            rookDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackRookList[i])) >> 1;
        }

        rawMGEval += Weights[792] * rookDifference;
        rawEGEval += Weights[793] * rookDifference;



        int queenDifference = 0;

        for (int i = 0; i < whiteQueenList.Length; i++)
        {
            queenDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteQueenList[i]) | MagicData.GetBishopMoveBoard(whiteAllPieces, whiteQueenList[i])) >> 1;
        }

        for (int i = 0; i < blackQueenList.Length; i++)
        {
            queenDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackQueenList[i]) | MagicData.GetBishopMoveBoard(blackAllPieces, blackQueenList[i])) >> 1;
        }

        rawMGEval += Weights[794] * queenDifference;
        rawEGEval += Weights[795] * queenDifference;
    }



    private static bool IsPEXTFaster()
    {
        //Console.WriteLine("Running PEXT benchmark...");

        Span<ulong> occupancies = stackalloc ulong[RandomBoardCount];

        //Create random piece patterns
        for (int i = 0; i < RandomBoardCount; i++)
        {
            ulong board = (ulong)Random.Shared.NextInt64();
            //Console.WriteLine(BitBoardHelper.BitCount(board));

            occupancies[i] = board;
        }



        //Warmup
        for (int i = 0; i < 4; i++) //TODO: Set this to only 1 round to save a lot of startup delay
        {
            RunPEXT(occupancies);
            RunMagics(occupancies);
        }

        long totalPEXTTime = 0;
        long totalMagicTime = 0;

        for (int i = 0; i < MaxRounds; i++)
        {
            if (i % 2 == 0)
            {
                totalPEXTTime += RunPEXT(occupancies);
                totalMagicTime += RunMagics(occupancies);
            }
            else
            {
                totalMagicTime += RunMagics(occupancies);
                totalPEXTTime += RunPEXT(occupancies);
            }


            if (totalPEXTTime > MinTime || totalMagicTime > MinTime) //Otherwise could cause inconclusive results
            {
                double difference = totalMagicTime / (double)totalPEXTTime; // < 1 if magics are faster, > 1 if PEXT is faster

                if (difference > 1.25d)
                {
                    //Console.WriteLine("Exiting after " + (i + 1) + " rounds because diff is " + difference);
                    break; //If PEXT is significantly faster //TODO: return true
                }
                else if (difference < 0.75d)
                {
                    //Console.WriteLine("Exiting after " + (i + 1) + " rounds because diff is " + difference);
                    break; //If magics are significantly faster //TODO: return false
                }
            }
        }

        //Console.WriteLine("Total PEXT time was: " + totalPEXTTime);
        //Console.WriteLine("Total Magic time was: " + totalMagicTime);

        return totalPEXTTime < totalMagicTime;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long RunPEXT(Span<ulong> occupancies)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int square = 0; square < 64; square++)
        {
            for (int i = 0; i < RandomBoardCount; i++)
            {
                MagicData.GetRookBoardPEXT(occupancies[i], square);
                MagicData.GetBishopBoardPEXT(occupancies[i], square);
            }

            for (int i = 0; i < RandomBoardCount; i++)
            {
                MagicData.GetBishopBoardPEXT(occupancies[i], square);
                MagicData.GetRookBoardPEXT(occupancies[i], square);
            }
        }

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long RunMagics(Span<ulong> occupancies)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int square = 0; square < 64; square++)
        {
            for (int i = 0; i < RandomBoardCount; i++)
            {
                MagicData.GetRookBoardMagic(occupancies[i], square);
                MagicData.GetBishopBoardMagic(occupancies[i], square);
            }

            for (int i = 0; i < RandomBoardCount; i++)
            {
                MagicData.GetBishopBoardMagic(occupancies[i], square);
                MagicData.GetRookBoardMagic(occupancies[i], square);
            }
        }

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}