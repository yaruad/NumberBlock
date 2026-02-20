using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textCurrentScore;
    [SerializeField]
    private TextMeshProUGUI textHighScore;
    [SerializeField]
    private GameObject panelGameOver;

    private void Awake()
    {
        if (panelGameOver.activeSelf == true)
        panelGameOver.SetActive(false);
    }

    public void UpdateCurrentScore(int score)
    {
        textCurrentScore.text = score.ToString();
    }

    public void UpdateHighScore(int score)
    {
        textHighScore.text = score.ToString();
    }

    public void OnClickGoToMain()
    {
        SceneManager.LoadScene("01Main");
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("02Game");
    }

    public void OnGameOver()
    {
        panelGameOver.SetActive(true);
    }
}
