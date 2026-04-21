using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PurlyMovementModel))]
// Applies the movement model to the Rigidbody2D while preventing movement through colliders.
public class PurlyMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PurlyMovementModel movementModel;
    private Transform rotatingChild;
    private ContactFilter2D movementContactFilter;
    private readonly RaycastHit2D[] movementHits = new RaycastHit2D[8];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementModel = GetComponent<PurlyMovementModel>();
        // Rotate only the visible middle body piece, not the whole root object.
        rotatingChild = transform.Find(movementModel.RotatingChildName);

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        movementContactFilter = new ContactFilter2D
        {
            useTriggers = false
        };
        // Match the collision matrix of Purly's layer when checking future movement.
        movementContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            Vector2 movementDelta = movementModel.MoveInput * movementModel.Speed * Time.fixedDeltaTime;
            Vector2 allowedDelta = GetAllowedMovement(movementDelta);
            rb.MovePosition(rb.position + allowedDelta);
        }

        if (rotatingChild != null && movementModel.RotationInput != 0f)
        {
            rotatingChild.Rotate(0f, movementModel.RotationInput * movementModel.RotationSpeed * Time.fixedDeltaTime, 0f);
        }
    }

    private Vector2 GetAllowedMovement(Vector2 movementDelta)
    {
        float distance = movementDelta.magnitude;
        if (distance <= 0f)
        {
            return Vector2.zero;
        }

        Vector2 direction = movementDelta / distance;
        // Cast ahead before moving so Purly stops just short of the obstacle instead of clipping into it.
        int hitCount = rb.Cast(direction, movementContactFilter, movementHits, distance + collisionBuffer);
        if (hitCount == 0)
        {
            return movementDelta;
        }

        float allowedDistance = distance;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = movementHits[i];
            if (hit.collider == null)
            {
                continue;
            }

            allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, hit.distance - collisionBuffer));
        }

        return direction * allowedDistance;
    }

    private float collisionBuffer => movementModel != null ? movementModel.CollisionBuffer : 0.02f;
}
