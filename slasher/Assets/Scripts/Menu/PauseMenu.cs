using System.IO;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";

    private bool isPaused;
    private bool isChangingScene;

    private void Start()
    {
        pauseMenu.SetActive(false);
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
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void SaveBeforeSceneChange()
    {
        if (TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
            TeamManager.Instance.SaveTeam();

        if (MapSaveSystem.Instance == null)
        {
            Debug.LogError("SAVE TEST: MapSaveSystem.Instance is NULL!");
            return;
        }

        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
            MapSaveSystem.Instance.SaveMap(generator);
        else
        {
            Debug.LogWarning("SAVE TEST: RoomGenerator not found. Saving character data only.");
            MapSaveSystem.Instance.SaveCharactersNow();
        }

        string path = Path.Combine(Application.persistentDataPath, "mapSave.json");

        if (!File.Exists(path))
        {
            Debug.LogError("SAVE TEST: mapSave.json DOES NOT EXIST!");
            return;
        }

        try
        {
            MapSaveSystem.MapData data = JsonUtility.FromJson<MapSaveSystem.MapData>(File.ReadAllText(path));

            int count = data != null && data.characters != null ? data.characters.Count : -1;
            Debug.Log("SAVE TEST: file=" + path + ", characters=" + count);

            if (data != null && data.characters != null)
            {
                foreach (MapSaveSystem.CharacterSaveData character in data.characters)
                    Debug.Log("SAVE TEST: " + (character != null ? character.characterName : "NULL"));
            }
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

        if (CharacterManager.Instance != null && TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
        {
            CharacterManager.Instance.SaveCharacterSwitchPosition(TeamManager.Instance.CurrentPlayer.transform.position);
            CharacterManager.Instance.StartCharacterSwitch();
        }

        SaveBeforeSceneChange();
        sceneLoader.LoadScene(characterSelectSceneName);
    }
}