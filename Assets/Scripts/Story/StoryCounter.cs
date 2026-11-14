using UnityEngine;
using UnityEngine.Events;

// Счётчик для задания: «Палаты 0 / 5», «Рубильники 0 / 3».
// Каждый счётчик — свой объект со своим событием «когда всё собрано».
// Вешается на пустой объект (например, Counter_Rooms).
//   Begin()  — показать задачу со счётчиком вверху экрана (вызывать из события).
//   Add()    — +1 (вызывать из зоны в палате, из рубильника и т.п.).
// Пока Begin() не вызван, Add() ничего не делает.
public class StoryCounter : MonoBehaviour
{
    [Tooltip("Текст задачи, например: Отметить палаты из списка")]
    public string title = "Отметить палаты из списка";
    [Tooltip("Сколько всего нужно")]
    public int total = 5;

    [Tooltip("Что произойдёт, когда счётчик заполнится")]
    public UnityEvent onComplete;

    int current;
    bool started, finished;

    public void Begin()
    {
        started = true;
        finished = false;
        current = 0;
        if (ObjectiveUI.Instance != null) ObjectiveUI.Instance.StartCounterWithTotal(title, total);
    }

    public void Add()
    {
        if (!started || finished) return;
        current++;
        if (ObjectiveUI.Instance != null) ObjectiveUI.Instance.AddProgress();
        if (current >= total)
        {
            finished = true;
            onComplete.Invoke();
        }
    }
}
