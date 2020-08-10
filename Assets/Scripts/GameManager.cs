using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] GameObject ui_GameOverlay;
    [SerializeField] GameObject ui_GameOver;
    [SerializeField] GameObject ui_Instructions;

    [SerializeField] Text gameTimerText;
    [SerializeField] Text totalShroomCount;

    [SerializeField] int gameplayTime = 100;

    WaitForSeconds wait_One = new WaitForSeconds(1f);

    public bool gameStarted = false;

    [Header("Game UI Audio")]
    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip resumeGameClip;
    [SerializeField] AudioClip gameOverClip;
    [SerializeField] AudioClip timeTickingDownClip;


    #region Singleton
    private static GameManager _instance;
    private static bool _isInstanceNull = true;

    public static GameManager Instance
    {
        get
        {
            if (_isInstanceNull)
            {
                return null;
            }
            else
            {
                return _instance;
            }
        }

        set
        {
            _instance = value;
            _isInstanceNull = false;
        }
    }
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Instance = null;
            Destroy(this);
        }
    }
    
    private void Start()
    {
        ui_Instructions.SetActive(true);
        ui_GameOverlay.SetActive(false);
        ui_GameOver.SetActive(false);
        
        StartCoroutine(WaitForInstructionsClose());
    }


    public void ReplayGame()
    {
        SceneLoadManager.Instance.RestartLevel();
    }

    public void QuitGame()
    {
        SceneLoadManager.Instance.LoadMenu();
    }

    private IEnumerator WaitForInstructionsClose()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.X));

        audioSource.PlayOneShot(resumeGameClip);
        
        ui_Instructions.SetActive(false);
        ui_GameOverlay.SetActive(true);

        gameStarted = true;

        MushroomSpawnManager.Instance.StartGame();
        StartCoroutine(GameTimer());
    }

    private IEnumerator GameTimer()
    {
        while(true)
        {
            if (gameplayTime > 0)
            {
                yield return wait_One;
                gameplayTime--;

                if (gameplayTime < 11 && gameplayTime >= 1)
                    audioSource.PlayOneShot(timeTickingDownClip);

                gameTimerText.text = gameplayTime.ToString();

            }
            else
            {
                GameOver();
                yield break;
            }

            yield return null;
        }
    }

    public void GameOver()
    {
        gameStarted = false;

        StopAllCoroutines();
        Time.timeScale = 0;

        SceneLoadManager.Instance.PauseBGM();

        totalShroomCount.text = PlayerManager.Instance.mushroomFollowers.Count.ToString();
        audioSource.PlayOneShot(gameOverClip);

        ui_GameOverlay.SetActive(false);
        ui_GameOver.SetActive(true);
    }

}
