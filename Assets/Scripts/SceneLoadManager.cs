using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadManager : MonoBehaviour
{
    [SerializeField] Canvas fadeCanvas;
    [SerializeField] Image fadeOverlay;

    [SerializeField] Color fadeToColour;

    Color fadeInColour;

    [Tooltip("Time between fade transition frame steps in seconds")]
    [SerializeField] float timeBetweenFadeSteps = 0.1f;

    [Tooltip("Number of Single Frames the Fade Transition takes")]
    [SerializeField] int frameDuration = 5;

    [SerializeField] float minLoadingTime = 2f;

    private bool loading = false;
    Scene currentScene;

    [Header("BGM")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip titleBGM;
    [SerializeField] AudioClip mainBGM;

    private static SceneLoadManager _instance;
    private static bool _isInstanceNull = true;

    public enum BuildIndex
    {
        Menu = 0,
        MainGame = 1,
        Loading = 2,
    }

    public static SceneLoadManager Instance
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

    }

    private void Start()
    {
        //BGM initialization
        currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex == (int)BuildIndex.Menu)
            audioSource.clip = titleBGM;
        else
            audioSource.clip = mainBGM;

        audioSource.Play();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadMenu();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RestartLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadGame();
        }
    }
#endif

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loading = false;

        fadeInColour = fadeToColour;
        fadeInColour.a = 0f;

        currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex == (int)BuildIndex.Menu)
            audioSource.clip = titleBGM;
        else
            audioSource.clip = mainBGM;

        StartCoroutine(ScreenFade(fadeToColour, fadeInColour));
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneAsync((int)BuildIndex.Menu));
    }

    public void LoadGame()
    {
        StartCoroutine(LoadSceneAsync((int)BuildIndex.MainGame));
    }

    public void RestartLevel()
    {
        StartCoroutine(Reload());
        audioSource.Play();

    }

    private IEnumerator Reload()
    {
        Time.timeScale = 1f;
        //Fade to black
        yield return StartCoroutine(ScreenFade(fadeInColour, fadeToColour));


        //Load the current scene again
        SceneManager.LoadScene((int)BuildIndex.MainGame);
    }

    private IEnumerator LoadSceneAsync(int index)
    {
        loading = true;

        //Fade to black
        yield return StartCoroutine(ScreenFade(fadeInColour, fadeToColour));

        if (Time.timeScale != 1f)
            Time.timeScale = 1f;

        //Load in the loading screen
        yield return SceneManager.LoadSceneAsync((int)BuildIndex.Loading);
      
        //Fade to clear
        yield return StartCoroutine(ScreenFade(fadeToColour, fadeInColour));

        float loadingTime = Time.time + minLoadingTime;

        //Start a new async operation for loading the next scene
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(index);
        asyncOp.allowSceneActivation = false;

        //Wait until loading progress is at 90% (which is where it caps at just before
        //automatically activating the next scene)
        yield return new WaitUntil(() => asyncOp.progress == 0.9f);

        //Check if amount of time passed is less than 
        //the min duration we want for loading
        while (Time.time < loadingTime)
            yield return null;
        
        //Fade out the loading scene to black
        StartCoroutine(ScreenFade(Color.clear, fadeToColour));

        StartCoroutine(AudioFadeOut());

        yield return new WaitUntil(() => fadeOverlay.color == fadeToColour);

        StartCoroutine(AudioFadeIn());
        //Activate the next scene
        asyncOp.allowSceneActivation = true;
    }

    private IEnumerator AudioFadeOut()
    {
        while (audioSource.volume > 0.8f)
        {
            audioSource.volume -= 0.1f;

            if (audioSource.volume <= 0.8f)
            {
                audioSource.volume = 0.8f;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator AudioFadeIn()
    {
        while (audioSource.volume < 1)
        {
            audioSource.volume += 0.1f;

            if (audioSource.volume >= 1)
            {
                audioSource.volume = 1;
                audioSource.Play();
                yield break;
            }

            yield return null;
        }
    }

    public void PauseBGM()
    {
        audioSource.Pause();
    }

    #region Screen Fade Coroutines

    private IEnumerator ScreenFade(Color startColour, Color endColour)
    {
        //turn on the canvas if it's off
        if (!fadeCanvas.enabled)
            fadeCanvas.enabled = true;

        fadeOverlay.color = startColour;

        float progress = 0f;

        while (progress < frameDuration)
        {
            fadeOverlay.color = Color.Lerp(startColour, endColour, progress / frameDuration);
            progress++;

            yield return new WaitForSeconds(timeBetweenFadeSteps);
        }

        fadeOverlay.color = endColour;

        //turn off the canvas if we're fading to clear
        if (fadeOverlay.color == Color.clear)
            fadeCanvas.enabled = false;
    }

    #endregion
}
