using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TetrisGameManager : MonoBehaviour
{

    public const int EMPTY = 0;
    public const int I = 1;
    public const int J = 2;
    public const int L = 3;
    public const int O = 4;
    public const int S = 5;
    public const int T = 6;
    public const int Z = 7;

    public int[,] IBlock = new int[,] { { 0, 0 }, { 0, 1 }, { 0, 2 }, { 0, 3 } };
    public int[,] JBlock = new int[,] { { 1, 0 }, { 1, 1 }, { 1, 2 }, { 0, 0 } };
    public int[,] LBlock = new int[,] { { 0, 0 }, { 0, 1 }, { 0, 2 }, { 1, 0 } };
    public int[,] OBlock = new int[,] { { 0, 0 }, { 1, 0 }, { 0, 1 }, { 1, 1 } };
    public int[,] SBlock = new int[,] { { 0, 0 }, { 0, 1 }, { 1, 1 }, { -1, 0 } };
    public int[,] TBlock = new int[,] { { 1, 0 }, { 0, 0 }, { 0, 1 }, { -1, 0 } };
    public int[,] ZBlock = new int[,] { { -1, 1 }, { 0, 0 }, { 0, 1 }, { 1, 0 } };

    public const int gridWidth = 10;
    public const int gridHeight = 22;

    Tile tileEMPTY;
    Tile tileI;
    Tile tileJ;
    Tile tileL;
    Tile tileO;
    Tile tileS;
    Tile tileT;
    Tile tileZ;

    public float gameSpeed = 1f;
    public int[,] grid = new int[gridWidth, gridHeight];
    int[,] controlPoints = new int[4, 2];
    int currentBlock = EMPTY;
    int score = 0;
    bool gameOver = false;

    Tilemap tilemap;

    TetrisAgent agent;

    Text scoreText;

    private int placedHeight = 0;
    private bool placedInThisUpdate;
    private bool lineFilled;
    private int emptyCellsBellowPlaced = 0;
    // Use this for initialization
    void Awake()
    {

        tilemap = transform.GetComponentInChildren<Tilemap>();
        tileEMPTY = Resources.Load<Tile>("Tetris/EMPTYtile");
        tileI = Resources.Load<Tile>("Tetris/Itile");
        tileJ = Resources.Load<Tile>("Tetris/Jtile");
        tileL = Resources.Load<Tile>("Tetris/Ltile");
        tileO = Resources.Load<Tile>("Tetris/Otile");
        tileS = Resources.Load<Tile>("Tetris/Stile");
        tileT = Resources.Load<Tile>("Tetris/Ttile");
        tileZ = Resources.Load<Tile>("Tetris/Ztile");

        scoreText = transform.Find("Canvas/Score").GetComponent<Text>();
        agent = transform.Find("TetrisAgent").GetComponent<TetrisAgent>();
        agent.setGameManager(this);
        ResetGame();
    }


    float time = 0;
    int inputCD = 0; // debug for player input cooldown
    /*
    // Update is called once per frame
    void FixedUpdate()
    {
        if (inputCD > 0)
        {
            inputCD -= 1;
        }
        time += Time.fixedDeltaTime;
        if (gameSpeed == 0 || time >= 1 / gameSpeed)
        {
            if (gameSpeed == 0)
            {
                time -= 1 / gameSpeed;
            }
            updateLogic();
            scoreText.text = "Score: " + score;
            DrawGrid(grid);
        }
    }
    */

    void ResetEvents()
    {
        lineFilled = false;
        placedInThisUpdate = false;
        placedHeight = 0;
        emptyCellsBellowPlaced = 0;
    }

    bool updateGrid = true;
    public void updateGame(bool updateGrid)
    {
        this.updateGrid = updateGrid;
        ResetEvents();
        updateLogic();
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        if (updateGrid)
            DrawGrid(grid);
    }

    bool place = false;
    void updateLogic()
    {
        int[,] previousControlPoints = movePoints(controlPoints, new int[] { 0, 0 });

        // move down
        int[,] moveDown = movePoints(controlPoints, new int[] { 0, -1 });
        if (validPoints(moveDown))
        {
            controlPoints = moveDown;
        }
        else
        {
            int placeY = gridHeight;
            for (int i = 0; i < 4; i++)
            {
                if (previousControlPoints[i, 1] < placeY)
                {
                    placeY = previousControlPoints[i, 1];
                }
            }

            int emptyCount = 0;
            for (int y = placeY; y >= 0; y--)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[x, y] == EMPTY)
                    {
                        emptyCount++;
                    }
                }
            }

            placedInThisUpdate = true;
            emptyCellsBellowPlaced = emptyCount;
            placedHeight = placeY;
            place = true;
        }

        // clear previous
        for (int i = 0; i < 4; i++)
        {
            grid[previousControlPoints[i, 0], previousControlPoints[i, 1]] = EMPTY;
        }

        // add block at new positon
        for (int i = 0; i < 4; i++)
        {
            grid[controlPoints[i, 0], controlPoints[i, 1]] = currentBlock;
        }

        // if the piece has been placed, spawn a new piece
        if (place)
        {
            placedInThisUpdate = true;
            place = false;
            checkFullLines();
            previousControlPoints = movePoints(controlPoints, new int[] { 0, 0 });
            spawnPiece();
            if (!isClear(controlPoints))
            {
                //Debug.Log("Game over!");
                gameOver = true;
            }
        }

    }

    public void ResetGame()
    {
        score = 0;
        gameOver = false;
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = EMPTY;
            }
        }
        time = 1 / gameSpeed;
        spawnPiece();
    }

    void checkFullLines()
    {
        bool firstLine = true;
        int highestY = 0;
        int lowestY = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            bool allFilled = true;
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] == EMPTY)
                {
                    allFilled = false;
                    break;
                }
            }

            if (allFilled)
            {
                score++;
                lineFilled = true;
                if (firstLine)
                {
                    firstLine = false;
                    lowestY = y;
                }
                highestY = y;
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[x, y] = EMPTY;
                }
            }
        }

        if (!firstLine)
        {
            // if any line was filled, move all blocks above it down
            for (int y = lowestY; y < gridHeight; y++)
            {
                int off = y - lowestY;
                for (int x = 0; x < gridWidth; x++)
                {
                    if (highestY + 1 + off < gridHeight)
                    {
                        grid[x, y] = grid[x, highestY + 1 + off];
                    }
                    else
                    {
                        grid[x, y] = EMPTY;
                    }
                }
            }
        }
    }


    int[] spawnOffset = new int[] { 4, gridHeight - 4 };
    void spawnPiece()
    {
        int[,] block = IBlock;
        currentBlock = I;
        int r = Random.Range(0, 7);
        if (r == 1)
        {
            block = JBlock;
            currentBlock = J;
        }
        else if (r == 2)
        {
            block = LBlock;
            currentBlock = L;
        }
        else if (r == 3)
        {
            block = OBlock;
            currentBlock = O;
        }
        else if (r == 4)
        {
            block = SBlock;
            currentBlock = S;
        }
        else if (r == 5)
        {
            block = TBlock;
            currentBlock = T;
        }
        else if (r == 6)
        {
            block = ZBlock;
            currentBlock = Z;
        }
        controlPoints = movePoints(block, spawnOffset);
    }

    public void flip()
    {
        /*
        if (inputCD > 0)
        {
            return;
        }
        inputCD = 20;
        */
        if (currentBlock == O)
        {
            return;
        }
        int[,] flipped = movePoints(controlPoints, new int[] { -controlPoints[1, 0], -controlPoints[1, 1] });
        for (int i = 0; i < 4; i++)
        {
            int temp = flipped[i, 0];
            flipped[i, 0] = flipped[i, 1];
            flipped[i, 1] = -temp;
        }

        flipped = movePoints(flipped, new int[] { controlPoints[1, 0], controlPoints[1, 1] });

        if (validPoints(flipped))
        {
            // clear previous
            for (int i = 0; i < 4; i++)
            {
                grid[controlPoints[i, 0], controlPoints[i, 1]] = EMPTY;
            }
            controlPoints = flipped;
            // add block at new positon
            for (int i = 0; i < 4; i++)
            {
                grid[controlPoints[i, 0], controlPoints[i, 1]] = currentBlock;
            }
        }
        if (updateGrid)
            DrawGrid(grid);
    }

    public void moveRight()
    {
        int[,] movedPoints = movePoints(controlPoints, new int[] { 1, 0 });
        if (validPoints(movedPoints))
        {
            // clear previous
            for (int i = 0; i < 4; i++)
            {
                grid[controlPoints[i, 0], controlPoints[i, 1]] = EMPTY;
            }
            controlPoints = movedPoints;
            // add block at new positon
            for (int i = 0; i < 4; i++)
            {
                grid[controlPoints[i, 0], controlPoints[i, 1]] = currentBlock;
            }
        }
        if (updateGrid)
            DrawGrid(grid);
    }

    public void moveLeft()
    {
        int[,] movedPoints = movePoints(controlPoints, new int[] { -1, 0 });
        if (validPoints(movedPoints))
        {
            // clear previous
            for (int i = 0; i < 4; i++)
            {
                grid[controlPoints[i, 0], controlPoints[i, 1]] = EMPTY;
            }
            controlPoints = movedPoints;
            // add block at new positon
            for (int i = 0; i < 4; i++)
            {
                grid[controlPoints[i, 0], controlPoints[i, 1]] = currentBlock;
            }
        }
        if (updateGrid)
            DrawGrid(grid);
    }


    public void placeDown()
    {
        // move down
        bool canMoveDown = true;
        while (canMoveDown)
        {
            int[,] previousControlPoints = movePoints(controlPoints, new int[] { 0, 0 });
            int[,] moveDown = movePoints(controlPoints, new int[] { 0, -1 });
            if (validPoints(moveDown))
            {
                controlPoints = moveDown;
                // clear previous
                for (int i = 0; i < 4; i++)
                {
                    grid[previousControlPoints[i, 0], previousControlPoints[i, 1]] = EMPTY;
                }

                // add block at new positon
                for (int i = 0; i < 4; i++)
                {
                    grid[controlPoints[i, 0], controlPoints[i, 1]] = currentBlock;
                }
            }
            else
            {
                canMoveDown = false;
            }
        }
    }

    int[,] movePoints(int[,] points, int[] dir)
    {
        int[,] newPoints = new int[4, 2];
        for (int i = 0; i < 4; i++)
        {
            newPoints[i, 0] = points[i, 0] + dir[0];
            newPoints[i, 1] = points[i, 1] + dir[1];
        }
        return newPoints;
    }


    bool validPoints(int[,] points)
    {
        for (int i = 0; i < 4; i++)
        {
            if (points[i, 0] < 0
                || points[i, 0] >= grid.GetLength(0)
                || points[i, 1] < 0
                || points[i, 1] >= grid.GetLength(1))
            {
                return false;
            }

            if (grid[points[i, 0], points[i, 1]] != EMPTY && !hasPoint(controlPoints, new int[] { points[i, 0], points[i, 1] }))
            {
                return false;
            }
        }
        return true;
    }

    bool isClear(int[,] points)
    {
        for (int i = 0; i < 4; i++)
        {
            if (grid[points[i, 0], points[i, 1]] != EMPTY)
            {
                return false;
            }
        }
        return true;
    }

    bool hasPoint(int[,] points, int[] point)
    {
        for (int i = 0; i < 4; i++)
        {
            if (points[i, 0] == point[0] && points[i, 1] == point[1])
            {
                return true;
            }
        }
        return false;
    }

    bool hasSamePoint(int[,] points, int[,] points2)
    {
        for (int i = 0; i < 4; i++)
        {
            if (hasPoint(points, new int[] { points2[i, 0], points2[i, 1] }))
            {
                return true;
            }
        }
        return false;
    }

    public int[,] getData()
    {
        /*

        int[,] data = new int[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (hasPoint(controlPoints, new int[] { x, y }))
                {
                    data[x, y] = 2;
                }
                else if (grid[x, y] != EMPTY)
                {
                    data[x, y] = 1;
                }
                else
                {
                    data[x, y] = 0;
                }
            }
        }
        */
        
        int[,] data = new int[gridWidth + 1, 1];
        for (int x = 0; x < gridWidth; x++)
        {
            int lastY = 0;
            for (int y = 1; y < gridHeight; y++)
            {
                if (!hasPoint(controlPoints, new int[] { x, y }) && grid[x, y] != EMPTY)
                {
                    lastY = y;
                }
            }

            data[x, 0] = lastY;
        }
        data[gridWidth, 0] = currentBlock;
        
        return data;
    }

    public int[,] getDataForMoves()
    {
        int[,] data = new int[gridWidth, gridHeight];
        return data;
    }

    int[] drawOffset = new int[] { -3, -8 };
    void DrawGrid(int[,] map)
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x <= map.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= map.GetUpperBound(1); y++)
            {
                Tile tile = tileEMPTY;
                if (map[x, y] == I)
                {
                    tile = tileI;
                }
                else if (map[x, y] == J)
                {
                    tile = tileJ;
                }
                else if (map[x, y] == L)
                {
                    tile = tileL;
                }
                else if (map[x, y] == O)
                {
                    tile = tileO;
                }
                else if (map[x, y] == S)
                {
                    tile = tileS;
                }
                else if (map[x, y] == T)
                {
                    tile = tileT;
                }
                else if (map[x, y] == Z)
                {
                    tile = tileZ;
                }
                tilemap.SetTile(new Vector3Int(x + drawOffset[0], y + drawOffset[1], 0), tile);
            }
        }
    }

    public bool isGameOver()
    {
        return gameOver;
    }

    public TetrisAgent getAgent()
    {
        return agent;
    }

    public bool isLineFilled()
    {
        return lineFilled;
    }

    public bool isPlaced()
    {
        return placedInThisUpdate;
    }

    public int getEmptyCellsBellowPlaced()
    {
        return emptyCellsBellowPlaced;
    }

    public int getPlacedHeight()
    {
        return placedHeight;
    }

    public int getHoleCount(int[,] grid)
    {
        int r = 0;
        for (int x = 0; x <= grid.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= grid.GetUpperBound(1); y++)
            {
                if (grid[x, y] == EMPTY &&
                    (x - 1 < 0 || grid[x - 1, y] != EMPTY) &&
                    (y - 1 < 0 || grid[x, y - 1] != EMPTY) &&
                    (x + 1 > grid.GetUpperBound(0) || grid[x + 1, y] != EMPTY) &&
                    (y + 1 > grid.GetUpperBound(1) || grid[x, y + 1] != EMPTY))
                {
                    r++;
                }
            }
        }
        return r;
    }

    public int getBumpCount(int[,] grid)
    {
        int r = 0;
        int prev = 0;
        for (int x = 0; x <= grid.GetUpperBound(0); x++)
        {
            int bump = 0;
            for (int y = grid.GetUpperBound(1); y >= 0; y--)
            {
                if (grid[x, y] != EMPTY)
                {
                    bump = y;
                    break;
                }
            }
            if (x > 0)
            {
                r += Mathf.Abs(prev - bump);
            }
            prev = bump;
        }
        return r;
    }

    public int getFilledLines(int[,] grid)
    {
        int r = 0;
        for (int y = 0; y <= grid.GetUpperBound(1); y++)
        {
            bool filled = true;
            for (int x = 0; x <= grid.GetUpperBound(0); x++)
            {
                if (grid[x, y] == EMPTY)
                {
                    filled = false;
                    break;
                }
            }
            if (filled)
                r++;
        }
        return r;
    }
    
    private int[,] saveGrid = new int[gridWidth, gridHeight];
    private int[,] saveControlPoints = new int[4, 2];
    public void SaveState()
    {
        saveGrid = (int[,])grid.Clone();
        saveControlPoints = (int[,])controlPoints.Clone();
    }

    public void LoadState()
    {
        grid = saveGrid;
        controlPoints = saveControlPoints;
    }

    public int getScore()
    {
        return score;
    }

    public int[,] getGrid()
    {
        return grid;
    }

    public int getGridSize()
    {
        return gridWidth * gridHeight;
    }

    public int getGridHeight()
    {
        return gridHeight;
    }

    public int getGridWidth()
    {
        return gridHeight;
    }
}
