using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectUI : MonoBehaviour
{
    [SerializeField] private string mapSceneName = "YourMapSceneName";

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SelectMap()
    {
        GameSelection.selectedMapSceneName = mapSceneName;
        StartCoroutine(LoadSceneNextFrame(mapSceneName));
    }

    public void BackToCharacterSelect()
    {
        StartCoroutine(LoadSceneNextFrame("CharacterSelect"));
    }

    IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}