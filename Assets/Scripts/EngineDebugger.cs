using TMPro;
using UnityEngine;

public class EngineDebugger : MonoBehaviour
{
    [SerializeField] private TMP_Text gameStage;
    [SerializeField] private TMP_Text whiteMopup;
    [SerializeField] private TMP_Text blackMopup;

    void Update()
    {
        Evaluation.Evaluate();

        float stage = Evaluation.gameStage;
        gameStage.text = stage.ToString("0.#####");

        float endgameMultiplier = Mathf.Max(stage - 1f, 0f);


        int whiteMopScore = 0;
        whiteMopScore += Mathf.CeilToInt(10f * PrecomputedData.manhattanDistanceFromCenter[Board.blackKingSquare] * endgameMultiplier);
        whiteMopScore += Mathf.CeilToInt(10f * (7f - PrecomputedData.kingDistanceLookup[Board.whiteKingSquare][Board.blackKingSquare]) * endgameMultiplier);
        whiteMopup.text = whiteMopScore.ToString();

        int blackMopScore = 0;
        blackMopScore += Mathf.CeilToInt(10f * PrecomputedData.manhattanDistanceFromCenter[Board.whiteKingSquare] * endgameMultiplier);
        blackMopScore += Mathf.CeilToInt(10f * (7f - PrecomputedData.kingDistanceLookup[Board.blackKingSquare][Board.whiteKingSquare]) * endgameMultiplier);
        blackMopup.text = blackMopScore.ToString();
    }
}