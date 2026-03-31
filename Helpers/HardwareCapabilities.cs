

using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

public static class HardwareCapabilities
{
    private const int MinTime = 100; //Minimum amount of time a benchmark has to run for, before we can count it as conclusive
    private const int MaxRounds = 256;
    private const int RandomBoardCount = 65536;
    public static readonly bool UsePEXT = false;

    static HardwareCapabilities()
    {
        UsePEXT = IsPEXTFaster();
    }

    public static void Initialize()
    {

    }

    private static bool IsPEXTFaster()
    {
        Console.WriteLine("Running PEXT benchmark...");

        Span<ulong> occupancies = stackalloc ulong[RandomBoardCount];

        //Create random piece patterns
        for (int i = 0; i < RandomBoardCount; i++)
        {
            ulong board = (ulong)Random.Shared.NextInt64();
            //Console.WriteLine(BitBoardHelper.BitCount(board));

            occupancies[i] = board;
        }



        //Warmup
        for (int i = 0; i < 4; i++)
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
                    Console.WriteLine("Exiting after " + (i + 1) + " rounds because diff is " + difference);
                    break; //If PEXT is significantly faster //TODO: return true
                }
                else if (difference < 0.75d)
                {
                    Console.WriteLine("Exiting after " + (i + 1) + " rounds because diff is " + difference);
                    break; //If magics are significantly faster //TODO: return false
                }
            }
        }

        Console.WriteLine("Total PEXT time was: " + totalPEXTTime);
        Console.WriteLine("Total Magic time was: " + totalMagicTime);

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