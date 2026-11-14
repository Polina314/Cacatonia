using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Экран чтения записки: тёмная панель, заголовок, текст и (по желанию) картинка.
// Вешается на панель внутри Canvas. Сама панель в начале игры прячется.
// Закрыть записку — E, Esc или правая кнопка мыши.
public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance { get; private set; }

    public GameObject panel;          // сама панель (обычно этот же объект)
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Image picture;             // для картинок (план эвакуации, рисунок). Необязательно

    [Header("Где текст, когда в записке есть картинка (чтобы картинка его не закрывала)")]
    public Vector2 bodyPosWithImage = new Vector2(0f, 300f);
    public Vector2 bodySizeWithImage = new Vector2(1300f, 110f);
    [Tooltip("Где и какого размера картинка")]
    public Vector2 picturePos = new Vector2(0f, -70f);
    public Vector2 pictureSize = new Vector2(900f, 560f);

    Vector2 bodyPos, bodySize;
    bool remembered;

    public bool IsOpen { get; private set; }
    // Кадр, в котором записку закрыли: чтобы та же нажатая E не открыла её снова
    public static int ClosedFrame = -1;

    FirstPersonController player;
    float openedAt;

    void Awake()
    {
        Instance = this;
        if (panel == null) panel = gameObject;
        panel.SetActive(false);
    }

    void Start()
    {
        player = FindObjectOfType<FirstPersonController>();
    }

    void Update()
    {
        if (!IsOpen) return;
        // Небольшая пауза, чтобы та же нажатая E не закрыла записку сразу
        if (Time.unscaledTime - openedAt < 0.2f) return;
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            Close();
    }

    public void Show(string title, string body, Sprite image)
    {
        // Запоминаем обычное место текста (как ты его поставила в редакторе)
        if (!remembered && bodyText != null)
        {
            bodyPos = bodyText.rectTransform.anchoredPosition;
            bodySize = bodyText.rectTransform.sizeDelta;
            remembered = true;
        }
        if (bodyText != null)
        {
            bodyText.rectTransform.anchoredPosition = image != null ? bodyPosWithImage : bodyPos;
            bodyText.rectTransform.sizeDelta = image != null ? bodySizeWithImage : bodySize;
        }
        if (picture != null && image != null)
        {
            picture.rectTransform.anchoredPosition = picturePos;
            picture.rectTransform.sizeDelta = pictureSize;
        }
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
        if (picture != null)
        {
            picture.gameObject.SetActive(image != null);
            picture.sprite = image;
            picture.preserveAspect = true;
        }
        panel.SetActive(true);
        IsOpen = true;
        openedAt = Time.unscaledTime;
        if (player != null) player.canMove = false;
    }

    public void Close()
    {
        panel.SetActive(false);
        IsOpen = false;
        ClosedFrame = Time.frameCount;
        if (player != null) player.canMove = true;
    }
}
