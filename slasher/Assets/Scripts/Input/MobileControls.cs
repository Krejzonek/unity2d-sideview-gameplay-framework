using UnityEngine;
using UnityEngine.InputSystem;

public class MobileControls : MonoBehaviour
{
    [SerializeField] private GameInputSettings settings;
    [SerializeField] private GameObject mobileControlsRoot;
    [SerializeField] private VirtualJoystick movementJoystick;
    [SerializeField] private VirtualJoystick aimJoystick;
    [SerializeField] private MobileActionButton[] actionButtons;
    [SerializeField] private RectTransform movementJoystickRect;
    [SerializeField] private RectTransform aimJoystickRect;
    [SerializeField] private RectTransform buttonsRect;

    private void Start()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (settings == null)
        {
            Debug.LogWarning("MobileControls: GameInputSettings is not assigned.", this);
            return;
        }

        bool showControls = settings.mobileControlsMode switch
        {
            MobileControlsMode.Always => true,
            MobileControlsMode.Never => false,
            _ => Application.isMobilePlatform || Touchscreen.current != null
        };

        if (mobileControlsRoot != null)
            mobileControlsRoot.SetActive(showControls);

        if (movementJoystick != null)
            movementJoystick.gameObject.SetActive(showControls && settings.enableVirtualJoystick);

        if (aimJoystick != null)
            aimJoystick.gameObject.SetActive(showControls && settings.enableVirtualJoystick);

        if (actionButtons != null)
        {
            foreach (MobileActionButton button in actionButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(showControls && settings.enableActionButtons);
            }
        }

        if (movementJoystickRect != null)
        {
            Vector2 anchor = settings.joystickSide == JoystickSide.Left ? new Vector2(0f, 0f) : new Vector2(1f, 0f);
            movementJoystickRect.anchorMin = anchor;
            movementJoystickRect.anchorMax = anchor;
            movementJoystickRect.localScale = Vector3.one * settings.joystickScale;
        }

        if (aimJoystickRect != null)
        {
            aimJoystickRect.anchorMin = new Vector2(1f, 0f);
            aimJoystickRect.anchorMax = new Vector2(1f, 0f);
            aimJoystickRect.localScale = Vector3.one * settings.joystickScale;
        }
    }
}