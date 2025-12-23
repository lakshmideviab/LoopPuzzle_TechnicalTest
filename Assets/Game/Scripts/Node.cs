using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour
{
    [Header("Data")]
    // Connections: [Top, Right, Bottom, Left]
    [SerializeField] private bool[] connections = new bool[4];
    public bool isSource;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color offColor = Color.white;
    [SerializeField] private Color onColor = Color.yellow; // The "Win" light

    private bool isFixed;
    private bool isAnimating;

    // --- INITIALIZATION ---
    // UPDATE THIS METHOD IN NODE.CS
    public void Init(bool[] startConnections, bool _isFixed, float startRotation, Sprite image)
    {
        connections = startConnections;
        isFixed = _isFixed;

        // 1. Set the correct image!
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = image;
        }

        // 2. Set Rotation
        transform.localEulerAngles = new Vector3(0, 0, startRotation);

        // 3. Rotate Logic
        int rotations = Mathf.RoundToInt(startRotation / 90f);
        for (int i = 0; i < rotations; i++)
        {
            ShiftConnectionsArray();
        }
    }

    // --- VISUAL FEEDBACK (LIGHTS) ---
    public void SetPowered(bool isPowered)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPowered ? onColor : offColor;
        }
    }

    public bool[] GetConnections()
    {
        return connections;
    }

    // --- INTERACTION ---
    private void OnMouseDown()
    {
        if (isFixed || isAnimating) return;
        StartCoroutine(RotateRoutine());
    }

    // The "Vanilla" Coroutine (No DOTween needed!)
    private IEnumerator RotateRoutine()
    {
        isAnimating = true;

        // 1. Update the Math (Data)
        ShiftConnectionsArray();

        // 2. Animate the Rotation (Visuals)
        Quaternion startRot = transform.rotation;
        Quaternion endRot = transform.rotation * Quaternion.Euler(0, 0, -90);
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot; // Ensure we finish exactly at target

        isAnimating = false;

        // 3. Check for Win Condition
        if (GridManager.Instance != null)
        {
            GridManager.Instance.CheckConnections();
        }
    }

    private void ShiftConnectionsArray()
    {
        // Shifts array [0,1,2,3] -> [3,0,1,2] (Clockwise)
        bool last = connections[3];
        for (int i = 3; i > 0; i--)
        {
            connections[i] = connections[i - 1];
        }
        connections[0] = last;
    }
}