using UnityEngine;

[CreateAssetMenu(fileName = "GameInputSettings", menuName = "Game Low Code Kit/Input Settings")]
public class GameInputSettings : ScriptableObject
{
    [Header("Mobile Controls")]
    public MobileControlsMode mobileControlsMode = MobileControlsMode.Auto;
    public bool enableVirtualJoystick = true;
    public bool enableActionButtons = true;
    public JoystickSide joystickSide = JoystickSide.Left;
    [Range(0.5f, 2f)] public float joystickScale = 1f;

    [Header("Canvas")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    [Range(0f, 1f)] public float matchWidthOrHeight = 0.5f;

}