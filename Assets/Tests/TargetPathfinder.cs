using System.Collections.Generic;
using Metroidvania;
using Metroidvania.Characters.Knight;
using Metroidvania.Pathfinding;
using UnityEngine;

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

    // TODO:: when following path, can differentiate between hor jump and ver jump by looking at
    // next 4 nodes and checking the max y difference.
    public List<Vector2> GetCurrentPath() => currentPath?.vectorPath;

    private void UpdatePath()
    {
        Vector2 playerPos = player.transform.position;
        Vector2 targetPos = target.transform.position;

        if (currentPath != null)
            pathfinderInstance.ReleasePath(ref currentPath);

        currentPath = pathfinderInstance.FindPathForPlayer(playerPos, targetPos);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (currentPath != null)
            new GizmosDrawer().DrawPathNodes(currentPath);
    }
#endif
}
