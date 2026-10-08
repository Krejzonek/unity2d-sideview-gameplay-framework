using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";

    private bool isPaused;

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

    public void MainMenu()
    {
        Time.timeScale = 1f;
        sceneLoader.LoadScene(mainMenuSceneName);
    }

    public void ChangeCharacter()
    {
        Time.timeScale = 1f;

        if (CharacterManager.Instance != null && TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
        {
            CharacterManager.Instance.StartCharacterSwitch();
            CharacterManager.Instance.SaveCharacterSwitchPosition(TeamManager.Instance.CurrentPlayer.transform.position);
        }

        sceneLoader.LoadScene(characterSelectSceneName);
    }
}