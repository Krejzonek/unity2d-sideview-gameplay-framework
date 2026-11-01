using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NameInputGamepadNavigation : MonoBehaviour, IMoveHandler
{
    [SerializeField] private Selectable downTarget;
    [SerializeField] private Selectable upTarget;

    public void OnMove(AxisEventData eventData)
    {
        Debug.Log("[NameNavigation] Move: " + eventData.moveDir);

        if (eventData.moveDir == MoveDirection.Down && downTarget != null)
        {
            Debug.Log("[NameNavigation] Moving DOWN to: " + downTarget.name);
            eventData.Use();
            EventSystem.current.SetSelectedGameObject(downTarget.gameObject);
        }
        else if (eventData.moveDir == MoveDirection.Up && upTarget != null)
        {
            Debug.Log("[NameNavigation] Moving UP to: " + upTarget.name);
            eventData.Use();
            EventSystem.current.SetSelectedGameObject(upTarget.gameObject);
        }
        else
        {
            Debug.LogWarning("[NameNavigation] No target assigned for: " + eventData.moveDir);
        }
    }
}