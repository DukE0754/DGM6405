using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Helper component to handle selection cleanup on pointer exit for mouse/touch.
/// Prevents buttons from remaining selected when clicking/touching and dragging off.
/// </summary>
[RequireComponent(typeof(Selectable))]
public class SelectionPointerHandler : MonoBehaviour, IPointerExitHandler
{
    private Selectable _selectable;

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // If we are currently the selected object
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            // Only deselect if we are using mouse or touch (or no scheme detected)
            if (IsMouseOrTouch())
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private bool IsMouseOrTouch()
    {
        string scheme = null;

        // Use the same logic as MenuBase for consistency
        var playerInput = Object.FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            scheme = playerInput.currentControlScheme;
        }

        if (string.IsNullOrEmpty(scheme))
        {
            foreach (var user in UnityEngine.InputSystem.Users.InputUser.all)
            {
                if (user.controlScheme.HasValue)
                {
                    scheme = user.controlScheme.Value.name;
                    break;
                }
            }
        }

        // If we still don't know, we might be in KBM mode if there's any Mouse device active
        if (string.IsNullOrEmpty(scheme))
        {
            if (Mouse.current != null && Mouse.current.wasUpdatedThisFrame) return true;
            if (Touchscreen.current != null && Touchscreen.current.wasUpdatedThisFrame) return true;
            return false;
        }

        return scheme.Equals("KeyboardMouse", System.StringComparison.OrdinalIgnoreCase) ||
               scheme.Equals("Touch", System.StringComparison.OrdinalIgnoreCase);
    }
}
