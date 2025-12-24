using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "LoopGame/Level Data")]
public class LevelData : ScriptableObject
{
    public int rows;
    public int columns;
    public NodeData[] nodeLayout; // Rows * Columns size
}

[System.Serializable]
public class NodeData
{
    public NodeType type;
    public float initialRotation; // 0, 90, 180, 270
    public bool isFixed;          // If true, player can't rotate it
}

public enum NodeType { Empty, Line, Corner, Cross, Source, Bulb }