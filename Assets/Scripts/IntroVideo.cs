using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

// Intro de marca Drixwave (2026-10-01): video a pantalla completa al abrir el
// juego, una sola vez por arranque, encima del menú. Reemplaza al logo de
// Unity (apagado en Player Settings; Unity 6 Personal lo permite).
// - El video vive en StreamingAssets y se reproduce por URL: es el único modo
//   que WebGL soporta, y así no engorda la descarga inicial de la web.
// - Se crea solo (RuntimeInitializeOnLoadMethod): no requiere objetos en la escena.
// - Se salta con un clic/toque/tecla. Si el video falla o tarda en cargar, se
//   cierra solo y el menú queda usable (nunca bloquea el juego).
public class IntroVideo : MonoBehaviour
{
    const string VideoFile = "Intro/drixwave_intro.mp4";
    const string MenuScene = "Menu";        // solo al arrancar desde el menú (no al probar Game suelto)
    const float PrepareTimeout = 6f;        // segundos máx. esperando que cargue (web lenta)
    const float SkipGrace = 0.3f;           // ignora el clic que pudo abrir el juego
    const int SortingOrder = 32000;         // por encima de cualquier canvas del menú
    const int RenderWidth = 1280, RenderHeight = 720; // resolución nativa del video (16:9)

    VideoPlayer player;
    RenderTexture target;
    RawImage screenImage;
    AspectRatioFitter fitter;
    float startTime;
    bool finished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void PlayOnStartup()
    {
        if (SceneManager.GetActiveScene().name != MenuScene) return;
        new GameObject("IntroVideo").AddComponent<IntroVideo>();
    }

    void Awake()
    {
        startTime = Time.unscaledTime;
        BuildOverlay();
        BuildPlayer();
        // pausa la música del menú; el audio del video ignora esta pausa
        AudioListener.pause = true;
        // Play directo (prepara solo): en WebGL, Prepare() sin Play() nunca baja el video
        player.Play();
    }

    // canvas negro a pantalla completa (tapa el menú y bloquea sus clics) + imagen del video
    void BuildOverlay()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;
        gameObject.AddComponent<GraphicRaycaster>();

        var background = new GameObject("Background", typeof(Image));
        background.transform.SetParent(transform, false);
        Stretch(background.GetComponent<RectTransform>());
        background.GetComponent<Image>().color = Color.black;

        target = new RenderTexture(RenderWidth, RenderHeight, 0);
        var screen = new GameObject("Video", typeof(RawImage), typeof(AspectRatioFitter));
        screen.transform.SetParent(transform, false);
        Stretch(screen.GetComponent<RectTransform>());
        screenImage = screen.GetComponent<RawImage>();
        screenImage.texture = target;
        screenImage.raycastTarget = false;
        screenImage.enabled = false; // se muestra recién con el primer frame (evita un cuadro gris)
        fitter = screen.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = (float)RenderWidth / RenderHeight;
        ApplyFitMode();
    }

    // pantalla más ancha que 16:9 (celulares 20:9): llena y recorta un poco arriba/abajo
    // (el logo está en la franja central). Más angosta (tablets 4:3): bandas negras,
    // porque recortar los costados cortaría el texto "DRIXWAVE".
    void ApplyFitMode()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        fitter.aspectMode = screenAspect >= fitter.aspectRatio
            ? AspectRatioFitter.AspectMode.EnvelopeParent
            : AspectRatioFitter.AspectMode.FitInParent;
    }

    void BuildPlayer()
    {
        player = gameObject.AddComponent<VideoPlayer>();
        player.playOnAwake = false;
        player.isLooping = false;
        player.source = VideoSource.Url;
        player.url = System.IO.Path.Combine(Application.streamingAssetsPath, VideoFile);
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = target;
        player.skipOnDrop = true;
        ConfigureAudio();

        player.loopPointReached += _ => Finish();
        player.errorReceived += (_, message) =>
        {
            Debug.LogWarning($"[IntroVideo] No se pudo reproducir la intro: {message}");
            Finish();
        };
    }

    void ConfigureAudio()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // los navegadores bloquean el autoplay con sonido antes de un clic:
        // en web la intro va muda para que arranque siempre
        player.audioOutputMode = VideoAudioOutputMode.None;
#else
        // por AudioSource: respeta el volumen general (AudioListener.volume)
        var audioOut = gameObject.AddComponent<AudioSource>();
        audioOut.playOnAwake = false;
        audioOut.ignoreListenerPause = true;
        player.audioOutputMode = VideoAudioOutputMode.AudioSource;
        player.SetTargetAudioSource(0, audioOut);
#endif
    }

    void Update()
    {
        if (finished) return;
        ApplyFitMode(); // la ventana (PC/web) puede cambiar de tamaño

        bool started = player.frame > 0; // ya hay imagen en la textura
        if (started) screenImage.enabled = true;

        float elapsed = Time.unscaledTime - startTime;
        if (!started && elapsed > PrepareTimeout)
        {
            Debug.LogWarning("[IntroVideo] La intro tardó demasiado en cargar; se salta.");
            Finish();
        }
        else if (elapsed > SkipGrace && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
        {
            Finish(); // salteo manual (el toque en Android llega como clic)
        }
    }

    // cierra la intro: devuelve la música del menú y libera el video
    void Finish()
    {
        if (finished) return;
        finished = true;
        AudioListener.pause = false;
        player.Stop();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        AudioListener.pause = false; // por si la escena se descarga a mitad de la intro
        if (target != null) { target.Release(); Destroy(target); }
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
