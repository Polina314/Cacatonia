using UnityEngine;

// Предмет сам покачивается: капельница, лампа на проводе, табличка.
// Вешается на объект, точка которого — это точка подвеса или основание
// (для капельницы — пустой объект на полу, а стойка и пакет — внутри него).
public class Sway : MonoBehaviour
{
    [Tooltip("Наклон в градусах (на сколько отклоняется)")]
    public float angle = 4f;
    [Tooltip("Скорость качания (качаний в секунду)")]
    public float speed = 0.5f;
    [Tooltip("Вокруг какой оси качается. (1,0,0) — вперёд-назад, (0,0,1) — влево-вправо")]
    public Vector3 axis = new Vector3(1f, 0f, 0.3f);
    [Tooltip("Качаться сразу с начала игры")]
    public bool playOnStart = true;

    [Header("Скрип (необязательно)")]
    public AudioSource creak;

    Quaternion startRot;
    bool playing;
    float phase;

    void Start()
    {
        startRot = transform.localRotation;
        phase = Random.Range(0f, 10f);
        if (playOnStart) Play();
    }

    void Update()
    {
        if (!playing) return;
        float t = Time.time + phase;
        // две синусоиды — качание неровное, «живое»
        float a = (Mathf.Sin(t * speed * Mathf.PI * 2f) + 0.3f * Mathf.Sin(t * speed * 5.3f)) / 1.3f * angle;
        transform.localRotation = startRot * Quaternion.AngleAxis(a, axis.normalized);
    }

    public void Play()
    {
        playing = true;
        if (creak != null && !creak.isPlaying) creak.Play();
    }

    public void Stop()
    {
        playing = false;
        transform.localRotation = startRot;
        if (creak != null) creak.Stop();
    }
}
