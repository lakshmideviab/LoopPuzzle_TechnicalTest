using UnityEngine;

// This allows you to right-click in Project view -> Create -> LoopGame -> Level Data
[CreateAssetMenu(fileName = "NewLevel", menuName = "LoopGame/Level Data")]
public class LevelData : ScriptableObject
{
    public int rows;
    public int columns;
    public NodeData[] nodeLayout; // One big list of all nodes
}

[System.Serializable]
public class NodeData
{
    public NodeType type;
    public float initialRotation; // 0, 90, 180, 270
    public bool isFixed;        // Can the player rotate this?
}

public enum NodeType
{
    Empty,
    Line,       // Straight line (I shape)
    Corner,     // Turn (L shape)
    Cross,      // All 4 directions (+)
    Source      // The Battery/Power source
}