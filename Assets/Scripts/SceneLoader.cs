using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private GameObject bgLoadingPrefab;
    [SerializeField] private float minDisplayTime = 1.2f;

    private bool _loading = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (_loading) return;
        StartCoroutine(DoLoad(sceneName));
    }

    IEnumerator DoLoad(string sceneName)
    {
        _loading = true;

        GameObject bg = Instantiate(bgLoadingPrefab);
        yield return null;
        DontDestroyOnLoad(bg);

        Camera bgCam = bg.GetComponentInChildren<Camera>();
        if (bgCam != null) bgCam.depth = 1000;

        AudioListener bgAudio = bg.GetComponentInChildren<AudioListener>();
        if (bgAudio != null) bgAudio.enabled = false;

        yield return null;

        float startTime = Time.unscaledTime;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f || Time.unscaledTime - startTime < minDisplayTime)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone) yield return null;

        Destroy(bg);
        _loading = false;
    }
}
