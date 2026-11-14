using System.Collections;
using UnityEngine;

// Звук ЗА СПИНОЙ игрока: шаги в темноте, шёпот, скрип.
// Звук каждый раз ставится сзади игрока (куда бы он ни повернулся) и приближается.
// Вешается на пустой объект. Запуск из события: SoundBehind → Play ().
public class SoundBehind : MonoBehaviour
{
    public AudioClip clip;
    [Tooltip("Сколько раз повторить (для шагов — количество шагов)")]
    public int repeats = 7;
    [Tooltip("Пауза между повторами (секунды)")]
    public float interval = 0.6f;
    [Tooltip("Расстояние за спиной в начале и в конце (шаги приближаются)")]
    public float startDistance = 6f;
    public float endDistance = 2f;
    [Range(0f, 1f)] public float volume = 1f;

    AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;               // 3D: слышно, с какой стороны
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = 15f;
    }

    public void Play()
    {
        if (clip != null) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        FirstPersonController player = FindObjectOfType<FirstPersonController>();
        if (player == null) yield break;

        for (int i = 0; i < repeats; i++)
        {
            float k = repeats > 1 ? (float)i / (repeats - 1) : 1f;
            float dist = Mathf.Lerp(startDistance, endDistance, k);
            Vector3 back = -player.transform.forward; back.y = 0f; back.Normalize();
            transform.position = player.transform.position + back * dist + Vector3.down * 0.7f;
            source.pitch = Random.Range(0.9f, 1.1f);
            source.PlayOneShot(clip, volume);
            yield return new WaitForSeconds(interval);
        }
    }
}
