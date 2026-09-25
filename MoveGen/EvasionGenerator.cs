

public class EvasionGenerator
{
    public int Generate(ref Span<Move> moves, CheckType checkType)
    {
        int moveCount = 0;

        if (checkType.IsSlidingCheck()) //If sliding piece check, we don't know if we're in double check yet
        {

        }
        else if (checkType.IsKnightCheck()) //Knight check
        {

        }
        else //Pawn check
        {

        }

        return moveCount;
    }
}