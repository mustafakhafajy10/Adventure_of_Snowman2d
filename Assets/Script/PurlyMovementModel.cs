using UnityEngine;

// Stores movement tuning values and the latest player input in one shared place.
public class PurlyMovementModel : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private string rotatingChildName = "Body_Middle";
    [SerializeField] private float collisionBuffer = 0.02f;

    public float Speed => speed;
    public float RotationSpeed => rotationSpeed;
    public string RotatingChildName => rotatingChildName;
    public float CollisionBuffer => collisionBuffer;
    public Vector2 MoveInput { get; private set; }
    public float RotationInput { get; private set; }

    public void SetMoveInput(Vector2 input)
    {
        // Clamp diagonals to avoid moving faster on two keys at once.
        MoveInput = input.sqrMagnitude > 1f ? input.normalized : input;
    }

    public void SetRotationInput(float input)
    {
        // Rotation is treated as a -1..1 axis.
        RotationInput = Mathf.Clamp(input, -1f, 1f);
    }
}
