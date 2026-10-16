
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private string gameSceneName = "CharacterSelect";

    private void Start()
    {
        GameSettings.Load();

        if (settingsMenu != null)
            settingsMenu.SetActive(false);
    }

    public void Play()
    {
        sceneLoader.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
