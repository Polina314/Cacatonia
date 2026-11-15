using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Рубильник в электрощитке. Повесить на объект "рубильник" (Electric_Lever).
// Смотришь на ручку + E -> рычаг переключается вверх/вниз и вызывает события (например, выключить/включить свет на этаже).
public class ElectricLever : MonoBehaviour
{
    [Tooltip("Ось поворота в локальных координатах. Обычно X. Если рычаг крутится не туда — попробуй (0,0,1) или поменяй знак угла")]
    public Vector3 axis = new Vector3(1, 0, 0);
    [Tooltip("На сколько градусов опускается рычаг")]
    public float angle = -150f;
    public float moveTime = 0.25f;
    public bool startOn = true;

    [Header("Своё управление (выключить, если есть своя система взаимодействия)")]
    public bool useOwnInput = true;
    public KeyCode key = KeyCode.E;
    public float reachDistance = 2f;

    [Header("События")]
    public UnityEvent onTurnedOn;
    public UnityEvent onTurnedOff;

    [Header("Звук (необязательно)")]
    public AudioSource audioSource;
    public AudioClip switchSound;

    public bool IsOn { get; private set; }
    Quaternion onRot;
    Coroutine moving;

    void Awake()
    {
        onRot = transform.localRotation;
        IsOn = startOn;
        if (!IsOn) transform.localRotation = onRot * Quaternion.AngleAxis(angle, axis);
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

    public void Toggle()
    {
        IsOn = !IsOn;
        if (audioSource && switchSound) audioSource.PlayOneShot(switchSound);
        if (moving != null) StopCoroutine(moving);
        moving = StartCoroutine(Rotate(IsOn ? onRot : onRot * Quaternion.AngleAxis(angle, axis)));
        if (IsOn) onTurnedOn.Invoke(); else onTurnedOff.Invoke();
    }

    IEnumerator Rotate(Quaternion target)
    {
        Quaternion start = transform.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, moveTime);
            transform.localRotation = Quaternion.Slerp(start, target, t);
            yield return null;
        }
        transform.localRotation = target;
        moving = null;
    }
}
