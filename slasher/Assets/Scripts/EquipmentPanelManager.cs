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

        Debug.Log("Characters count: " + CharacterManager.Instance.Characters.Count);

        foreach (CharacterData character in CharacterManager.Instance.Characters)
        {
            if (character == null)
                continue;

            Debug.Log("Creating panel for: " + character.characterName);

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