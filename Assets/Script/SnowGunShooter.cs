using System.Collections;
using UnityEngine;

// Alternates two snow guns so they fire snowballs into the arena one after the other.
public class SnowGunShooter : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 20f;
    [SerializeField] private float snowballSpeed = 6.5f;
    [SerializeField] private float snowballLifetime = 10f;
    [SerializeField] private float snowballRadius = 0.28f;
    [SerializeField] private float spawnOffset = 2f;
    [SerializeField] private int gunOrder;
    [SerializeField] private float targetWallX = -55.73f;
    [SerializeField] private float targetMinY = -37.94f;
    [SerializeField] private float targetMaxY = -32.94f;

    private static Sprite cachedSnowballSprite;
    private static int nextGunOrderToFire;
    private static bool cycleInitialized;

    private void Awake()
    {
        // Initialize the shared firing cycle once so all gun instances stay in sync.
        if (!cycleInitialized)
        {
            nextGunOrderToFire = 0;
            cycleInitialized = true;
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Wait until this gun's turn in the alternating fire sequence.
            yield return new WaitUntil(() => gunOrder == nextGunOrderToFire);
            yield return new WaitForSeconds(spawnInterval);
            SpawnSnowball();
            nextGunOrderToFire = 1 - gunOrder;
        }
    }

    private void SpawnSnowball()
    {
        Vector2 direction = GetShootDirection();
        GameObject snowball = new GameObject($"{name}_Snowball");
        snowball.transform.position = (Vector2)transform.position + (direction * spawnOffset);
        snowball.transform.localScale = Vector3.one * 0.6f;

        SpriteRenderer spriteRenderer = snowball.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSnowballSprite();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 6;

        CircleCollider2D circleCollider = snowball.AddComponent<CircleCollider2D>();
        // Trigger collision is enough; the projectile should not physically bounce off Purly.
        circleCollider.radius = snowballRadius;
        circleCollider.isTrigger = true;

        Rigidbody2D rb = snowball.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction * snowballSpeed;

        SnowballProjectile projectile = snowball.AddComponent<SnowballProjectile>();
        projectile.Initialize(snowballLifetime);
    }

    private Vector2 GetShootDirection()
    {
        float targetX = gunOrder == 0 ? -47.46f : -63.94f;
        // Shoot toward a random point on the opposite side of the arena.
        Vector2 targetPoint = new Vector2(targetX, Random.Range(targetMinY, targetMaxY));
        return (targetPoint - (Vector2)transform.position).normalized;
    }

    private static Sprite GetSnowballSprite()
    {
        if (cachedSnowballSprite != null)
        {
            return cachedSnowballSprite;
        }

        const int size = 64;
        // Build a simple white circle in code so the shooter does not depend on a separate sprite asset.
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        cachedSnowballSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return cachedSnowballSprite;
    }
}

// Moves with its Rigidbody2D and destroys Purly on contact.
public class SnowballProjectile : MonoBehaviour
{
    private float lifetime;

    public void Initialize(float newLifetime)
    {
        // Self-destruct later so missed shots do not accumulate forever.
        lifetime = newLifetime;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PurlyMovement purly = other.GetComponentInParent<PurlyMovement>();
        if (!other.CompareTag("Player") && purly == null)
        {
            return;
        }

        KillPurly(purly);
    }

    private void KillPurly(PurlyMovement purly)
    {
        if (purly != null)
        {
            // Save the score before the player object is destroyed.
            ScoreManager.Instance?.SaveScoreForCurrentPlayer();
            Destroy(purly.gameObject);
        }

        Destroy(gameObject);
    }
}
