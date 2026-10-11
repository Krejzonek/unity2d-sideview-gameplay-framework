using UnityEngine;

public class EquipmentPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject eqPanelPrefab;

    private void OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        if (CharacterManager.Instance == null)
            return;

        foreach (CharacterData character in CharacterManager.Instance.Characters)
        {
            if (character == null)
                continue;

            GameObject panel = Instantiate(eqPanelPrefab, transform);

            UiEq uiEq = panel.GetComponent<UiEq>();

            if (uiEq != null)
                uiEq.Setup(character);

            EquipmentSlot[] slots = panel.GetComponentsInChildren<EquipmentSlot>(true);

            foreach (EquipmentSlot slot in slots)
            {
                if (slot != null)
                    slot.Setup(character);
            }
        }
    }
}