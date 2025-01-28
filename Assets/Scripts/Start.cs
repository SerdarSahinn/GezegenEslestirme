using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StartGame();
            // Start ekran�n� kapat
            transform.parent.gameObject.SetActive(false);
        }
    }
}