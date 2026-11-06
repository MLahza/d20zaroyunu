using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThrowableDice2D : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform area;
    public Text valueText;
    public MatchTurnManager manager;

    public float pixelsPerUnit = 100f;
    public float powerMultiplier = 6f;
    public float maxPullPixels = 200f;
    public float minPullPixels = 15f;
    public float wallThickness = 1f;
    public float settleSpeed = 0.1f;
    public float torqueImpulse = 10f;

    public event Action OnThrown;
    public event Action OnResolved;

    RectTransform diceRect;
    Rigidbody2D body;
    RectTransform aimLine;
    Vector2 origin = new Vector2(500f, 500f);
    bool ready, aiming, rolling;
    float rollTimer, stillTimer, flickerTimer;

    IEnumerator Start()
    {
        diceRect = (RectTransform)transform;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        BuildPhysics();
        BuildAimLine();
        ready = true;
    }

    void BuildPhysics()
    {
        var world = new GameObject("DiceWorld2D");
        world.transform.position = origin;

        Vector2 size = area.rect.size / pixelsPerUnit;
        Vector2 half = size * 0.5f;
        float t = wallThickness;

        var mat = new PhysicsMaterial2D("DiceBounce") { bounciness = 0.8f, friction = 0.1f };

        CreateWall(world.transform, mat, "Wall_Top", new Vector2(0f, half.y + t * 0.5f), new Vector2(size.x + 2f * t, t));
        CreateWall(world.transform, mat, "Wall_Bottom", new Vector2(0f, -half.y - t * 0.5f), new Vector2(size.x + 2f * t, t));
        CreateWall(world.transform, mat, "Wall_Left", new Vector2(-half.x - t * 0.5f, 0f), new Vector2(t, size.y));
        CreateWall(world.transform, mat, "Wall_Right", new Vector2(half.x + t * 0.5f, 0f), new Vector2(t, size.y));

        var bodyGo = new GameObject("DiceBody");
        bodyGo.transform.SetParent(world.transform, false);
        bodyGo.transform.position = origin + diceRect.anchoredPosition / pixelsPerUnit;

        body = bodyGo.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.linearDamping = 1.2f;
        body.angularDamping = 0.8f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.sharedMaterial = mat;

        var col = bodyGo.AddComponent<CircleCollider2D>();
        col.radius = diceRect.rect.width * 0.5f / pixelsPerUnit;
        col.sharedMaterial = mat;
    }

    static void CreateWall(Transform parent, PhysicsMaterial2D mat, string name, Vector2 localPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var box = go.AddComponent<BoxCollider2D>();
        box.size = size;
        box.sharedMaterial = mat;
    }

    void BuildAimLine()
    {
        var go = new GameObject("AimLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(area, false);
        aimLine = (RectTransform)go.transform;
        aimLine.anchorMin = aimLine.anchorMax = aimLine.pivot = new Vector2(0.5f, 0.5f);
        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.8f);
        img.raycastTarget = false;
        go.SetActive(false);
    }

    void Update()
    {
        if (!ready) return;

        diceRect.anchoredPosition = ((Vector2)body.position - origin) * pixelsPerUnit;
        diceRect.localRotation = Quaternion.Euler(0f, 0f, body.rotation);

        if (!rolling) return;

        rollTimer += Time.deltaTime;
        flickerTimer += Time.deltaTime;
        if (flickerTimer > 0.07f)
        {
            flickerTimer = 0f;
            valueText.text = UnityEngine.Random.Range(1, 21).ToString();
        }

        if (rollTimer > 0.3f && body.linearVelocity.magnitude < settleSpeed) stillTimer += Time.deltaTime;
        else stillTimer = 0f;

        if (stillTimer > 0.2f || rollTimer > 12f) Settle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!ready || rolling) return;
        aiming = true;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        aimLine.gameObject.SetActive(true);
        UpdateAim(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (aiming) UpdateAim(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!aiming) return;
        aiming = false;
        aimLine.gameObject.SetActive(false);

        Vector2 pull = diceRect.anchoredPosition - PointerLocal(eventData);
        float mag = pull.magnitude;
        if (mag < minPullPixels) return;

        float speed = Mathf.Min(mag, maxPullPixels) / pixelsPerUnit * powerMultiplier;
        Launch(pull.normalized * speed);
    }

    Vector2 PointerLocal(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, e.position, e.pressEventCamera, out Vector2 p);
        return p;
    }

    void UpdateAim(PointerEventData e)
    {
        Vector2 a = diceRect.anchoredPosition;
        Vector2 d = PointerLocal(e) - a;
        aimLine.anchoredPosition = a + d * 0.5f;
        aimLine.sizeDelta = new Vector2(d.magnitude, 6f);
        aimLine.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    public void ThrowRandom()
    {
        if (!ready || rolling) return;
        Launch(UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(8f, 14f));
    }

    void Launch(Vector2 velocity)
    {
        rolling = true;
        rollTimer = stillTimer = flickerTimer = 0f;

        body.linearVelocity = velocity;
        body.angularVelocity = 0f;
        body.AddTorque(UnityEngine.Random.Range(0.6f, 1f) * torqueImpulse * (UnityEngine.Random.value < 0.5f ? -1f : 1f), ForceMode2D.Impulse);

        OnThrown?.Invoke();
    }

    void Settle()
    {
        rolling = false;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.SetRotation(0f);

        int result = UnityEngine.Random.Range(1, 21);
        valueText.text = result.ToString();

        manager.RollDiceAndExecute(result);
        OnResolved?.Invoke();
    }
}
