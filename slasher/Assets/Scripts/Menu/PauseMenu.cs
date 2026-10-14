using System.IO;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private FightPreparationTimer preparationTimer;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";

    private bool isPaused;
    private bool isChangingScene;

    private void Start()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (preparationTimer == null)
            preparationTimer = FindFirstObjectByType<FightPreparationTimer>();

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (isPaused || isChangingScene)
            return;

        isPaused = true;

        if (preparationTimer != null)
            preparationTimer.CancelPreparation();

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Resume()
    {

        if (!isPaused || isChangingScene)
            return;

        isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;

        if (preparationTimer != null)
            preparationTimer.CheckAndStart();
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
        Time.timeScale = 1f;

        SaveBeforeSceneChange();
        sceneLoader.LoadScene(mainMenuSceneName);
    }

    public void ChangeCharacter()
    {
        if (isChangingScene)
            return;

        isChangingScene = true;
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