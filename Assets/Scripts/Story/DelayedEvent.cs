using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// «Сделать что-то через N секунд». Вешается на пустой объект.
// Запуск из любого события: DelayedEvent → Run ().
public class DelayedEvent : MonoBehaviour
{
    public float delaySeconds = 3f;
    public UnityEvent onDone;

    public void Run()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(delaySeconds);
        onDone.Invoke();
    }
}
