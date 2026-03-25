using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCollider : MonoBehaviour
{
    [SerializeField] private float checkRadius = 0.4f;
    [SerializeField] private Transform visTransform;

    void Start()
    {
        visTransform.localScale = Vector3.one * checkRadius * 2f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePos;
        // Collider2D[] tiles = Physics2D.OverlapPointAll(mousePos, LayerMask.GetMask("Ground"));
        Collider2D[] tiles = Physics2D.OverlapCircleAll(mousePos, checkRadius, LayerMask.GetMask("Ground"));
        if (tiles.Length > 0)
        {
            Debug.Log($"Mouse at {mousePos} overlaps with {tiles.Length} ground tiles.");
            foreach (var tile in tiles)
            {
                Debug.DrawRay(mousePos, Vector3.up * 2f, Color.orange, 1f);
            }
        }
    }
}
