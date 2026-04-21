using System.Collections;
using UnityEngine;

// Keeps one balloon active on each wall and respawns popped balloons after a delay.
public class BalloonWallSpawner : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float balloonScale = 0.1f;
    [SerializeField] private float popDistance = 1.2f;
    [SerializeField] private int wallCount = 4;

    private Sprite balloonSprite;
    private Material balloonMaterial;
    private bool[] respawnQueued;

    private void Start()
    {
        // Reuse the look of any existing balloon in the scene, then rebuild the wall set from scratch.
        CacheBalloonVisuals();
        ClearExistingBalloons();
        SpawnInitialBalloons();
    }

    public void HandleBalloonPopped(int wallIndex)
    {
        if (wallIndex < 0 || wallIndex >= wallCount || respawnQueued[wallIndex])
        {
            return;
        }

        // Start the 2-second wait before replacing the balloon on this wall.
        StartCoroutine(RespawnBalloonAfterDelay(wallIndex));
    }

    private IEnumerator RespawnBalloonAfterDelay(int wallIndex)
    {
        respawnQueued[wallIndex] = true;
        yield return new WaitForSeconds(respawnDelay);
        // After the delay, spawn a new balloon back on the same wall.
        SpawnBalloonOnWall(wallIndex);
        respawnQueued[wallIndex] = false;
    }

    private void CacheBalloonVisuals()
    {
        // Grab the sprite/material once so newly spawned balloons match the original art setup.
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
        // Remove any hand-placed balloons so the spawner becomes the single source of truth.
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
        // Track which wall slots already have a respawn timer running.
        respawnQueued = new bool[wallCount];

        for (int i = 0; i < wallCount; i++)
        {
            SpawnBalloonOnWall(i);
        }
    }

    private void SpawnBalloonOnWall(int wallIndex)
    {
        if (balloonSprite == null || wallIndex < 0 || wallIndex >= wallCount)
        {
            return;
        }

        GameObject balloon = new GameObject($"Balloon_{wallIndex}");
        // Use a different random point each time the balloon respawns.
        balloon.transform.position = BalloonPop.GetRandomPositionOnWall(wallIndex);
        balloon.transform.localScale = Vector3.one * balloonScale;

        SpriteRenderer spriteRenderer = balloon.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = balloonSprite;
        spriteRenderer.sharedMaterial = balloonMaterial;
        spriteRenderer.sortingOrder = 5;

        CircleCollider2D circleCollider = balloon.AddComponent<CircleCollider2D>();
        // Trigger-only so the player can overlap the balloon without being blocked by it.
        circleCollider.isTrigger = true;
        circleCollider.radius = 5f;

        BalloonPop balloonPop = balloon.AddComponent<BalloonPop>();
        balloonPop.Configure(this, wallIndex);
        balloonPop.SetPopDistance(popDistance);
    }
}
