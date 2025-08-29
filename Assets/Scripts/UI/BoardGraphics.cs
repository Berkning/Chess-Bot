using System;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class BoardGraphics : MonoBehaviour
{
    [SerializeField] private Color lightColor;
    [SerializeField] private Color darkColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color moveColor;

    [SerializeField] private Color trueColor;
    [SerializeField] private Color falseColor;

    [SerializeField] private Sprite[] pieceSprites;
    [SerializeField] private Material pieceMat;
    [SerializeField] private float pieceScale = 2;
    [SerializeField] private float pieceDepth;
    [SerializeField] private bool whiteIsBottom = true;

    private MeshRenderer[] squareRenderers;
    private SpriteRenderer[] squarePieceRenderers;

    void Start()
    {
        CreateBoardUI();
    }

    void Update()
    {
        UpdatePieces();

        //HighlightBitBoard(MoveGenerator.checkRayBitMap);

        // for (int i = 0; i < 64; i++)
        // {
        //     if (Board.Squares[i] == Piece.None) HighlightSquare(i, falseColor);
        //     else HighlightSquare(i, trueColor);
        // }
    }

    private void CreateBoardUI()
    {

        Shader squareShader = Shader.Find("Unlit/Color");
        squareRenderers = new MeshRenderer[64];
        squarePieceRenderers = new SpriteRenderer[64];

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                // Create square
                Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                square.parent = transform;
                square.name = BoardHelper.SquareNameFromCoord(file, rank);
                square.position = PositionFromCoord(file, rank);
                Material squareMaterial = new Material(squareShader);

                squareRenderers[BoardHelper.CoordToIndex(file, rank)] = square.gameObject.GetComponent<MeshRenderer>();
                squareRenderers[BoardHelper.CoordToIndex(file, rank)].material = squareMaterial;

                // Create piece sprite renderer for current square
                SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
                pieceRenderer.transform.parent = square;
                pieceRenderer.transform.position = PositionFromCoord(file, rank, pieceDepth);
                pieceRenderer.transform.localScale = Vector3.one * 100f / (2000f / 6f) * pieceScale;
                pieceRenderer.sharedMaterial = pieceMat;
                squarePieceRenderers[BoardHelper.CoordToIndex(file, rank)] = pieceRenderer;
            }
        }

        ResetSquareColors();
    }


    public void HighlightBitBoard(ulong bitboard)
    {
        for (int i = 0; i < 64; i++)
        {
            if ((bitboard & (1UL << i)) > 0)
            {
                squareRenderers[i].sharedMaterial.color = trueColor;
            }
            else squareRenderers[i].sharedMaterial.color = falseColor;
        }
    }



    public void UpdatePieces()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                UpdatePieceSprite(file, rank);
            }
        }
    }

    private void UpdatePieceSprite(int file, int rank)
    {
        int piece = Board.Squares[BoardHelper.CoordToIndex(file, rank)];

        bool isWhite = Piece.Color(piece) == Piece.White;

        int type = Piece.Type(piece) - 1;

        if (type == -1)
        {
            squarePieceRenderers[BoardHelper.CoordToIndex(file, rank)].sprite = null;
            return;
        }

        squarePieceRenderers[BoardHelper.CoordToIndex(file, rank)].sprite = pieceSprites[type + (isWhite ? 0 : 6)];
    }

    public bool TryGetSquareUnderMouse(Vector2 mouseWorld, out int selectedIndex)
    {
        int file = (int)(mouseWorld.x + 4);
        int rank = (int)(mouseWorld.y + 4);
        if (!whiteIsBottom)
        {
            file = 7 - file;
            rank = 7 - rank;
        }
        selectedIndex = BoardHelper.CoordToIndex(file, rank);
        return file >= 0 && file < 8 && rank >= 0 && rank < 8;
    }

    public void HighlightLegalMoves(int startSquare)
    {
        //Obv insanely inefficient to regenerate moves instead of using existing ones
        Span<Move> moves = MoveGenerator.GenerateMovesSlow();

        foreach (Move move in moves)
        {
            if (move.startSquare == startSquare)
            {
                squareRenderers[move.targetSquare].sharedMaterial.color = moveColor;
            }
        }
    }

    public void HighlightSquare(int index)
    {
        squareRenderers[index].sharedMaterial.color = highlightColor;
    }

    public void HighlightSquare(int index, Color color)
    {
        squareRenderers[index].sharedMaterial.color = color;
    }

    public void ResetSquareColors()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                bool isLight = (file + rank) % 2 == 1;

                squareRenderers[BoardHelper.CoordToIndex(file, rank)].sharedMaterial.color = isLight ? lightColor : darkColor;
            }
        }
    }

    private Vector3 PositionFromCoord(int file, int rank, float depth = 0)
    {
        if (whiteIsBottom)
        {
            return new Vector3(-3.5f + file, -3.5f + rank, depth);
        }
        return new Vector3(-3.5f + 7 - file, 7 - rank - 3.5f, depth);
    }
}