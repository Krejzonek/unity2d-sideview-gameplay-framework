using UnityEngine;

public class StatsPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject statsPanelPrefab;

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

            GameObject panel = Instantiate(statsPanelPrefab, transform);

            UIStats uiStats = panel.GetComponent<UIStats>();

            if (uiStats != null)
                uiStats.Setup(character);
        }
    }
}