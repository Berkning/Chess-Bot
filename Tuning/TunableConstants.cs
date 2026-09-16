
using System.Diagnostics;

public static class TunableConstants
{
    public enum Category { MoveOrdering, Search, Evaluation };


    //Move Ordering ----------------------------------------------

#if TUNABLE
    public static TunableConstant prevBestBias = Tuning.AddConstant(new TunableConstant("PrevBestBias", Category.MoveOrdering, 15406, 0, 20000));
    public static TunableConstant killerBias =  Tuning.AddConstant(new TunableConstant("KillerBias", Category.MoveOrdering, 6515, 0, 10000));
    public static TunableConstant goodCaptureBias =  Tuning.AddConstant(new TunableConstant("GoodCaptureBias", Category.MoveOrdering, 9250, 0, 10000));
    public static TunableConstant badCaptureBias =  Tuning.AddConstant(new TunableConstant("BadCaptureBias", Category.MoveOrdering, 1805, 0, 10000));
    public static TunableConstant attackedByPawnBias =  Tuning.AddConstant(new TunableConstant("AttackedByPawnBias", Category.MoveOrdering, -704, -2500, 500));
    public static TunableConstant captureValueDeltaMultiplier =  Tuning.AddConstant(new TunableConstant("CaptureValueDeltaMultiplier", Category.MoveOrdering, 2, 0, 10));
    public static TunableConstant maxHistory =  Tuning.AddConstant(new TunableConstant("MaxHistory", Category.MoveOrdering, 1020, 1, 2048));
    public static TunableConstant historyDecay =  Tuning.AddConstant(new TunableConstant("HistoryDecay", Category.MoveOrdering, 7917, 0, 10000));

    public static int PrevBestBias {get {return prevBestBias.currentValue;}}
    public static int KillerBias {get {return killerBias.currentValue;}}
    public static int GoodCaptureBias {get {return goodCaptureBias.currentValue;}}
    public static int BadCaptureBias {get {return badCaptureBias.currentValue;}}
    public static int AttackedByPawnBias {get {return attackedByPawnBias.currentValue;}}
    public static int CaptureValueDeltaMultiplier {get {return captureValueDeltaMultiplier.currentValue;}}
    public static int MaxHistory {get {return maxHistory.currentValue;}}
    public static int HistoryDecay {get {return historyDecay.currentValue;}}
#else
    public const int PrevBestBias = 15406; //2000000
    public const int KillerBias = 6515; //500000
    public const int GoodCaptureBias = 9250;
    public const int BadCaptureBias = 1805;
    public const int AttackedByPawnBias = -704;
    public const int CaptureValueDeltaMultiplier = 2;
    public const int MaxHistory = 1020;
    public const int HistoryDecay = 7917;
#endif

    //------------------------------------------------------------



    //Search -----------------------------------------------------

#if TUNABLE

    public static TunableConstant aspInstabilityMargin = Tuning.AddConstant(new TunableConstant("AspInstabilityMargin", Category.Search, 25, 0, 75)); //TODO: Check once if tuning wants this to go negative, otherwise remove and just go like 0-100
    public static TunableConstant aspWindowIncrement0 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement0", Category.Search, 25, 0, 75));
    public static TunableConstant aspWindowIncrement1 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement1", Category.Search, 50, 0, 110));
    public static TunableConstant aspWindowIncrement2 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement2", Category.Search, 97, 0, 250));
    public static TunableConstant aspWindowIncrement3 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement3", Category.Search, 210, 0, 325));
    public static TunableConstant aspWindowIncrement4 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement4", Category.Search, 394, 0, 600));
    public static TunableConstant aspWindowIncrement5 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement5", Category.Search, 825, 0, 1100));
    public static TunableConstant aspWindowIncrement6 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement6", Category.Search, 1438, 0, 1800));

    public static int AspInstabilityMargin {get {return aspInstabilityMargin.currentValue;}}
    public static int AspWindowIncrement0 {get {return aspWindowIncrement0.currentValue;}}
    public static int AspWindowIncrement1 {get {return aspWindowIncrement1.currentValue;}}
    public static int AspWindowIncrement2 {get {return aspWindowIncrement2.currentValue;}}
    public static int AspWindowIncrement3 {get {return aspWindowIncrement3.currentValue;}}
    public static int AspWindowIncrement4 {get {return aspWindowIncrement4.currentValue;}}
    public static int AspWindowIncrement5 {get {return aspWindowIncrement5.currentValue;}}
    public static int AspWindowIncrement6 {get {return aspWindowIncrement6.currentValue;}}

#else
    public const int AspInstabilityMargin = 25;
    public const int AspWindowIncrement0 = 25;
    public const int AspWindowIncrement1 = 50;
    public const int AspWindowIncrement2 = 97;
    public const int AspWindowIncrement3 = 210;
    public const int AspWindowIncrement4 = 394;
    public const int AspWindowIncrement5 = 825;
    public const int AspWindowIncrement6 = 1438;
#endif

    //------------------------------------------------------------



    //Evaluation -------------------------------------------------

#if TUNABLE
    //TODO:
#else

#endif

    //------------------------------------------------------------







#if TUNABLE
    public static void Initialize()
    {
        prevBestBias.currentValue = prevBestBias.currentValue + 0; //Lowkey janky but otherwise variables aren't initialized and thereby not added to the constant-list
    }

    public class TunableConstant
    {
        public string name;
        public Category category;
        public int currentValue;
        public int minValue;
        public int maxValue;
        public float cEnd;
        public float rEnd;

        public TunableConstant(string _name, Category _category, int _currentValue, int _minValue, int _maxValue, float _cEnd = float.MaxValue, float _rEnd = 0.002f)
        {
            name = _name;
            category = _category;
            currentValue = _currentValue;
            minValue = _minValue;
            maxValue = _maxValue;

            if (_cEnd == float.MaxValue)
            {
                cEnd = ((float)maxValue - (float)minValue) / 20f;
            }
            else cEnd = _cEnd;

            rEnd = _rEnd;
        }
    }
#endif
}