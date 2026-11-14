using UnityEngine;
using UnityEngine.Events;

// «Посвети фонариком на предмет» — например, на кровать в палате.
// Срабатывает один раз, когда фонарик игрока включён, предмет попал в луч
// и игрок держит на нём свет Hold Time секунд (и ничего не загораживает).
// Вешается на предмет (кровать). У предмета или его деталей должен быть Collider.
public class FlashlightTarget : MonoBehaviour
{
    [Tooltip("Сколько секунд держать свет на предмете")]
    public float holdTime = 0.8f;
    [Tooltip("С какого расстояния засчитывается (метры)")]
    public float maxDistance = 6f;

    [Tooltip("Зона палаты (Box Collider с Is Trigger). Если указана — засчитывается, только когда игрок ВНУТРИ палаты, а не из коридора")]
    public Collider insideZone;

    [Tooltip("Что произойдёт, когда посветили")]
    public UnityEvent onLit;

    Flashlight flashlight;
    float timer;
    bool done;

    void Update()
    {
        if (done) return;
        if (flashlight == null) flashlight = FindObjectOfType<Flashlight>();
        if (flashlight == null || !flashlight.IsOn) { timer = 0f; return; }
        // Палаты считаются только после того, как на экране появился список палат
        if (RoomChecklist.Instance != null && !RoomChecklist.Instance.IsShown) { timer = 0f; return; }

        // игрок должен стоять в палате, а не светить из коридора
        if (insideZone != null && !insideZone.bounds.Contains(flashlight.transform.position)) { timer = 0f; return; }

        if (IsLit()) timer += Time.deltaTime;
        else timer = 0f;

        if (timer >= holdTime)
        {
            done = true;
            onLit.Invoke();
        }
    }

    bool IsLit()
    {
        Light lamp = flashlight.Lamp;
        Vector3 from = lamp.transform.position;
        Vector3 target = Center();
        Vector3 dir = target - from;
        float dist = dir.magnitude;

        if (dist > Mathf.Min(maxDistance, lamp.range)) return false;

        // в конусе луча? (берём чуть шире половины угла — чтобы не надо было целиться в пиксель)
        float half = lamp.type == LightType.Spot ? lamp.spotAngle * 0.5f + 5f : 30f;
        if (Vector3.Angle(lamp.transform.forward, dir) > half) return false;

        // ничего не загораживает? (стена, дверь)
        if (Physics.Raycast(from, dir / dist, out RaycastHit hit, dist + 0.5f, ~0, QueryTriggerInteraction.Ignore))
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        return true;
    }

    Vector3 Center()
    {
        Collider c = GetComponentInChildren<Collider>();
        if (c != null) return c.bounds.center;
        Renderer r = GetComponentInChildren<Renderer>();
        return r != null ? r.bounds.center : transform.position;
    }
}
