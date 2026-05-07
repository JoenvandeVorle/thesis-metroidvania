using Metroidvania;
using Metroidvania.Characters.Knight;
using Metroidvania.Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TargetPathFinder : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject target;
    [SerializeField] private float pathUpdateRate = 3f;

    private Pathfinder pathfinderInstance;
    private Path currentPath;
    private float pathUpdateTimer;
    private KnightCharacterController character;

    void Start()
    {
        if (player == null || target == null)
        {
            Debug.LogError("Player or Target GameObject is not assigned!");
            return;
        }

        pathfinderInstance = Pathfinder.instance;
        character = player.GetComponent<KnightCharacterController>();

        UpdatePath();
    }

    void Update()
    {
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateRate)
        {
            if (!character.collisionChecker.isGrounded)
                return; // to prevent path updates mid-air

            UpdatePath();
            pathUpdateTimer = 0f;
        }
    }

    private void UpdatePath()
    {
        Vector2 playerPos = player.transform.position;
        Vector2 targetPos = target.transform.position;

        if (currentPath != null)
            pathfinderInstance.ReleasePath(ref currentPath);

        currentPath = pathfinderInstance.FindPathForPlayer(playerPos, targetPos);
        print(currentPath == null ? "No path found." : $"Path {currentPath} found.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (currentPath != null)
            new GizmosDrawer().SetColor(GizmosColor.instance.pathfinding.pathColor).DrawPath(currentPath);
    }
#endif
}
