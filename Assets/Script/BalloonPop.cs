using UnityEngine;

// Handles balloon pop detection and notifies the wall spawner to replace it later.
public class BalloonPop : MonoBehaviour
{
    [SerializeField] private float popDistance = 1.2f;

    private static readonly Vector2[] wallMinPositions =
    {
        new Vector2(-61.6f, -37.6f),
        new Vector2(-49.89f, -37.6f),
        new Vector2(-60.8f, -32.94f),
        new Vector2(-60.8f, -37.94f)
    };

    private static readonly Vector2[] wallMaxPositions =
    {
        new Vector2(-61.6f, -33.3f),
        new Vector2(-49.89f, -33.3f),
        new Vector2(-50.4f, -32.94f),
        new Vector2(-50.4f, -37.94f)
    };

    private BalloonWallSpawner wallSpawner;
    private int wallIndex = -1;
    private Transform purlyBody;

    private void Awake()
    {
        // Prefer the visible body child so distance checks feel centered on the character.
        PurlyMovement purly = FindFirstObjectByType<PurlyMovement>();
        if (purly != null)
        {
            purlyBody = purly.transform.Find("Body_Middle");
            if (purlyBody == null)
            {
                purlyBody = purly.transform;
            }
        }
    }

    private void Update()
    {
        if (purlyBody == null)
        {
            return;
        }

        // Pop the balloon when Purly gets close enough in world space.
        if (Vector2.Distance(transform.position, purlyBody.position) <= popDistance)
        {
            Pop(purlyBody.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Allow either the tagged player collider or one of Purly's child colliders to trigger a pop.
        if (!other.CompareTag("Player") && other.GetComponentInParent<PurlyMovement>() == null)
        {
            return;
        }

        Pop(other.name);
    }

    private void Pop(string otherName)
    {
        Debug.Log("Triggered by: " + otherName);

        ScoreManager.Instance?.IncrementScore();

        if (wallSpawner != null && wallIndex >= 0)
        {
            wallSpawner.HandleBalloonPopped(wallIndex);
        }

        Destroy(gameObject);
    }

    public void Configure(BalloonWallSpawner newWallSpawner, int newWallIndex)
    {
        // Called by the spawner so this balloon knows which wall slot it belongs to.
        wallSpawner = newWallSpawner;
        wallIndex = newWallIndex;
    }

    public void SetPopDistance(float newPopDistance)
    {
        popDistance = newPopDistance;
    }

    public static Vector2 GetRandomPositionOnWall(int wallIndex)
    {
        if (wallIndex < 0 || wallIndex >= wallMinPositions.Length)
        {
            return Vector2.zero;
        }

        Vector2 min = wallMinPositions[wallIndex];
        Vector2 max = wallMaxPositions[wallIndex];

        // Pick a random spawn point inside the allowed range for this wall.
        return new Vector2(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y));
    }
}
