using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private Button pauseButtonUI;
    [SerializeField] private Button settingsButton;
    [SerializeField] private FightPreparationTimer preparationTimer;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";
    [SerializeField] private GameObject selectedBorder;

    private bool isPaused;
    private bool isChangingScene;

    private void Start()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (preparationTimer == null)
            preparationTimer = FindFirstObjectByType<FightPreparationTimer>();

        if (pauseButtonUI != null)
            pauseButtonUI.onClick.AddListener(Pause);

        if (GameInputManager.Instance != null)
            GameInputManager.Instance.PauseStarted += TogglePause;

        SetSelectedBorder(false);

        GameSettings.Load();
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (pauseButtonUI != null)
            pauseButtonUI.onClick.RemoveListener(Pause);

        if (GameInputManager.Instance != null)
            GameInputManager.Instance.PauseStarted -= TogglePause;
    }

    private void TogglePause()
    {
        if (isChangingScene)
            return;

        if (settingsMenu != null && settingsMenu.activeSelf)
            CloseSettings();
        else if (isPaused)
            Resume();
        else
            Pause();
    }

    private void SetSelectedBorder(bool active)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(active);
    }

    private void SetPlayerInputBlocked(bool blocked)
    {
        if (TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
            TeamManager.Instance.CurrentPlayer.SetInputBlocked(blocked);
    }

    private void SelectButton(Button button)
    {
        if (button == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void Pause()
    {
        if (isPaused || isChangingScene)
            return;

        isPaused = true;
        SetPlayerInputBlocked(true);

        if (preparationTimer != null)
            preparationTimer.CancelPreparation();

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        if (pauseButtonUI != null)
            pauseButtonUI.gameObject.SetActive(false);

        SetSelectedBorder(true);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!isPaused || isChangingScene)
            return;

        isPaused = false;
        SetPlayerInputBlocked(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (pauseButtonUI != null)
            pauseButtonUI.gameObject.SetActive(true);

        SetSelectedBorder(false);

        Time.timeScale = 1f;

        if (preparationTimer != null)
            preparationTimer.CheckAndStart();
    }

    public void OpenSettings()
    {
        if (!isPaused || isChangingScene)
            return;

        SetPlayerInputBlocked(true);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(true);

        SetSelectedBorder(true);
    }

    public void CloseSettings()
    {
        if (!isPaused || isChangingScene)
            return;

        SetPlayerInputBlocked(true);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        SelectButton(settingsButton);
        SetSelectedBorder(true);
    }

    private void SaveBeforeSceneChange()
    {
        if (TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
            TeamManager.Instance.SaveTeam();

        if (MapSaveSystem.Instance == null)
            return;

        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
            MapSaveSystem.Instance.SaveMap(generator);
        else
            MapSaveSystem.Instance.SaveCharactersNow();

        string path = Path.Combine(Application.persistentDataPath, "mapSave.json");

        if (!File.Exists(path))
            return;

        try
        {
            MapSaveSystem.MapData data = JsonUtility.FromJson<MapSaveSystem.MapData>(File.ReadAllText(path));
            int count = data != null && data.characters != null ? data.characters.Count : -1;
        }
        catch (System.Exception exception)
        {
            Debug.LogError("SAVE TEST: Failed to read save file: " + exception.Message);
        }
    }

    public void MainMenu()
    {
        if (isChangingScene)
            return;

        isChangingScene = true;
        SetPlayerInputBlocked(true);
        SetSelectedBorder(false);
        Time.timeScale = 1f;

        SaveBeforeSceneChange();
        sceneLoader.LoadScene(mainMenuSceneName);
    }

    public void ChangeCharacter()
    {
        if (isChangingScene)
            return;

        isChangingScene = true;
        SetPlayerInputBlocked(true);
        SetSelectedBorder(false);
        Time.timeScale = 1f;

        if (preparationTimer != null)
            preparationTimer.CancelPreparation();

        if (CharacterManager.Instance != null && TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
        {
            CharacterManager.Instance.SaveCharacterSwitchPosition(TeamManager.Instance.CurrentPlayer.transform.position);
            CharacterManager.Instance.StartCharacterSwitch();
        }

        SaveBeforeSceneChange();
        sceneLoader.LoadScene(characterSelectSceneName);
    }
}