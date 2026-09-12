
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
    public static TunableConstant captureValueDeltaMultiplier =  Tuning.AddConstant(new TunableConstant("CaptureValueDeltaMultiplier", Category.MoveOrdering, 2, 0, 15));
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
    public const int PrevBestBias = 15406; //2000000
    public const int KillerBias = 6515; //500000
    public const int GoodCaptureBias = 9250;
    public const int BadCaptureBias = 1805;
    public const int AttackedByPawnBias = -704;
    public const int CaptureValueDeltaMultiplier = 2;
    public const int MaxHistory = 1024;
    public const int HistoryDecay = 8000;
#endif

    //------------------------------------------------------------

#if TUNABLE
    public static void Initialize()
    {
        prevBestBias.currentValue = prevBestBias.currentValue + 0; //Lowkey janky but otherwise variables aren't initialized and thereby not added to the constant-list
    }
#endif













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
}