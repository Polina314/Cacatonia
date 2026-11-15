using System.Collections;
using UnityEngine;

// Выдвижной ящик. Повесить на КАЖДЫЙ ящик стола (Drawer_L1 ... Drawer_R3, Tray_L, Tray_R).
// Открывается/закрывается по E, если игрок смотрит на ящик (или через Toggle() из вашей системы взаимодействия).
public class Drawer : MonoBehaviour
{
    [Tooltip("Насколько и куда выезжает ящик (в локальных осях стола). " +
             "Если ящик уезжает ВНУТРЬ стола — поменяй знак Z (0.3 -> -0.3)")]
    public Vector3 openOffset = new Vector3(0f, 0f, 0.3f);
    [Tooltip("Время выдвигания, секунд")]
    public float moveTime = 0.35f;

    [Header("Своё управление (выключить, если у вас уже есть система взаимодействия, как у дверей)")]
    public bool useOwnInput = true;
    public KeyCode key = KeyCode.E;
    public float reachDistance = 2f;

    [Header("Звук (необязательно)")]
    public AudioSource audioSource;
    public AudioClip openSound, closeSound;

    public bool IsOpen { get; private set; }

    Vector3 closedPos;
    Coroutine moving;

    void Awake()
    {
        closedPos = transform.localPosition;
        // коллайдер, чтобы по ящику можно было "попасть" взглядом; едет вместе с ящиком
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

    public void Toggle() { if (IsOpen) Close(); else Open(); }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        Play(openSound);
        MoveTo(closedPos + openOffset);
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Play(closeSound);
        MoveTo(closedPos);
    }

    void MoveTo(Vector3 target)
    {
        if (moving != null) StopCoroutine(moving);
        moving = StartCoroutine(Move(target));
    }

    IEnumerator Move(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, moveTime);
            transform.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localPosition = target;
        moving = null;
    }

    void Play(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }
}
