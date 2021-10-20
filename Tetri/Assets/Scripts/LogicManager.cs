
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class LogicManager : MonoBehaviour, ILogicManager
{

	public enum GameState
	{
		None,
		PreGame,
		InPlay,
		Paused,
		GameOver
	}

	public delegate void GameStateChangeEventHandler(GameState gameState);

	public delegate void GameUpdateEventHandler();

	public event GameStateChangeEventHandler GameStateChange;
	public event GameUpdateEventHandler GameUpdate;

	[SerializeField] private Vector2Int gridSize = new Vector2Int(10, 24);
	[Header("Controls")] 
	[SerializeField] private KeyCode moveLeftKey = KeyCode.LeftArrow;
	[SerializeField] private KeyCode moveRightKey = KeyCode.RightArrow;
	[SerializeField] private KeyCode softDropKey = KeyCode.DownArrow;
	[SerializeField] private KeyCode hardDropKey = KeyCode.Space;
	[SerializeField] private KeyCode rotateKey = KeyCode.UpArrow;
	[SerializeField] private KeyCode startGameKey = KeyCode.Space;
	[SerializeField] private KeyCode pauseGameKey = KeyCode.P;

	private float time;
	[SerializeField]private float timer;
	[SerializeField]private float timerSpeed;
	[SerializeField] private float timerDefault;
	private float timerSoftDropSpeed;

	private int[,,] iPiece = new int[,,] // 1
	{
		{
			{0, 0, 0, 0},
			{1, 1, 1, 1},
			{0, 0, 0, 0},
			{0, 0, 0, 0}
		},
		{
			{0, 0, 1, 0},
			{0, 0, 1, 0},
			{0, 0, 1, 0},
			{0, 0, 1, 0}
		}
	};

	private int[,,] jPiece = new int[,,] // 2
	{
		{
			{2, 0, 0,0},
			{2, 2, 2,0},
			{0, 0, 0,0},
			{0,0,0,0}
		},
		{
			{0, 2, 2, 0},
			{0, 2, 0, 0},
			{0, 2, 0, 0},
			{0,0,0,0}
		},
		{
			{0, 0, 0, 0},
			{2, 2, 2, 0},
			{0, 0, 2, 0},
			{0,0,0,0}
		},
		{
			{0, 2, 0,0},
			{0, 2, 0,0},
			{2, 2, 0,0},
			{0,0,0,0}
		}
	};

	private int[,,] lPiece = new int[,,] // 3
	{
		{
			{0, 0, 3, 0},
			{3, 3, 3, 0},
			{0, 0, 0, 0},
			{0,0,0,  0}
		},
		{
			{3, 0, 0,0},
			{3, 0, 0,0},
			{3, 3, 0,0},
			{0,0,0,0}
		},
		{
			{0, 0, 0,0},
			{3, 3, 3,0},
			{3, 0, 0,0},
			{0,0,0,0}
		},
		{
			{3, 3, 0,0},
			{0, 3, 0,0},
			{0, 3, 0,0},
			{0,0,0,0}
		}
	};

	private int[,,] oPiece = new int[,,] // 4
	{
		{
			{0,0,0,0},
			{0, 4,4,0},
			{0, 4,4,0},
			{0,0,0,0}
		}
	};

	private int[,,] sPiece = new int[,,] //5
	{
		{
			{0, 5, 5,0},
			{5, 5, 0,0},
			{0, 0, 0,0},
			{0,0,0,0}
		},
		{
			{0, 5, 0,0},
			{0, 5, 5,0},
			{0, 0 ,5,0},
			{0,0,0,0}
		}
	};
	
	private int[,,] zPiece = new int[,,] // 6 
	{
		{
			{6, 6, 0,0},
			{0, 6, 6,0},
			{0, 0, 0,0},
			{0,0,0,0}
		},
		{
			{0, 0, 6,0},
			{0, 6, 6,0},
			{0, 6, 0,0},
			{0,0,0,0}
		}
	};

	private int[,,] tPiece = new int[,,] // 7
	{
		{
			{0, 7, 0,0},
			{7, 7, 7,0},
			{0, 0, 0,0},
			{0,0,0,0}
		},
		{
			{0, 7, 0,0},
			{0, 7, 7,0},
			{0, 7, 0,0},
			{0,0,0,0}
		},
		{
			{0, 0, 0,0},
			{7, 7, 7,0},
			{0, 7, 0,0},
			{0,0,0,0}
		},
		{
			{0, 7, 0,0},
			{7, 7, 0,0},
			{0, 7, 0,0},
			{0,0,0,0}
		}
	};

	private int[][,,] allPieces;

	public Vector2Int GridSize => gridSize;
	public int[,] FixedPieces { get; private set; }
	public int[,,]  ActivePiece { get; private set; }
	public Vector2Int ActivePiecePosition { get; private set; }
	public int ActivePieceRotation { get; private set; }
	public GameState CurrentGameState { get; private set; }
	public int Level { get; private set; }
	public int Score { get; private set; }

	private Bag<int> bag = new Bag<int>(new int[]{0,1,2,3,4,5,6});

	private readonly int[] scorePerLineClears = new int[]
	{
		100,
		300,
		500,
		800  
	};

	
	
	public Vector2Int GetActivePieceHardDropPosition() {
		throw new System.NotImplementedException();
	}

	public int[,,] GetNextPieceInBag()
	{
		return allPieces[bag.Peek()];
	}
	
	private void Awake() {
		AwakeActions();	
	}

	private void AwakeActions()
	{
		allPieces = new int[][,,] {
			iPiece,
			jPiece,
			lPiece,
			oPiece,
			sPiece,
			zPiece,
			tPiece
		};
		Level = 0;
		timerSoftDropSpeed = timerSpeed * 2;
		timerDefault = timerSpeed;
		FixedPieces = new int[GridSize.y, GridSize.x];
		ActivePiecePosition = new Vector2Int(4, 2);
		ActivePieceRotation = 0;
		bag.Shuffle();
		ActivePiece = allPieces[bag.Next()];
		GameStateChange?.Invoke(GameState.PreGame);
	}

	private void Update() {
		// use this for getkeydowns and piece auto fall counter
		var convertPiece = ConvertPiece(ActivePiece, ActivePieceRotation);
		timer += timerSpeed * Time.deltaTime;
		if (timer >= 1f)
		{
			if (!OverlapBoundsCheck(convertPiece, ActivePiecePosition))
			{
				ActivePiecePosition += Vector2Int.down;
			}
			else
			{
				NextPiece();
			}
			GameUpdate?.Invoke();
			timer = 0;
		}

		Movement(convertPiece);
		
	}

	private void ResetGame() {
		FixedPieces = new int[gridSize.y, gridSize.x];
		ChangeGameState(GameState.PreGame);
	}

	private void ChangeGameState(GameState gameState) {
		if (CurrentGameState != gameState) {
			CurrentGameState = gameState;
			GameStateChange?.Invoke(CurrentGameState);
		}
	}

	public Vector2Int GetPieceSize(int[,,] piece) {
		return new Vector2Int(piece.GetLength(2), piece.GetLength(1));
	}

	private void Movement(int[,] piece)
	{
		if ( Input.GetKeyDown(moveLeftKey) && !OverlapBoundsCheck(piece,ActivePiecePosition + Vector2Int.left))
		{
			ActivePiecePosition += Vector2Int.left;
			GameUpdate?.Invoke();
		}

		if (Input.GetKeyDown(moveRightKey) && !OverlapBoundsCheck(piece, ActivePiecePosition + Vector2Int.right))
		{
			ActivePiecePosition += Vector2Int.right;
			GameUpdate?.Invoke();			
		}

		if (Input.GetKeyDown(rotateKey) && !CanRotateCheck(ActivePieceRotation, ActivePiecePosition))
		{ 
			ActivePieceRotation++;
			if (ActivePieceRotation >= ActivePiece.GetLength(0))
			{
				ActivePieceRotation = 0;
			}
			GameUpdate?.Invoke();
		}

		if (Input.GetKey(softDropKey))
			timerSpeed = timerSoftDropSpeed;
		else
			timerSpeed = timerDefault;
	}

	private void NextPiece()
	{
		SetFixedPiece();
		HandleFullRows();
		if (GameOver()) CurrentGameState = GameState.GameOver;
		GameStateChange?.Invoke(CurrentGameState);
		ActivePiecePosition = new Vector2Int(4, gridSize.y - 4);
		ActivePiece = allPieces[bag.Next()];
		ActivePieceRotation = 0;
		Debug.Log("completed");
	}

	private void SetFixedPiece()
	{
		Vector2Int pieceSize = GetPieceSize(ActivePiece);
		for (int x = 0; x < pieceSize.x; x++)
		{
			for (int y = 0; y < pieceSize.y; y++)
			{
				if (ActivePiece[ActivePieceRotation, y, x] != 0)
					FixedPieces[ActivePiecePosition.y + y + 1, x + ActivePiecePosition.x] = ActivePiece[ActivePieceRotation, y, x];
			}
		}
	}

	private void HandleFullRows()
	{
		int rowsCleared = 0;
		for (int y = 0; y < gridSize.y; y++)
		{
			for (int x = 0; x < gridSize.x; x++)
			{
				if (FixedPieces[y,x] == 0)
				{
					break;
				}
				if (x == gridSize.x - 1)
				{
					Debug.Log("complete");
					RemoveRow(y);
					rowsCleared++;
					y--;
				}
			}
		}

		if (rowsCleared > 0)
		{
			Score += scorePerLineClears[rowsCleared - 1] * Level;
		}
	}

	private void RemoveRow(int rowIndex)
	{
		for (int i = 0; i < gridSize.x; i++)
		{
			FixedPieces[rowIndex, i] = 0;
		}

		for (int y = rowIndex; y < gridSize.y; y++)
		{
			for (int x = 0; x < gridSize.x; x++ )
			{
				if(y >= gridSize.y - 1) break;
				FixedPieces[y, x] = FixedPieces[y + 1, x];
			}
		}
	}
	
	private int[,] ConvertPiece(int[,,] piece, int rotIndex)
	{
		Vector2Int pieceSize = GetPieceSize(piece);
		int[,] convertedPiece = new int[pieceSize.y, pieceSize.x];
		for (int x = 0; x < pieceSize.x; x++)
		{
			for (int y = 0; y < pieceSize.y; y++)
			{
				convertedPiece[y, x] = piece[rotIndex, y, x];
			}
		}
		return convertedPiece;
	}

	private Vector2Int[] GetPiecePositions(int[,] conPiece, Vector2Int pos)
	{
		List<Vector2Int> positions = new List<Vector2Int>();
		int xSize = conPiece.GetLength(1);
		int ySize = conPiece.GetLength(0);

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				if (conPiece[y, x] != 0)
				{
					positions.Add(new Vector2Int(x,y) + pos);
				}
			}
		}

		return positions.ToArray();
	}

	private bool OverlapBoundsCheck(int[,] piece , Vector2Int pos)
	{
		Vector2Int[] positions = GetPiecePositions(piece, pos);
		for (int i = 0; i < positions.Length; i++)
		{
			Vector2Int coord = positions[i];
			if (coord.y < 0 || coord.x < 0 || coord.x > gridSize.x - 1 || FixedPieces[coord.y, coord.x] != 0)
				return true;
		}
		return false;
	}

	private bool CanRotateCheck( int rotIndex, Vector2Int pos)
	{
		int nextRotIndex = rotIndex + 1; 
		if (nextRotIndex >= ActivePiece.GetLength(0))
		{
			nextRotIndex = 0;
		}
		
		int[,] nextConPiece = ConvertPiece(ActivePiece, nextRotIndex);
		Vector2Int[] positions = GetPiecePositions(nextConPiece, pos);

		for (int i = 0; i < positions.Length; i++)
		{
			Vector2Int coord = positions[i];
			if (coord.x < 0 || coord.x > gridSize.x - 1 || FixedPieces[coord.y, coord.x] != 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool GameOver()
	{
		for (int y = gridSize.y - 2; y < gridSize.y; y++)
		{
			for (int x = 0; x < gridSize.x; x++)
			{
				if (FixedPieces[y, x] != 0) return true;
			}
		}
		return false;
	}


	private void ResetTheGame()
	{
		Score = 0;
		Level = 0;
		for (int y = 0; y < gridSize.y; y++)
		{
			for (int x = 0; x < gridSize.x; x++)
			{
				FixedPieces[y, x] = 0;
			}
		}
	}

	
	
	
}

