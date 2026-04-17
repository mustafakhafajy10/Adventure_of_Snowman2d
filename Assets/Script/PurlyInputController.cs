using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PurlyMovementModel))]
public class PurlyInputController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string rotateActionName = "Rotate";

    private PurlyMovementModel movementModel;
    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction rotateAction;

    private void Awake()
    {
        movementModel = GetComponent<PurlyMovementModel>();

        if (inputActions != null)
        {
            actionMap = inputActions.FindActionMap(actionMapName, true);
            moveAction = actionMap.FindAction(moveActionName, true);
            rotateAction = actionMap.FindAction(rotateActionName, true);
        }
        else
        {
            CreateFallbackActions();
        }
    }

    private void OnEnable()
    {
        if (moveAction == null || rotateAction == null)
        {
            return;
        }

        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        rotateAction.performed += OnRotatePerformed;
        rotateAction.canceled += OnRotateCanceled;
        actionMap.Enable();
    }

    private void OnDisable()
    {
        if (moveAction == null || rotateAction == null)
        {
            return;
        }

        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        rotateAction.performed -= OnRotatePerformed;
        rotateAction.canceled -= OnRotateCanceled;
        actionMap.Disable();
        movementModel.SetMoveInput(Vector2.zero);
        movementModel.SetRotationInput(0f);
    }

    private void CreateFallbackActions()
    {
        actionMap = new InputActionMap(actionMapName);

        moveAction = actionMap.AddAction(moveActionName, InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        rotateAction = actionMap.AddAction(rotateActionName, InputActionType.Value);
        rotateAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/e")
            .With("Positive", "<Keyboard>/q");
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        movementModel.SetMoveInput(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        movementModel.SetMoveInput(Vector2.zero);
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        movementModel.SetRotationInput(context.ReadValue<float>());
    }

    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        movementModel.SetRotationInput(0f);
    }
}
