using System.Collections;
using UnityEngine;

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

public class SnowballProjectile : MonoBehaviour
{
    private float lifetime;

    public void Initialize(float newLifetime)
    {
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
            ScoreManager.Instance?.SaveScoreForCurrentPlayer();
            Destroy(purly.gameObject);
        }

        Destroy(gameObject);
    }
}
