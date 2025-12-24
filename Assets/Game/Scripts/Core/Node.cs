using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Node : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image nodeImage;
    [SerializeField] private Color offColor = Color.gray;
    [SerializeField] private Color onColor = Color.yellow;
    [SerializeField] private ParticleSystem bulbParticles;

    private bool[] connections; // [0:Top, 1:Right, 2:Bottom, 3:Left]
    public bool isSource { get; private set; }
    public bool isBulb { get; private set; }
    public Vector2Int GridPos { get; private set; }

    private bool isFixed;
    private bool isAnimating;

    public void Init(NodeType type, float startRotation, bool _isFixed, Sprite img, Vector2Int pos)
    {
        GridPos = pos;
        isFixed = _isFixed;
        isSource = type == NodeType.Source;
        isBulb = type == NodeType.Bulb;
        if (nodeImage != null) nodeImage.sprite = img;

        connections = GetBaseConnections(type);

        // Apply visual rotation
        transform.localEulerAngles = new Vector3(0, 0, startRotation);

        // Sync logical array with starting rotation
        // A -90 rotation (clockwise) moves index 0 to 1.
        int turns = Mathf.RoundToInt((360 - (startRotation % 360)) / 90f) % 4;
        for (int i = 0; i < turns; i++) ShiftConnections();
    }

    private bool[] GetBaseConnections(NodeType type)
    {
        return type switch
        {
            NodeType.Line => new bool[] { false, true, false, true },   // Right, Left
            NodeType.Corner => new bool[] { false, false, true, true }, // Top, Right
            NodeType.Cross => new bool[] { true, true, true, true },
            NodeType.Source => new bool[] { false, true, false, false }, // Points Right
            NodeType.Bulb => new bool[] { true, true, true, true },   // Receives Left
            _ => new bool[] { false, false, false, false }
        };
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isFixed || isAnimating || GridManager.Instance.IsLevelFinished) return;
        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        isAnimating = true;
        ShiftConnections();

        Quaternion startRot = transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, -90);
        float elapsed = 0, duration = 0.15f;

        while (elapsed < duration)
        {
            transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = endRot;
        isAnimating = false;
        GridManager.Instance.CheckConnections();
    }

    private void ShiftConnections()
    {
        bool last = connections[3];
        for (int i = 3; i > 0; i--) connections[i] = connections[i - 1];
        connections[0] = last;
    }

    public void SetPowered(bool state)
    {
        nodeImage.color = state ? onColor : offColor;
        // Trigger particles if this is a bulb and it just got powered
        if (isBulb && state && bulbParticles != null)
        {
            if (!bulbParticles.isPlaying)
            {
                bulbParticles.Play();
                // Optional: Add a small screen shake or sound here
                //AudioManager.Instance.PlaySFX(AudioManager.Instance.powerOn);
            }
        }
        else if (isBulb && !state && bulbParticles != null)
        {
            bulbParticles.Stop();
        }
    }

        public bool[] GetConnections() => connections;
}