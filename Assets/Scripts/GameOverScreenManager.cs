using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class GameOverScreenManager : MonoBehaviour
{
    // ボタンが押されたら実行される関数
    public void OnStartButtonPressed()
    {
      
        SceneManager.LoadScene("StartScene");
    }
}