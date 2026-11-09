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
    public float wallThickness = 1f;
    public float settleSpeed = 0.1f;
    public float followGain = 25f;
    public float maxHoldSpeed = 40f;
    public float throwBoost = 1.3f;
    public float minThrowSpeed = 3f;
    public float maxThrowSpeed = 28f;
    public float torqueImpulse = 10f;
    public float linearDamping = 1.5f;

    public event Action OnThrown;
    public event Action OnResolved;

    public bool Locked { get; private set; } = true;
    public bool IsRolling => rolling;

    static readonly Color UnlockedColor = Color.white;
    static readonly Color LockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    Image image;
    RectTransform diceRect;
    Rigidbody2D body;
    Vector2 origin = new Vector2(500f, 500f);
    Vector2 holdTarget;
    bool ready, holding, rolling;
    float rollTimer, stillTimer, flickerTimer;

    IEnumerator Start()
    {
        diceRect = (RectTransform)transform;
        image = GetComponent<Image>();
        image.sprite = D20Sprite.Get();
        image.preserveAspect = true;
        SetLocked(Locked);

        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        BuildPhysics();
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
        body.linearDamping = linearDamping;
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

    void FixedUpdate()
    {
        if (!ready || !holding) return;

        Vector2 targetWorld = origin + holdTarget / pixelsPerUnit;
        Vector2 v = (targetWorld - body.position) * followGain;
        body.linearVelocity = Vector2.ClampMagnitude(v, maxHoldSpeed);
        body.angularVelocity = 0f;
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

    public void SetLocked(bool locked)
    {
        Locked = locked;
        if (image != null) image.color = locked ? LockedColor : UnlockedColor;

        if (locked && holding) ReleaseHold(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!ready || rolling || Locked) return;

        holding = true;
        holdTarget = PointerLocal(eventData);
        body.linearDamping = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (holding) holdTarget = PointerLocal(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (holding) ReleaseHold(true);
    }

    void ReleaseHold(bool allowThrow)
    {
        holding = false;
        body.linearDamping = linearDamping;

        Vector2 v = body.linearVelocity * throwBoost;
        if (!allowThrow || v.magnitude < minThrowSpeed)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Launch(Vector2.ClampMagnitude(v, maxThrowSpeed));
    }

    Vector2 PointerLocal(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, e.position, e.pressEventCamera, out Vector2 p);
        return p;
    }

    public void AutoThrow()
    {
        if (!ready || rolling) return;
        Launch(UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(10f, 20f));
    }

    void Launch(Vector2 velocity)
    {
        rolling = true;
        rollTimer = stillTimer = flickerTimer = 0f;

        body.linearVelocity = velocity;
        body.angularVelocity = 0f;
        float speedFactor = Mathf.Clamp(velocity.magnitude / 10f, 0.6f, 1.5f);
        body.AddTorque(UnityEngine.Random.Range(0.6f, 1f) * torqueImpulse * speedFactor * (UnityEngine.Random.value < 0.5f ? -1f : 1f), ForceMode2D.Impulse);

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
