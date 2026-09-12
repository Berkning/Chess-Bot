
using System.Diagnostics;

public static class TunableConstants
{
    public enum Category { MoveOrdering, Search, Evaluation };


    //Move Ordering ----------------------------------------------

#if TUNABLE
    public static TunableConstant prevBestBias = Tuning.AddConstant(new TunableConstant("PrevBestBias", Category.MoveOrdering, 15000, 0, 25000));
    public static TunableConstant killerBias =  Tuning.AddConstant(new TunableConstant("KillerBias", Category.MoveOrdering, 10000, 0, 20000));
    public static TunableConstant goodCaptureBias =  Tuning.AddConstant(new TunableConstant("GoodCaptureBias", Category.MoveOrdering, 8000, 0, 20000));
    public static TunableConstant badCaptureBias =  Tuning.AddConstant(new TunableConstant("BadCaptureBias", Category.MoveOrdering, 1100, 0, 20000));
    public static TunableConstant attackedByPawnBias =  Tuning.AddConstant(new TunableConstant("AttackedByPawnBias", Category.MoveOrdering, -350, -10000, 500));
    public static TunableConstant captureValueDeltaMultiplier =  Tuning.AddConstant(new TunableConstant("CaptureValueDeltaMultiplier", Category.MoveOrdering, 1, 0, 20));
    public static TunableConstant maxHistory =  Tuning.AddConstant(new TunableConstant("MaxHistory", Category.MoveOrdering, 1024, 1, 4096));
    public static TunableConstant historyDecay =  Tuning.AddConstant(new TunableConstant("HistoryDecay", Category.MoveOrdering, 8000, 0, 10000));

    public static int PrevBestBias {get {return prevBestBias.currentValue;}}
    public static int KillerBias {get {return killerBias.currentValue;}}
    public static int GoodCaptureBias {get {return goodCaptureBias.currentValue;}}
    public static int BadCaptureBias {get {return badCaptureBias.currentValue;}}
    public static int AttackedByPawnBias {get {return attackedByPawnBias.currentValue;}}
    public static int CaptureValueDeltaMultiplier {get {return captureValueDeltaMultiplier.currentValue;}}
    public static int MaxHistory {get {return maxHistory.currentValue;}}
    public static int HistoryDecay {get {return historyDecay.currentValue;}}
#else
    public const int PrevBestBias = 15000; //2000000
    public const int KillerBias = 10000; //500000
    public const int GoodCaptureBias = 8000;
    public const int BadCaptureBias = 1100;
    public const int AttackedByPawnBias = -350;
    public const int CaptureValueDeltaMultiplier = 1;
    public const int MaxHistory = 1024;
    public const int HistoryDecay = 8000;
#endif

    //------------------------------------------------------------



    //Search -----------------------------------------------------

#if TUNABLE

    public static TunableConstant aspInstabilityMargin = Tuning.AddConstant(new TunableConstant("AspInstabilityMargin", Category.Search, 25, -100, 150)); //TODO: Check once if tuning wants this to go negative, otherwise remove and just go like 0-100
    public static TunableConstant aspWindowIncrement0 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement0", Category.Search, 25, 0, 150));
    public static TunableConstant aspWindowIncrement1 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement1", Category.Search, 50, 0, 300));
    public static TunableConstant aspWindowIncrement2 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement2", Category.Search, 100, 0, 650));
    public static TunableConstant aspWindowIncrement3 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement3", Category.Search, 200, 0, 1100));
    public static TunableConstant aspWindowIncrement4 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement4", Category.Search, 400, 0, 1600));
    public static TunableConstant aspWindowIncrement5 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement5", Category.Search, 800, 0, 3000));
    public static TunableConstant aspWindowIncrement6 = Tuning.AddConstant(new TunableConstant("AspWindowIncrement6", Category.Search, 1600, 0, 9000));

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
    public const int AspWindowIncrement2 = 100;
    public const int AspWindowIncrement3 = 200;
    public const int AspWindowIncrement4 = 400;
    public const int AspWindowIncrement5 = 800;
    public const int AspWindowIncrement6 = 1600;
#endif

    //------------------------------------------------------------



    //Evaluation -------------------------------------------------

#if TUNABLE

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