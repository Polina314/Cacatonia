using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

// Список палат в левом верхнем углу экрана:
//   □ Палата 201
//   ✓ Палата 203   ← галочка ставится сама, когда палату проверили
//     313          ← приписка «карандашом», без квадратика
// Вешается на пустой объект ВНУТРИ Canvas (например, Checklist).
// Строки, квадратики и галочки скрипт создаёт сам при запуске игры.
//
// Из событий:
//   RoomChecklist → Show ()        — показать список (например, на 2 этаже)
//   RoomChecklist → Check (int)    — отметить палату: 1 = первая строка, 2 = вторая и т.д.
public class RoomChecklist : MonoBehaviour
{
    [Serializable]
    public class Room
    {
        public string name = "Палата 201";
        [Tooltip("Пустой объект на тумбочке этой палаты: сюда ляжет записка, если палата окажется последней")]
        public Transform notePoint;
        [HideInInspector] public bool done;
    }

    [Serializable]
    public class CountEvent
    {
        [Tooltip("После скольких проверенных палат сработает")]
        public int count = 3;
        public UnityEvent onReached;
    }

    [Header("Палаты")]
    public Room[] rooms = new Room[5];
    [Tooltip("Приписка внизу без квадратика (как будто дописали карандашом)")]
    public string extraLine = "313";

    [Header("Картинки квадратика и галочки")]
    public Sprite boxSprite;
    public Sprite checkSprite;

    [Header("Внешний вид")]
    public float fontSize = 26f;
    public float rowHeight = 38f;
    public Color textColor = Color.white;
    public Color doneColor = new Color(0.6f, 0.6f, 0.6f);
    public Color pencilColor = new Color(0.65f, 0.65f, 0.7f);
    public bool showOnStart = false;

    [Header("Сообщение при проверке ({0} — название палаты)")]
    public string checkedMessage = "{0} — проверена";

    [Header("События")]
    [Tooltip("Например: после 3 палат гаснет свет")]
    public CountEvent[] countEvents;
    [Tooltip("Когда проверены ВСЕ палаты")]
    public UnityEvent onAllChecked;
    [Tooltip("Записка, которая появится на тумбочке последней проверенной палаты (в начале галочка у неё снята)")]
    public GameObject finalItem;

    [Header("Записка — только после события (например, после плана эвакуации)")]
    [Tooltip("Галочка: записка появится, только когда проверены все палаты И вызван Unlock () (например, из плана эвакуации)")]
    public bool waitForUnlock = true;
    [Tooltip("Задача, когда записка появилась ({0} — палата, где она лежит)")]
    public string finalObjective = "Проверить тумбочку ({0})";
    [Tooltip("Сообщение, когда записка появилась")]
    public string finalMessage = "Где-то в палатах что-то упало...";
    [Tooltip("Задача, если Unlock () вызвали, а палаты ещё не все проверены")]
    public string notDoneObjective = "Закончить обход палат";

    bool allDone, unlocked, finished;
    int lastIndex = -1;

    public static RoomChecklist Instance { get; private set; }
    // Список показан на экране? Пока нет — палаты не засчитываются
    public bool IsShown => panel != null && panel.activeSelf;

    GameObject panel;
    Image[] checks;
    TMP_Text[] labels;
    int doneCount;

    void Awake()
    {
        Instance = this;
        Build();
        panel.SetActive(showOnStart);
    }

    void Build()
    {
        panel = new GameObject("Rows", typeof(RectTransform));
        RectTransform pr = (RectTransform)panel.transform;
        pr.SetParent(transform, false);
        pr.anchorMin = pr.anchorMax = new Vector2(0f, 1f);
        pr.pivot = new Vector2(0f, 1f);
        pr.anchoredPosition = Vector2.zero;

        checks = new Image[rooms.Length];
        labels = new TMP_Text[rooms.Length];
        float box = rowHeight * 0.7f;

        for (int i = 0; i < rooms.Length; i++)
        {
            float y = -i * rowHeight;
            // квадратик
            Image b = MakeImage("Box_" + (i + 1), pr, boxSprite, new Vector2(0f, y), box);
            if (boxSprite == null) b.color = new Color(1, 1, 1, 0.25f);
            // галочка поверх квадратика
            checks[i] = MakeImage("Check_" + (i + 1), pr, checkSprite, new Vector2(0f, y - box * 0.05f), box * 1.1f);
            if (checkSprite == null) checks[i].color = new Color(0.4f, 1f, 0.4f);
            checks[i].gameObject.SetActive(false);
            // название
            labels[i] = MakeText("Label_" + (i + 1), pr, rooms[i] != null ? rooms[i].name : "", new Vector2(box + 14f, y), textColor);
        }

        if (!string.IsNullOrEmpty(extraLine))
        {
            TMP_Text t = MakeText("Extra", pr, extraLine, new Vector2(box + 14f, -rooms.Length * rowHeight - 4f), pencilColor);
            t.fontStyle = FontStyles.Italic;
            t.rectTransform.localRotation = Quaternion.Euler(0, 0, 4f); // чуть криво, как от руки
        }
    }

    Image MakeImage(string n, Transform parent, Sprite sprite, Vector2 pos, float size)
    {
        GameObject go = new GameObject(n, typeof(RectTransform), typeof(Image));
        RectTransform r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
        r.pivot = new Vector2(0f, 1f);
        r.anchoredPosition = pos - new Vector2(0f, (rowHeight - size) / 2f);
        r.sizeDelta = new Vector2(size, size);
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    TMP_Text MakeText(string n, Transform parent, string text, Vector2 pos, Color color)
    {
        GameObject go = new GameObject(n, typeof(RectTransform));
        RectTransform r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
        r.pivot = new Vector2(0f, 1f);
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(500f, rowHeight);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        return t;
    }

    public void Show() { panel.SetActive(true); }
    public void Hide() { panel.SetActive(false); }

    // number: 1 = первая палата в списке, 2 = вторая ...
    public void Check(int number)
    {
        if (!IsShown) return; // список ещё не выдан — не считаем
        int i = number - 1;
        if (i < 0 || i >= rooms.Length || rooms[i].done) return;

        rooms[i].done = true;
        doneCount++;
        checks[i].gameObject.SetActive(true);
        labels[i].color = doneColor;

        if (GameManager.Instance != null && !string.IsNullOrEmpty(checkedMessage))
            GameManager.Instance.Say(string.Format(checkedMessage, rooms[i].name));

        if (countEvents != null)
            foreach (CountEvent e in countEvents)
                if (e != null && e.count == doneCount) e.onReached.Invoke();

        if (doneCount >= rooms.Length)
        {
            allDone = true;
            lastIndex = i;
            TryFinish();
        }
    }

    // Вызывать из события (например, On After Close у плана эвакуации)
    public void Unlock()
    {
        unlocked = true;
        if (!allDone)
        {
            if (ObjectiveUI.Instance != null && !string.IsNullOrEmpty(notDoneObjective))
                ObjectiveUI.Instance.SetObjective(notDoneObjective);
            return;
        }
        TryFinish();
    }

    void TryFinish()
    {
        if (finished || !allDone || (waitForUnlock && !unlocked)) return;
        finished = true;

        string roomName = lastIndex >= 0 ? rooms[lastIndex].name : "";
        // записка — на тумбочку последней палаты
        if (finalItem != null)
        {
            if (lastIndex >= 0 && rooms[lastIndex].notePoint != null)
            {
                finalItem.transform.position = rooms[lastIndex].notePoint.position;
                finalItem.transform.rotation = rooms[lastIndex].notePoint.rotation;
            }
            else Debug.LogWarning("RoomChecklist: у палаты " + roomName + " не заполнен Note Point — записка останется на своём месте");
            finalItem.SetActive(true);
        }
        if (GameManager.Instance != null && !string.IsNullOrEmpty(finalMessage))
            GameManager.Instance.ShowMessage(finalMessage, 4f);
        if (ObjectiveUI.Instance != null && !string.IsNullOrEmpty(finalObjective))
            ObjectiveUI.Instance.SetObjective(string.Format(finalObjective, roomName));
        onAllChecked.Invoke();
    }
}
