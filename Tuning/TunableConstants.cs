
using System.Diagnostics;

public static class TunableConstants
{
    public enum Category { MoveOrdering, Search, Evaluation };


    //Move Ordering ----------------------------------------------

#if TUNABLE
    public static TunableConstant killerBias =  Tuning.AddConstant(new TunableConstant("KillerBias", Category.MoveOrdering, 5302, 0, 10000));
    public static TunableConstant goodCaptureBias =  Tuning.AddConstant(new TunableConstant("GoodCaptureBias", Category.MoveOrdering, 9900, 0, 15000)); //Increased max from 10k bc tuner maxed it out pretty quickly
    public static TunableConstant equalCaptureBias =  Tuning.AddConstant(new TunableConstant("EqualCaptureBias", Category.MoveOrdering, 7842, 0, 10000));
    public static TunableConstant badCaptureBias =  Tuning.AddConstant(new TunableConstant("BadCaptureBias", Category.MoveOrdering, 2692, 0, 7500));
    public static TunableConstant attackedByPawnBias =  Tuning.AddConstant(new TunableConstant("AttackedByPawnBias", Category.MoveOrdering, -579, -2500, 0));
    public static TunableConstant attackedByKnightBias =  Tuning.AddConstant(new TunableConstant("AttackedByKnightBias", Category.MoveOrdering, -110, -1000, 0));
    public static TunableConstant attackedByKnightMinPieceValue =  Tuning.AddConstant(new TunableConstant("AttackedByKnightMinPieceValue", Category.MoveOrdering, 463, 0, 1500));
    public static TunableConstant captureValueDeltaMultiplier =  Tuning.AddConstant(new TunableConstant("CaptureValueDeltaMultiplier", Category.MoveOrdering, 2f, -1f, 10f, true));
    public static TunableConstant maxHistory =  Tuning.AddConstant(new TunableConstant("MaxHistory", Category.MoveOrdering, 844, 1, 2048));
    public static TunableConstant historyDecay =  Tuning.AddConstant(new TunableConstant("HistoryDecay", Category.MoveOrdering, 4982, 0, 10000));
    public static TunableConstant promotionMultiplier =  Tuning.AddConstant(new TunableConstant("PromotionMultiplier", Category.MoveOrdering, 1f, -1f, 5f, true));
    public static TunableConstant defendedByPawnBias =  Tuning.AddConstant(new TunableConstant("DefendedByPawnBias", Category.MoveOrdering, 58, 0, 1000));

    public static int KillerBias {get {return (int)killerBias.currentValue;}}
    public static int GoodCaptureBias {get {return (int)goodCaptureBias.currentValue;}}
    public static int EqualCaptureBias {get {return (int)equalCaptureBias.currentValue;}}
    public static int BadCaptureBias {get {return (int)badCaptureBias.currentValue;}}
    public static int AttackedByPawnBias {get {return (int)attackedByPawnBias.currentValue;}}
    public static int AttackedByKnightBias {get {return (int)attackedByKnightBias.currentValue;}}
    public static int AttackedByKnightMinPieceValue {get {return (int)attackedByKnightMinPieceValue.currentValue;}}
    public static float CaptureValueDeltaMultiplier {get {return captureValueDeltaMultiplier.currentValue;}}
    public static int MaxHistory {get {return (int)maxHistory.currentValue;}}
    public static int HistoryDecay {get {return (int)historyDecay.currentValue;}}
    public static float PromotionMultiplier {get {return promotionMultiplier.currentValue;}}
    public static int DefendedByPawnBias {get {return (int)defendedByPawnBias.currentValue;}}
#else
    public const int KillerBias = 5302; //500000
    public const int GoodCaptureBias = 9900;
    public const int EqualCaptureBias = 7842;
    public const int BadCaptureBias = 2692;
    public const int AttackedByPawnBias = -579;
    public const int AttackedByKnightBias = -110;
    public const int AttackedByKnightMinPieceValue = 463;
    public const float CaptureValueDeltaMultiplier = 2;
    public const int MaxHistory = 844;
    public const int HistoryDecay = 4982;
    public const float PromotionMultiplier = 1; //TODO: Try setting to 0 (just remove code using it completely), tuner seemed to be really considering that option
    public const int DefendedByPawnBias = 58;
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
    public static TunableConstant razoringMargin = Tuning.AddConstant(new TunableConstant("RazoringMargin", Category.Search, 300, 150, 350));
    public static TunableConstant rfpMargin = Tuning.AddConstant(new TunableConstant("RFPMargin", Category.Search, 150, 50, 200));

    public static int AspInstabilityMargin {get {return (int)aspInstabilityMargin.currentValue;}}
    public static int AspWindowIncrement0 {get {return (int)aspWindowIncrement0.currentValue;}}
    public static int AspWindowIncrement1 {get {return (int)aspWindowIncrement1.currentValue;}}
    public static int AspWindowIncrement2 {get {return (int)aspWindowIncrement2.currentValue;}}
    public static int AspWindowIncrement3 {get {return (int)aspWindowIncrement3.currentValue;}}
    public static int AspWindowIncrement4 {get {return (int)aspWindowIncrement4.currentValue;}}
    public static int AspWindowIncrement5 {get {return (int)aspWindowIncrement5.currentValue;}}
    public static int AspWindowIncrement6 {get {return (int)aspWindowIncrement6.currentValue;}}
    public static int RazoringMargin {get {return (int)razoringMargin.currentValue;}}
    public static int RFPMargin {get {return (int)rfpMargin.currentValue;}}

#else
    public const int AspInstabilityMargin = 25;
    public const int AspWindowIncrement0 = 25;
    public const int AspWindowIncrement1 = 50;
    public const int AspWindowIncrement2 = 100;
    public const int AspWindowIncrement3 = 200;
    public const int AspWindowIncrement4 = 400;
    public const int AspWindowIncrement5 = 800;
    public const int AspWindowIncrement6 = 1600;
    public const int RazoringMargin = 300;
    public const int RFPMargin = 150;
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
        killerBias.currentValue = killerBias.currentValue + 0; //Lowkey janky but otherwise variables aren't initialized and thereby not added to the constant-list
    }

    public class TunableConstant
    {
        public string name;
        public Category category;
        public float currentValue;
        public float minValue;
        public float maxValue;
        public float cEnd;
        public float rEnd;
        public bool isFloat;

        public TunableConstant(string _name, Category _category, float _currentValue, float _minValue, float _maxValue, bool _isFloat = false, float _cEnd = float.MaxValue, float _rEnd = 0.002f)
        {
            name = _name;
            category = _category;
            currentValue = _currentValue;
            minValue = _minValue;
            maxValue = _maxValue;

            if (_cEnd == float.MaxValue) //TODO: Make SURE C_end is above 0.5 (or 1 if that is the min, don't remember)
            {
                cEnd = ((float)maxValue - (float)minValue) / 20f;
            }
            else cEnd = _cEnd;

            rEnd = _rEnd;

            isFloat = _isFloat;
        }
    }
#endif
}