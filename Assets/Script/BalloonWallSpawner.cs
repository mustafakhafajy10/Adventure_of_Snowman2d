using System.Collections;
using UnityEngine;

public class BalloonWallSpawner : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float balloonScale = 0.1f;
    [SerializeField] private float popDistance = 1.2f;
    [SerializeField] private Vector2[] spawnPositions =
    {
        new Vector2(-61.6f, -36f),
        new Vector2(-49.89f, -35.79f),
        new Vector2(-55.44f, -32.94f),
        new Vector2(-55.4f, -37.94f)
    };

    private Sprite balloonSprite;
    private Material balloonMaterial;
    private bool[] respawnQueued;

    private void Start()
    {
        CacheBalloonVisuals();
        ClearExistingBalloons();
        SpawnInitialBalloons();
    }

    public void HandleBalloonPopped(int wallIndex)
    {
        if (wallIndex < 0 || wallIndex >= spawnPositions.Length || respawnQueued[wallIndex])
        {
            return;
        }

        StartCoroutine(RespawnBalloonAfterDelay(wallIndex));
    }

    private IEnumerator RespawnBalloonAfterDelay(int wallIndex)
    {
        respawnQueued[wallIndex] = true;
        yield return new WaitForSeconds(respawnDelay);
        SpawnBalloonOnWall(wallIndex);
        respawnQueued[wallIndex] = false;
    }

    private void CacheBalloonVisuals()
    {
        SpriteRenderer existingBalloonRenderer = FindFirstObjectByType<SpriteRenderer>();
        BalloonPop existingBalloon = FindFirstObjectByType<BalloonPop>();

        if (existingBalloon != null)
        {
            existingBalloonRenderer = existingBalloon.GetComponent<SpriteRenderer>();
        }

        if (existingBalloonRenderer != null)
        {
            balloonSprite = existingBalloonRenderer.sprite;
            balloonMaterial = existingBalloonRenderer.sharedMaterial;
        }
    }

    private void ClearExistingBalloons()
    {
        BalloonPop[] balloons = FindObjectsByType<BalloonPop>(FindObjectsSortMode.None);
        foreach (BalloonPop balloon in balloons)
        {
            if (balloon != null)
            {
                balloon.gameObject.SetActive(false);
                Destroy(balloon.gameObject);
            }
        }
    }

    private void SpawnInitialBalloons()
    {
        respawnQueued = new bool[spawnPositions.Length];

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            SpawnBalloonOnWall(i);
        }
    }

    private void SpawnBalloonOnWall(int wallIndex)
    {
        if (balloonSprite == null || wallIndex < 0 || wallIndex >= spawnPositions.Length)
        {
            return;
        }

        GameObject balloon = new GameObject($"Balloon_{wallIndex}");
        balloon.transform.position = spawnPositions[wallIndex];
        balloon.transform.localScale = Vector3.one * balloonScale;

        SpriteRenderer spriteRenderer = balloon.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = balloonSprite;
        spriteRenderer.sharedMaterial = balloonMaterial;
        spriteRenderer.sortingOrder = 5;

        CircleCollider2D circleCollider = balloon.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = 5f;

        BalloonPop balloonPop = balloon.AddComponent<BalloonPop>();
        balloonPop.Configure(this, wallIndex);
        balloonPop.SetPopDistance(popDistance);
    }
}
