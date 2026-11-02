using UnityEngine;

public class MobileControls : MonoBehaviour
{
    [SerializeField] private GameInputSettings settings;
    [SerializeField] private GameObject mobileControlsRoot;
    [SerializeField] private VirtualJoystick movementJoystick;
    [SerializeField] private VirtualJoystick aimJoystick;
    [SerializeField] private MobileActionButton[] actionButtons;
    [SerializeField] private RectTransform movementJoystickRect;
    [SerializeField] private RectTransform aimJoystickRect;
    [SerializeField] private RectTransform[] actionButtonRects;

    private void Start()
    {
        GameSettings.Load();
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (settings == null)
        {
            Debug.LogWarning("MobileControls: GameInputSettings is not assigned.", this);
            return;
        }

        bool showJoysticks = settings.enableVirtualJoystick;
        bool showActionButtons = settings.enableActionButtons;

        if (mobileControlsRoot != null)
            mobileControlsRoot.SetActive(showJoysticks || showActionButtons);

        if (movementJoystick != null)
            movementJoystick.gameObject.SetActive(showJoysticks);

        if (aimJoystick != null)
            aimJoystick.gameObject.SetActive(showJoysticks);

        if (actionButtons != null)
        {
            foreach (MobileActionButton button in actionButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(showActionButtons);
            }
        }

        ApplyRect(movementJoystickRect, GameSettings.MovementPosition, GameSettings.MovementScale);
        ApplyRect(aimJoystickRect, GameSettings.AimPosition, GameSettings.AimScale);

        if (actionButtonRects != null)
        {
            for (int i = 0; i < actionButtonRects.Length; i++)
            {
                RectTransform rect = actionButtonRects[i];

                if (rect == null)
                    continue;

                Vector2 position = GameSettings.GetActionButtonPosition(i, rect.anchoredPosition);
                ApplyRect(rect, position, 1f);
            }
        }
    }

    private void ApplyRect(RectTransform rect, Vector2 position, float scale)
    {
        if (rect == null)
            return;

        rect.anchoredPosition = position;
        rect.localScale = Vector3.one * scale;
    }
}