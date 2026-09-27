using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private string gameSceneName = "CharacterSelect";

    public void Play()
    {
        sceneLoader.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}