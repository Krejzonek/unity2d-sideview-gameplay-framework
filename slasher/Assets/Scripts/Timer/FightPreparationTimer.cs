using System.Collections;
using TMPro;
using UnityEngine;

public class FightPreparationTimer : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float preparationTime = 3f;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerPanel;

    private Coroutine timerCoroutine;
    private bool preparing;

    private IEnumerator Start()
    {

        if (titleText != null)
            titleText.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (timerPanel != null)
            timerPanel.SetActive(false);

        yield return null;
        yield return null;

        CheckAndStart();
    }

    public void CheckAndStart()
    {
        if (preparing || timerCoroutine != null)
        {
            return;
        }

        bool hasEnemies = HasEnemiesInCurrentRoom();

        if (!hasEnemies)
        {
            return;
        }

        timerCoroutine = StartCoroutine(PreparationCountdown());
    }

    public void CancelPreparation()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = null;
        preparing = false;

        if (titleText != null)
            titleText.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (timerPanel != null)
            timerPanel.SetActive(false);
    }

    private IEnumerator PreparationCountdown()
    {

        preparing = true;
        float remainingTime = preparationTime;
        Time.timeScale = 0f;

        if (timerPanel != null)
            timerPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.text = "Get Ready!";
            titleText.gameObject.SetActive(true);
        }

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(remainingTime).ToString();
            timerText.gameObject.SetActive(true);
        }

        while (remainingTime > 0f)
        {
            remainingTime -= Time.unscaledDeltaTime;

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(Mathf.Max(0f, remainingTime)).ToString();

            yield return null;
        }

        if (timerText != null)
        {
            timerText.text = "FIGHT!";
            yield return new WaitForSecondsRealtime(0.5f);
        }

        if (titleText != null)
            titleText.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (timerPanel != null)
            timerPanel.SetActive(false);

        preparing = false;
        timerCoroutine = null;
        Time.timeScale = 1f;
    }

    private bool HasEnemiesInCurrentRoom()
    {
        Player player = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;

        if (player == null)
        {
            return false;
        }

        Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);

        foreach (Room room in rooms)
        {
            if (room == null)
                continue;

            Collider2D bounds = room.GetMapBounds();

            if (bounds == null)
            {
                continue;
            }

            bool playerInside = bounds.OverlapPoint(player.transform.position);
            bool cleared = room.IsCleared();

            if (playerInside)
            {
                return !cleared;
            }
        }
        return false;
    }
}