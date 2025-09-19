using System;
using System.Collections.Generic;
using Analysis;
using UnityEngine;

public class PieceInteraction : MonoBehaviour
{
    [SerializeField] private BoardGraphics boardGraphics;
    [SerializeField] private EngineAnalysis engineAnalysis;
    private int prevSelectedSquare = -1;
    private Move lastMove = Move.nullMove;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            boardGraphics.ResetSquareColors();

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (boardGraphics.TryGetSquareUnderMouse(mousePos, out int selectedSquare))
            {
                Debug.Log(Convert.ToString(Board.Squares[selectedSquare], 2));

                if (prevSelectedSquare != -1)
                {
                    if (!Piece.IsNone(Board.Squares[prevSelectedSquare]))
                    {
                        //Insanely inefficient ofc
                        boardGraphics.HighlightSquare(selectedSquare);
                        boardGraphics.HighlightLegalMoves(selectedSquare);

                        Span<Move> moves = MoveGenerator.GenerateMovesSlow();
                        //Debug.Log(moves.Count + " moves found");

                        foreach (Move move in moves)
                        {
                            if (move.startSquare == prevSelectedSquare && move.targetSquare == selectedSquare)
                            {
                                lastMove = move;
                                Board.MakeMove(move);
                                boardGraphics.ResetSquareColors();
                                engineAnalysis.PlayMove(BoardHelper.GetMoveNameUCI(move));
                            }
                        }

                        prevSelectedSquare = selectedSquare;
                    }
                    else
                    {
                        boardGraphics.HighlightSquare(selectedSquare);
                        boardGraphics.HighlightLegalMoves(selectedSquare);
                        prevSelectedSquare = selectedSquare;
                    }
                }
                else prevSelectedSquare = selectedSquare;
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (lastMove.data != Move.nullMove.data)
            {
                Board.UnMakeMove(lastMove);
                lastMove = Move.nullMove;
            }
        }
    }
}