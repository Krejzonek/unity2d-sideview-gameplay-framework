using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject playerPlate;
    [SerializeField] private GameObject enemyPlate;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text enemyName;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueIndicator;

    [Header("Settings")]
    [SerializeField] private string defaultPlayerName = "Player";
    [SerializeField] private string defaultEnemyName = "Enemy";

    private DialogueLine currentDialogue;
    private Action onDialogueComplete;
    private int currentEntry;
    private float previousTimeScale = 1f;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        bool advance =
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
            Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame ||
            Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (!advance)
        {
            foreach (Gamepad gamepad in Gamepad.all)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    advance = true;
                    break;
                }
            }
        }

        if (advance)
            ShowNextEntry();
    }

    public bool PlayDialogue(DialogueLine dialogue, Action onComplete = null)
    {
        if (isPlaying || dialogue == null || dialogue.entries == null || dialogue.entries.Count == 0)
            return false;

        currentDialogue = dialogue;
        onDialogueComplete = onComplete;
        currentEntry = 0;
        isPlaying = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (playerName != null)
            playerName.text = GetFirstPlayerName();

        if (enemyName != null)
            enemyName.text = GetDialogueEnemyName(dialogue);

        ShowNextEntry();

        return true;
    }

    private void ShowNextEntry()
    {
        if (currentDialogue == null)
        {
            FinishDialogue();
            return;
        }

        while (currentEntry < currentDialogue.entries.Count)
        {
            DialogueEntry entry = currentDialogue.entries[currentEntry++];

            if (entry == null || string.IsNullOrWhiteSpace(entry.text))
                continue;

            bool isPlayer = entry.speaker == DialogueSpeaker.Player;

            if (playerPlate != null)
                playerPlate.SetActive(isPlayer);

            if (enemyPlate != null)
                enemyPlate.SetActive(!isPlayer);

            if (dialogueText != null)
                dialogueText.text = entry.text;

            if (continueIndicator != null)
                continueIndicator.SetActive(true);

            return;
        }

        FinishDialogue();
    }

    private string GetFirstPlayerName()
    {
        if (TeamManager.Instance == null || TeamManager.Instance.Team == null)
            return defaultPlayerName;

        foreach (Player player in TeamManager.Instance.Team)
        {
            if (player != null)
                return GetCleanName(player.gameObject.name);
        }

        return defaultPlayerName;
    }

    private string GetDialogueEnemyName(DialogueLine dialogue)
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null && enemy.GetDialogueLine() == dialogue)
                return GetCleanName(enemy.gameObject.name);
        }

        return defaultEnemyName;
    }

    private string GetCleanName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return string.Empty;

        return objectName.Replace("(Clone)", "").Trim();
    }

    private void FinishDialogue()
    {
        isPlaying = false;
        currentDialogue = null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        Time.timeScale = previousTimeScale;

        Action callback = onDialogueComplete;
        onDialogueComplete = null;

        callback?.Invoke();
    }

    public void SkipDialogue()
    {
        if (isPlaying)
            FinishDialogue();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        if (isPlaying)
            Time.timeScale = previousTimeScale;

        Instance = null;
    }
}