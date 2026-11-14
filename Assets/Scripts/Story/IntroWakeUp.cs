using System.Collections;
using UnityEngine;

// Начало игры: экран чёрный, герой «просыпается» — экран медленно светлеет,
// пара морганий, мысль героя внизу экрана и первая задача.
// Вешается на любой пустой объект (например, Intro).
// Нужны: ScreenFader (Fader в Canvas), GameManager, ObjectiveUI.
public class IntroWakeUp : MonoBehaviour
{
    [Tooltip("Сколько секунд экран полностью чёрный")]
    public float blackTime = 1.5f;
    [Tooltip("Сколько раз моргнуть перед тем, как открыть глаза")]
    public int blinks = 2;
    [Tooltip("Сколько секунд экран светлеет в конце")]
    public float wakeTime = 2.5f;

    [TextArea] public string thought = "Чёрт... я уснул прямо на смене. Надо сходить в ординаторскую, узнать, что мне делать.";
    public string objective = "Пойти в ординаторскую";

    IEnumerator Start()
    {
        FirstPersonController player = FindObjectOfType<FirstPersonController>();
        CanvasGroup fade = ScreenFader.Instance != null ? ScreenFader.Instance.GetComponent<CanvasGroup>() : null;

        if (player != null) player.canMove = false;
        if (ObjectiveUI.Instance != null) ObjectiveUI.Instance.SetObjective("");
        if (fade != null) fade.alpha = 1f;

        yield return new WaitForSeconds(blackTime);

        // моргания: чуть приоткрыл глаза — снова закрыл
        for (int i = 0; i < blinks && fade != null; i++)
        {
            yield return FadeTo(fade, 0.55f, 0.5f);
            yield return FadeTo(fade, 1f, 0.35f);
            yield return new WaitForSeconds(0.4f);
        }

        if (fade != null) yield return FadeTo(fade, 0f, wakeTime);
        if (player != null) player.canMove = true;

        if (GameManager.Instance != null && !string.IsNullOrEmpty(thought)) GameManager.Instance.ShowMessage(thought, 5f);
        if (ObjectiveUI.Instance != null) ObjectiveUI.Instance.SetObjective(objective);
    }

    IEnumerator FadeTo(CanvasGroup g, float target, float time)
    {
        float start = g.alpha, t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        g.alpha = target;
    }
}
