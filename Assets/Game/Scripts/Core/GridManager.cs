using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Layout")]
    public GridLayoutGroup gridLayout;
    public float totalGridSize = 175f;   
    public float cellSize = 35f;          

    [Header("Prefabs & Data")]
    public Node nodePrefab;
    public LevelData[] levels;

    [Header("Sprites")]
    public Sprite lineImg, cornerImg, crossImg, sourceImg, bulbImg, emptyimg;

    [Header("UI")]
    public GameObject Winpanel;

    private Node[,] grid;
    private int currentLevelIndex = 0;
    public bool IsLevelFinished { get; private set; }

    void Awake() => Instance = this;
    void Start() => LoadLevel(0);

    public void LoadLevel(int index)
    {
        if (index >= levels.Length) return;

        currentLevelIndex = index;
        IsLevelFinished = false;
        GenerateGrid(levels[index]);
    }

    private void CalculateCellSize(LevelData data)
    {
        
        int maxCount = Mathf.Max(data.columns, data.rows);
        cellSize = totalGridSize / maxCount;

        gridLayout.cellSize = new Vector2(cellSize, cellSize);
    }

    private void GenerateGrid(LevelData data)
    {
        foreach (Transform child in gridLayout.transform)
            Destroy(child.gameObject);

       
        CalculateCellSize(data);

        grid = new Node[data.columns, data.rows];
        gridLayout.constraintCount = data.columns;

        for (int y = 0; y < data.rows; y++)
        {
            for (int x = 0; x < data.columns; x++)
            {
                int i = y * data.columns + x;
                Node node = Instantiate(nodePrefab, gridLayout.transform);

                node.Init(
                    data.nodeLayout[i].type,
                    data.nodeLayout[i].initialRotation,
                    data.nodeLayout[i].isFixed,
                    GetSprite(data.nodeLayout[i].type),
                    new Vector2Int(x, y)
                );

                grid[x, y] = node;
            }
        }

        CheckConnections();
    }

    public void CheckConnections()
    {
        if (IsLevelFinished) return;

        foreach (var n in grid)
            if (n != null) n.SetPowered(false);

        HashSet<Node> powered = new HashSet<Node>();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y] != null && grid[x, y].isSource)
                    TraceFlow(grid[x, y], powered);
            }
        }

        bool win = true;
        bool hasBulb = false;

        foreach (var n in grid)
        {
            if (n != null && n.isBulb)
            {
                hasBulb = true;
                if (!powered.Contains(n)) win = false;
            }
        }

        if (hasBulb && win)
            StartCoroutine(WinSequence());
    }

    private void TraceFlow(Node node, HashSet<Node> visited)
    {
        if (visited.Contains(node)) return;

        visited.Add(node);
        node.SetPowered(true);

        Vector2Int[] dirs =
        {
            new Vector2Int(0, -1), // Top
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, 1),  // Bottom
            new Vector2Int(-1, 0)  // Left
        };

        bool[] conns = node.GetConnections();

        for (int i = 0; i < 4; i++)
        {
            if (!conns[i]) continue;

            Vector2Int next = node.GridPos + dirs[i];

            if (next.x >= 0 && next.x < grid.GetLength(0) &&
                next.y >= 0 && next.y < grid.GetLength(1))
            {
                Node neighbor = grid[next.x, next.y];
                if (neighbor != null && neighbor.GetConnections()[(i + 2) % 4])
                {
                    TraceFlow(neighbor, visited);
                }
            }
        }
    }

    private IEnumerator WinSequence()
    {
        IsLevelFinished = true;
        Debug.Log("LEVEL COMPLETE!");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.levelWin);

        int nextLevelIndex = currentLevelIndex + 1;
        int highestReached = PlayerPrefs.GetInt("ReachedLevel", 0);

        if (nextLevelIndex > highestReached)
        {
            PlayerPrefs.SetInt("ReachedLevel", nextLevelIndex);
            PlayerPrefs.Save();
        }

        Winpanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        Winpanel.SetActive(false);

        LoadLevel(currentLevelIndex + 1);
    }

    private Sprite GetSprite(NodeType t) => t switch
    {
        NodeType.Line => lineImg,
        NodeType.Empty => emptyimg,
        NodeType.Corner => cornerImg,
        NodeType.Cross => crossImg,
        NodeType.Source => sourceImg,
        NodeType.Bulb => bulbImg,
        _ => null
    };

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || grid == null) return;

        Gizmos.color = Color.green;
        float gizmoLength = cellSize * 0.3f;

        foreach (var n in grid)
        {
            if (n == null || !n.gameObject.activeSelf) continue;

            bool[] c = n.GetConnections();
            Vector3 p = n.transform.position;

            if (c[0]) Gizmos.DrawLine(p, p + Vector3.up * gizmoLength);
            if (c[1]) Gizmos.DrawLine(p, p + Vector3.right * gizmoLength);
            if (c[2]) Gizmos.DrawLine(p, p + Vector3.down * gizmoLength);
            if (c[3]) Gizmos.DrawLine(p, p + Vector3.left * gizmoLength);
        }
    }
}
