#if TUNABLE

using System.Diagnostics;

public static class Tuning
{
    private static List<TunableConstants.TunableConstant> constantList = new List<TunableConstants.TunableConstant>();

    public static void RecieveSetOption(string[] args)
    {
        int constantIndex = -1;

        for (int i = 0; i < constantList.Count; i++)
        {
            if (constantList[i].name == args[2])
            {
                constantIndex = i;
                break;
            }
        }

        if (constantIndex == -1)
        {
            Console.WriteLine("no tunable constant called: '" + args[2] + "'");
            return;
        }

        //args[3] == "value" assumed

        if (int.TryParse(args[4], out int value))
        {
            constantList[constantIndex].currentValue = value;
            Console.WriteLine("info string set tunable constant " + constantList[constantIndex].name + " to " + value);
        }
        else Console.WriteLine("Unable to parse provided value '" + args[4] + "'");
    }

    public static void RecieveTuningCommand(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Incomplete tuning command. Did you mean 'tuning list'?");
            return;
        }

        if (args[1].ToLower() != "list")
        {
            Console.WriteLine("Unrecognized tuning command '" + args[1] + "'. Did you mean 'tuning list'?");
            return;
        }

        if (args.Length < 3)
        {
            int count = 0;
            foreach (TunableConstants.TunableConstant constant in constantList)
            {
                count++;
                LogConstant(constant);
            }
            Console.WriteLine("Listed " + count + " tunable parameters");
            return;
        }



        if (Enum.TryParse(typeof(TunableConstants.Category), args[2], true, out object? parseResult))
        {
            if (parseResult == null)
            {
                Console.WriteLine("Parsing failed on category '" + args[2] + "'");
                return;
            }

            TunableConstants.Category category = (TunableConstants.Category)parseResult;


            int count = 0;
            foreach (TunableConstants.TunableConstant constant in constantList)
            {
                if (constant.category == category)
                {
                    count++;
                    LogConstant(constant);
                }
            }

            Console.WriteLine("Listed " + count + " tunable parameters");
        }
        else Console.WriteLine("Cannot parse category '" + args[2] + "'");
    }



    public static void LogAllConstants()
    {
        foreach (TunableConstants.TunableConstant constant in constantList)
        {
            Console.WriteLine("option name " + constant.name + " type spin default " + constant.currentValue + " min " + constant.minValue + " max " + constant.maxValue);
        }
    }

    private static void LogConstant(TunableConstants.TunableConstant constant)
    {
        Console.WriteLine(constant.name + ", int, " + constant.currentValue + ".0, " + constant.minValue + ".0, " + constant.maxValue + ".0, " + constant.cEnd + ", " + constant.rEnd);
    }

    public static TunableConstants.TunableConstant AddConstant(TunableConstants.TunableConstant constant)
    {
        constantList.Add(constant);
        return constant;
    }
}

#endif