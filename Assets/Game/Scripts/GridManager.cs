using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Settings")]
    [Header("Art Assets")]
    public Sprite lineSprite;
    public Sprite cornerSprite;
  //  public Sprite tShapeSprite;
    public Sprite crossSprite;
    public Sprite sourceSprite;
    public LevelData currentLevel;
    public Node nodePrefab;
    public Transform boardParent;
    public float spacing = 1.0f; // Distance between nodes

    // The actual grid in memory [x,y]
    private Node[,] grid;
    private List<Node> sourceNodes = new List<Node>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (currentLevel != null)
        {
            GenerateGrid();
        }
    }

    public void GenerateGrid()
    {
        // 1. Cleanup old level
        foreach (Transform child in boardParent) Destroy(child.gameObject);
        sourceNodes.Clear();

        int rows = currentLevel.rows;
        int cols = currentLevel.columns;
        grid = new Node[cols, rows];

        // 2. Spawn Loop
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                SpawnNode(x, y, rows);
            }
        }

        // 3. Center the Camera
        CenterCamera(cols, rows);
    }

    private void SpawnNode(int x, int y, int totalRows)
    {
        int index = (y * currentLevel.columns) + x;
        if (index >= currentLevel.nodeLayout.Length) return;

        NodeData data = currentLevel.nodeLayout[index];
        if (data.type == NodeType.Empty) return;

        // Position Logic
        float posX = x * spacing;
        float posY = (totalRows - 1 - y) * spacing;
        Vector3 pos = new Vector3(posX, posY, 0);

        // Instantiate
        Node newNode = Instantiate(nodePrefab, pos, Quaternion.identity, boardParent);
        newNode.name = $"Node_{x}_{y}";

        // --- NEW: PICK THE SPRITE ---
        Sprite nodeImage = null;
        switch (data.type)
        {
            case NodeType.Line: nodeImage = lineSprite; break;
            case NodeType.Corner: nodeImage = cornerSprite; break;
           // case NodeType.T_Shape: nodeImage = tShapeSprite; break;
            case NodeType.Cross: nodeImage = crossSprite; break;
            case NodeType.Source: nodeImage = sourceSprite; break;
        }

        // Initialize with the Sprite!
        bool[] connections = GetConnections(data.type);
        newNode.Init(connections, data.isFixed, data.initialRotation, nodeImage);

        grid[x, y] = newNode;

        if (data.type == NodeType.Source)
        {
            newNode.isSource = true;
            sourceNodes.Add(newNode);
        }
    }
    private void CenterCamera(int cols, int rows)
    {
        // Simple math to put camera in middle of grid
        float width = cols * spacing;
        float height = rows * spacing;
        Camera.main.transform.position = new Vector3(
            (width / 2) - (spacing / 2),
            (height / 2) - (spacing / 2),
            -10f // Keep camera back
        );
    }

    // Helper: Define what shapes have what connections
    // Order: [Top, Right, Bottom, Left]
    private bool[] GetConnections(NodeType type)
    {
        switch (type)
        {
            case NodeType.Line: return new bool[] { true, false, true, false }; // Up/Down
            case NodeType.Corner: return new bool[] { true, true, false, false }; // Up/Right
           // case NodeType.T_Shape: return new bool[] { true, true, true, false }; // Up/Right/Down
            case NodeType.Cross: return new bool[] { true, true, true, true };    // All
            case NodeType.Source: return new bool[] { false, false, true, false }; // Down only
            default: return new bool[] { false, false, false, false };
        }
    }

    // ---------------------------------------------------------
    // REPLACE YOUR 'CheckConnections' WITH THIS LOGIC
    // ---------------------------------------------------------

    public void CheckConnections()
    {
        // 1. Reset: Turn off the lights for everyone first
        foreach (Node node in grid)
        {
            if (node != null) node.SetPowered(false);
        }

        // 2. Prepare the "Water"
        HashSet<Node> visitedNodes = new HashSet<Node>();

        // 3. Start the flow from every Battery/Source
        foreach (Node source in sourceNodes)
        {
            FloodFill(source, visitedNodes);
        }

        // 4. Win Condition: Did the water reach everyone?
        // We count how many nodes are lit up vs total nodes in the grid
        int totalNodes = currentLevel.rows * currentLevel.columns;

        if (visitedNodes.Count >= totalNodes)
        {
            Debug.Log("<color=green>WINNER! Level Complete!</color>");
            // TODO: Add a UI Popup here later (e.g., UIManager.Instance.ShowWinScreen();)
        }
    }

    // The Recursive "Flow" Logic
    private void FloodFill(Node currentNode, HashSet<Node> visited)
    {
        // Base Case: Stop if we've already visited this node to prevent infinite loops
        if (visited.Contains(currentNode)) return;

        // 1. Mark as visited
        visited.Add(currentNode);

        // 2. Turn the light ON (Visual Feedback)
        currentNode.SetPowered(true);

        // 3. Check all 4 neighbors
        // Directions: 0=Top, 1=Right, 2=Bottom, 3=Left
        bool[] myConnections = currentNode.GetConnections();
        Vector2Int myPos = FindNodePosition(currentNode);

        // Define the math for neighbors: [Top, Right, Bottom, Left]
        Vector2Int[] offsets = new Vector2Int[] {
            new Vector2Int(0, 1),  // Top
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, -1), // Bottom
            new Vector2Int(-1, 0)  // Left
        };

        for (int i = 0; i < 4; i++)
        {
            // Rule 1: Do I have a pipe pointing this way?
            if (!myConnections[i]) continue;

            // Calculate neighbor coordinate
            Vector2Int neighborPos = myPos + offsets[i];

            // Rule 2: Does the neighbor exist? (Are we at the edge of the map?)
            if (IsValidPosition(neighborPos))
            {
                Node neighbor = grid[neighborPos.x, neighborPos.y];

                // Rule 3: Does the neighbor connect BACK to me?
                // If I point Right (1), Neighbor must point Left (3).
                // Logic: (Direction + 2) % 4 gives the opposite direction.
                int neighborLookDir = (i + 2) % 4;

                if (neighbor.GetConnections()[neighborLookDir])
                {
                    // Connection is valid! Flow into that node.
                    FloodFill(neighbor, visited);
                }
            }
        }
    }

    // --- HELPERS ---

    // Finds the (x,y) of a specific node object
    private Vector2Int FindNodePosition(Node node)
    {
        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                if (grid[x, y] == node) return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1); // Should never happen
    }

    // Checks if a coordinate is inside the grid
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < currentLevel.columns &&
               pos.y >= 0 && pos.y < currentLevel.rows;
    }
}