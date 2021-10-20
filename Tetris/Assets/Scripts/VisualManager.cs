using System;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class VisualManager : MonoBehaviour {

    private LogicManager logicManager;
    [SerializeField] private Sprite sprite;

    [SerializeField] private Color[] tileColors;

    [SerializeField] private TextMeshProUGUI showScore;

    [SerializeField] private Transform showNextPiecePos;

    [SerializeField] private GameObject beginPanel;

    [SerializeField] private GameObject pausePanel;

    [SerializeField] private GameObject gameOverPanel;
    struct Tile
    {
        public GameObject Object;
        public SpriteRenderer SpriteRenderer;
    }

    private Tile[,] grid;

    private Tile[,] nextPieceGrid;
    
    private void Start() {
        LogicManagerOnStart();
    }

    private void LogicManagerOnStart()
    {
        logicManager = FindObjectOfType<LogicManager>();
        logicManager.GameUpdate += HandleGameUpdate;
        logicManager.GameStateChange += HandleGameStateChange; 
        
        CreateNextPieceField();
        CreateField(logicManager.GridSize, logicManager.FixedPieces);
        ResetToIdle(logicManager.FixedPieces);
        ResetNextPieceBoard();
        DrawNextPiece(logicManager.GetNextPieceInBag());
        DrawActivePiece(logicManager.ActivePiece, logicManager.ActivePieceRotation, logicManager.ActivePiecePosition);
        
    }
    private void HandleGameStateChange(LogicManager.GameState gameState)
    {
        bool isActive = true;
        switch (gameState)
        {
            
            //TODO 
            case(LogicManager.GameState.Paused):
                pausePanel.SetActive(isActive);
                beginPanel.SetActive(!isActive);
                gameOverPanel.SetActive(!isActive);
                break;
            case(LogicManager.GameState.GameOver):
                pausePanel.SetActive(!isActive);
                beginPanel.SetActive(!isActive);
                gameOverPanel.SetActive(isActive);
                break;
            case(LogicManager.GameState.InPlay):
                pausePanel.SetActive(!isActive);
                beginPanel.SetActive(!isActive);
                gameOverPanel.SetActive(!isActive);
                break;
            case(LogicManager.GameState.PreGame):
                pausePanel.SetActive(!isActive);
                beginPanel.SetActive(isActive);
                gameOverPanel.SetActive(!isActive);
                break;
        }
    }

    private void HandleGameUpdate()
    {
        ShowScore();
        ResetToIdle(logicManager.FixedPieces);
        DrawActivePiece(logicManager.ActivePiece, logicManager.ActivePieceRotation, logicManager.ActivePiecePosition);
        ResetNextPieceBoard();
        DrawNextPiece(logicManager.GetNextPieceInBag());
    }

    private void CreateField(Vector2Int gridSize , int[,] fixedBoard)
    {
        grid = new Tile[gridSize.y, gridSize.x];
        GameObject tileHolder = new GameObject(String.Format("TileHolder"));
        for (int x = 0; x < logicManager.GridSize.x; x++)
        {
            for (int y = 0; y < logicManager.GridSize.y; y++)
            {
                fixedBoard[y, x] = 0;
                
                grid[y,x].Object = new GameObject(string.Format("Tile " + x + " " + y),typeof(SpriteRenderer));
                grid[y,x].Object.transform.parent = tileHolder.transform;
                grid[y,x].SpriteRenderer = grid[y, x].Object.GetComponent<SpriteRenderer>();
                grid[y,x].SpriteRenderer.sprite = sprite;
                
                Transform tileTransform = grid[y,x].Object.transform;
                Vector3 position = tileTransform.position;
                
                position = new Vector3(position.x + x - (logicManager.GridSize.x * 0.5f) + 0.5f , position.y + y);
                tileTransform.position = position;
            }
        }
    }

    private void CreateNextPieceField()
    {
        nextPieceGrid = new Tile[4, 4];
        GameObject tileHolder = new GameObject(String.Format("NextPieceTiles"));
        for (int x = 0; x < nextPieceGrid.GetLength(1); x++)
        {
            for (int y = 0; y < nextPieceGrid.GetLength(0); y++)
            {
                nextPieceGrid[y,x].Object = new GameObject(string.Format("Tile " + x + " " + y),typeof(SpriteRenderer));
                nextPieceGrid[y, x].Object.transform.parent = tileHolder.transform;
                nextPieceGrid[y,x].SpriteRenderer = nextPieceGrid[y, x].Object.GetComponent<SpriteRenderer>();
                nextPieceGrid[y,x].SpriteRenderer.sprite = sprite;
                
                Transform tileTransform = nextPieceGrid[y,x].Object.transform;
                Vector3 position = showNextPiecePos.position;
                
                position = new Vector3(position.x + x ,position.y + y);
                tileTransform.position = position;
            }
        }        
    }

    private void ShowScore() => showScore.text = string.Format("Score " + logicManager.Score);
   
    private void ResetToIdle(int[,] fixedPieces)
    {
        
        for (int x = 0; x < logicManager.GridSize.x; x++)
        {
            for (int y = 0; y < logicManager.GridSize.y; y++)
            {
                if(fixedPieces[y,x] > 0) continue;
                grid[y, x].SpriteRenderer.color = tileColors[0];
            }
        }
    }

    private void ResetNextPieceBoard()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                nextPieceGrid[y,x].SpriteRenderer.color = Color.clear;
            }
        }
    }

    // TODO Improve the bag peek method and make a press space and exit mode and make a reset mode when you hit top
    //
    
    private void DrawActivePiece(int[,,] activePiece, int pieceRotation, Vector2Int piecePosition)
    {
        Vector2Int pieceSize = logicManager.GetPieceSize(activePiece);
        for (int x = 0; x < pieceSize.x; x++)
        {
            for (int y = 0; y < pieceSize.y; y++)
            {
                if (activePiece[pieceRotation, y, x] != 0)
                {
                    grid[piecePosition.y + y + 1, x + piecePosition.x].SpriteRenderer.color = tileColors[activePiece[pieceRotation, y, x]];
                }    
            }
        }
    }

    private void DrawNextPiece(int[,,] piece)
    {
        for (int x = 0; x < nextPieceGrid.GetLength(1); x++)
        {
            for (int y = 0; y < nextPieceGrid.GetLength(0); y++)
            {
                if (piece[0, y, x] != 0)
                    nextPieceGrid[y, x].SpriteRenderer.color = tileColors[piece[0, y, x]];
            }
        }
        
    }
    
    private void PauseGame(bool isPaused)
    {
        
    }
}
