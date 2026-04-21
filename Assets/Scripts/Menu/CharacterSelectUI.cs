using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterSelectData
{
    public Sprite characterImage;
    public Sprite[] starterLoadoutImages;
}

public class CharacterSelectUI : MonoBehaviour
{
    public CharacterSelectData[] characters;
    public int currentCharacter;

    public Image characterImageDisplay;
    public Image[] starterLoadoutSlots;

    private void Start()
    {
        UpdateUI();


        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    


    public void NextCharacter()
    {
        currentCharacter++;

        if (currentCharacter >= characters.Length)
            currentCharacter = 0;

        UpdateUI();
    }

    public void PreviousCharacter()
    {
        currentCharacter--;

        if (currentCharacter < 0)
            currentCharacter = characters.Length - 1;

        UpdateUI();
    }

    public void ConfirmCharacter()
    {
        GameSelection.selectedCharacterIndex = currentCharacter;
        StartCoroutine(LoadSceneNextFrame("MapSelect"));
    }

    public void BackToMainMenu()
    {
        StartCoroutine(LoadSceneNextFrame("MainMenu"));
    }

    IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }

    void UpdateUI()
    {
        if (characters == null || characters.Length == 0)
            return;

        CharacterSelectData character = characters[currentCharacter];

        if (characterImageDisplay != null)
            characterImageDisplay.sprite = character.characterImage;

        for (int i = 0; i < starterLoadoutSlots.Length; i++)
        {
            if (i < character.starterLoadoutImages.Length)
            {
                starterLoadoutSlots[i].sprite = character.starterLoadoutImages[i];
                starterLoadoutSlots[i].enabled = true;
            }
            else
            {
                starterLoadoutSlots[i].enabled = false;
            }
        }
    }
}