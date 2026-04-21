using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class GrabUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Slider progressBar;

    public void Show(KeyCode key)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (promptText != null)
        {
            promptText.text = "Break Free";

        }
        SetProgress(0f);
    }

    public void SetProgress(float normalizedProgress)
    {
        normalizedProgress = Mathf.Clamp01(normalizedProgress);

        if (progressBar != null)
        {
            progressBar.value = normalizedProgress;
        }
    }

    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}
