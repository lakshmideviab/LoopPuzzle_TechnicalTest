using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "LoopGame/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Grid Settings")]
    public int rows;
    public int columns;

    [Header("Level Configuration")]
    // We represent the grid as a 1D array for easier serialization in Inspector
    // Row 0 is the top, Row 'rows-1' is the bottom
    public NodeData[] nodeLayout;
}

[System.Serializable]
public class NodeData
{
    public NodeType type;       // Straight, Curve, Cross, EndPoint, Source
    public float initialRotation; // 0, 90, 180, 270
    public bool isFixed;        // Can the player rotate this?
}

public enum NodeType
{
    Empty,
    Line,       // Straight line
    Corner,     // 90 degree turn
    T_Shape,    // 3 connections
    Cross,      // 4 connections
    Source      // The battery/start point
}