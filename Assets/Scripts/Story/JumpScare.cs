using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Скример: монстр появляется СЗАДИ игрока, игрока резко разворачивает к нему,
// монстр стоит перед лицом несколько секунд и исчезает.
// Вешается на любой пустой объект (например, JumpScare_Ch1).
// Запуск из события: JumpScare → Play ().
public class JumpScare : MonoBehaviour
{
    [Tooltip("Монстр (или временная заглушка). В начале игры галочка у него снята")]
    public GameObject monster;
    [Tooltip("На каком расстоянии за спиной игрока появится монстр")]
    public float distanceBehind = 1.2f;
    [Tooltip("Высота лица монстра над полом (куда игрок посмотрит)")]
    public float faceHeight = 1.7f;

    [Header("Время")]
    [Tooltip("Пауза перед поворотом: игрок слышит шорох за спиной")]
    public float delayBeforeTurn = 0.6f;
    [Tooltip("За сколько секунд игрока разворачивает")]
    public float turnTime = 0.35f;
    [Tooltip("Сколько секунд монстр стоит перед игроком")]
    public float showTime = 3f;

    [Header("Звук (необязательно)")]
    public AudioSource audioSource;
    public AudioClip rustleSound;   // шорох за спиной
    public AudioClip screamSound;   // крик при повороте

    [Header("Тряска камеры")]
    public float shakeStrength = 0.06f;

    [Tooltip("Что произойдёт после скримера (монстр уже исчез)")]
    public UnityEvent onFinished;

    bool playing;

    public void Play()
    {
        if (!playing && monster != null) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        playing = true;
        FirstPersonController player = FindObjectOfType<FirstPersonController>();
        if (player == null) { playing = false; yield break; }
        Transform body = player.transform;
        Transform cam = player.cameraHolder;

        player.canMove = false;

        // --- ставим монстра за спину ---
        Vector3 back = -body.forward; back.y = 0f; back.Normalize();
        float dist = distanceBehind;
        // если сзади стена — ставим ближе
        if (Physics.Raycast(body.position, back, out RaycastHit wall, distanceBehind + 0.3f, ~0, QueryTriggerInteraction.Ignore))
            dist = Mathf.Max(0.6f, wall.distance - 0.3f);
        Vector3 pos = body.position + back * dist;
        // ноги — на пол
        if (Physics.Raycast(pos + Vector3.up * 0.5f, Vector3.down, out RaycastHit floor, 5f, ~0, QueryTriggerInteraction.Ignore))
            pos.y = floor.point.y;
        monster.transform.position = pos;
        Vector3 toPlayer = body.position - pos; toPlayer.y = 0f;
        monster.transform.rotation = Quaternion.LookRotation(toPlayer);
        monster.SetActive(true);

        if (audioSource != null && rustleSound != null) audioSource.PlayOneShot(rustleSound);
        yield return new WaitForSeconds(delayBeforeTurn);

        // --- резкий разворот к монстру ---
        if (audioSource != null && screamSound != null) audioSource.PlayOneShot(screamSound);
        Quaternion bodyFrom = body.rotation;
        Quaternion bodyTo = Quaternion.LookRotation(-toPlayer);
        Quaternion camFrom = cam.localRotation;
        Vector3 face = pos + Vector3.up * faceHeight;
        float pitch = -Mathf.Atan2(face.y - cam.position.y, dist) * Mathf.Rad2Deg;
        Quaternion camTo = Quaternion.Euler(pitch, 0f, 0f);
        Vector3 camPos = cam.localPosition;

        float t = 0f;
        while (t < turnTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / turnTime);
            body.rotation = Quaternion.Slerp(bodyFrom, bodyTo, k);
            cam.localRotation = Quaternion.Slerp(camFrom, camTo, k);
            yield return null;
        }

        // --- монстр стоит перед лицом, камера трясётся ---
        t = 0f;
        while (t < showTime)
        {
            t += Time.deltaTime;
            float s = shakeStrength * Mathf.Clamp01(1f - t / showTime);
            cam.localPosition = camPos + (Vector3)Random.insideUnitCircle * s;
            yield return null;
        }
        cam.localPosition = camPos;

        // --- исчез ---
        monster.SetActive(false);
        cam.localRotation = camFrom;   // возвращаем наклон, как был
        player.canMove = true;
        playing = false;
        onFinished.Invoke();
    }
}
