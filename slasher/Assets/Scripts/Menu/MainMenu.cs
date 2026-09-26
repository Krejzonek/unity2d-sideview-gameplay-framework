using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private string gameSceneName = "Game";

    public void Play()
    {
        sceneLoader.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}