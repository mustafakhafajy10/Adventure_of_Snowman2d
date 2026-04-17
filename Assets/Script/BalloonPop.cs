using UnityEngine;

public class BalloonPop : MonoBehaviour
{
    [SerializeField] private float popDistance = 1.2f;

    private BalloonWallSpawner wallSpawner;
    private int wallIndex = -1;
    private Transform purlyBody;

    private void Awake()
    {
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

        if (Vector2.Distance(transform.position, purlyBody.position) <= popDistance)
        {
            Pop(purlyBody.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
        wallSpawner = newWallSpawner;
        wallIndex = newWallIndex;
    }

    public void SetPopDistance(float newPopDistance)
    {
        popDistance = newPopDistance;
    }
}
