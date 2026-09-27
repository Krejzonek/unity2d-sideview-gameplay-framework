using UnityEngine;

public class CharacterGameLoader : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Stats stats;

    private void Awake()
    {
        if (CharacterManager.Instance == null)
            return;

        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (stats == null)
            stats = FindFirstObjectByType<Stats>();

        if (player == null || stats == null)
            return;

        CharacterManager.Instance.ApplyCharacterToPlayer(player, stats);
    }
}