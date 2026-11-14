using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Записка, журнал, документ, план эвакуации: нажал E — текст (или картинка) на экране.
// Вешается на модель записки (у неё должен быть Collider).
public class NoteItem : MonoBehaviour, IInteractable
{
    public string prompt = "Прочитать";
    public string title = "Журнал дежурной медсестры";
    [TextArea(4, 12)]
    public string text = "03:13 — ушла на обход 2 этажа.";
    [Tooltip("Картинка вместо или вместе с текстом (необязательно)")]
    public Sprite image;

    [Tooltip("Галочка — после прочтения игрок забирает записку с собой (она исчезает со стола)")]
    public bool takeAfterReading = false;

    [Tooltip("Сразу при первом открытии (пока записка на экране)")]
    public UnityEvent onFirstRead;

    [Tooltip("После того как игрок ЗАКРЫЛ записку в первый раз. Сюда удобно ставить мысли героя (Say) и новую задачу")]
    public UnityEvent onAfterClose;

    bool read;

    public string GetPrompt()
    {
        return prompt;
    }

    public void Interact(PlayerInteraction player)
    {
        if (NoteUI.Instance == null)
        {
            Debug.LogWarning("На сцене нет NoteUI — записку некуда показать");
            return;
        }
        if (NoteUI.Instance.IsOpen) return;

        NoteUI.Instance.Show(title, text, image);
        if (!read)
        {
            read = true;
            onFirstRead.Invoke();
            StartCoroutine(WaitClose(true));
        }
        else if (takeAfterReading)
        {
            StartCoroutine(WaitClose(false));
        }
    }

    IEnumerator WaitClose(bool first)
    {
        while (NoteUI.Instance != null && NoteUI.Instance.IsOpen) yield return null;
        if (first) onAfterClose.Invoke();
        if (takeAfterReading)
        {
            // Прячем не сразу, а в конце кадра — чтобы события успели отработать
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        }
    }
}
