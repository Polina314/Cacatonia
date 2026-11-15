using System.Collections;
using UnityEngine;

// Дверца шкафа. Повесить на Wardrobe_Door_L и Wardrobe_Door_R.
// Открывается/закрывается по E, если игрок смотрит на дверцу (или через Toggle() из вашей системы взаимодействия).
public class CabinetDoor : MonoBehaviour
{
    [Tooltip("Угол открытия. Для Wardrobe_Door_L: 100, для Wardrobe_Door_R: -100. " +
             "Если дверца открывается ВНУТРЬ шкафа — поменяй знак")]
    public float openAngle = 100f;
    public float moveTime = 0.5f;

    [Header("Своё управление (выключить, если уже есть система взаимодействия)")]
    public bool useOwnInput = true;
    public KeyCode key = KeyCode.E;
    public float reachDistance = 2f;

    [Header("Заперта? (тогда не открывается, пока не вызвать Unlock())")]
    public bool locked = false;

    [Header("Звук (необязательно)")]
    public AudioSource audioSource;
    public AudioClip openSound, closeSound, lockedSound;

    public bool IsOpen { get; private set; }

    Quaternion closedRot;
    Coroutine moving;

    void Awake()
    {
        closedRot = transform.localRotation;
        if (!GetComponent<Collider>()) gameObject.AddComponent<BoxCollider>();
    }

    void Update()
    {
        if (!useOwnInput || !Input.GetKeyDown(key)) return;
        Camera cam = Camera.main;
        if (!cam) return;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, reachDistance)
            && hit.collider.transform == transform)
            Toggle();
    }

    public void Unlock() { locked = false; }

    public void Toggle()
    {
        if (locked) { Play(lockedSound); return; }
        IsOpen = !IsOpen;
        Play(IsOpen ? openSound : closeSound);
        if (moving != null) StopCoroutine(moving);
        moving = StartCoroutine(Rotate(IsOpen ? closedRot * Quaternion.Euler(0f, openAngle, 0f) : closedRot));
    }

    IEnumerator Rotate(Quaternion target)
    {
        Quaternion start = transform.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, moveTime);
            transform.localRotation = Quaternion.Slerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localRotation = target;
        moving = null;
    }

    void Play(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }
}
