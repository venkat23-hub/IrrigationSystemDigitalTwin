using System.Collections;
using UnityEngine;

public class WaterToggleSystemEnhanced : MonoBehaviour
{
    [Header("Water Target")]
    [Tooltip("Either assign a single water GameObject (plane) or use 'useTag' and leave this empty.")]
    public GameObject waterLayer;                 // single water object (optional if using tag)
    [Tooltip("If true, all objects with 'waterTag' will be toggled")]
    public bool useTag = false;
    public string waterTag = "IrrigationWater";   // tag to find multiple water objects

    [Header("Toggle behaviour")]
    public float toggleInterval = 5f;            // seconds between toggles
    public bool startOn = false;                 // start with water visible?
    public bool useRendererToggle = false;       // enable/disable Renderer instead of GameObject
    public bool liftWhenOn = true;               // nudge water upward when ON to avoid z-fight
    public float liftAmount = 0.02f;             // meters to lift when ON
    public float smoothTransitionTime = 0.3f;    // smooth movement time (seconds). set 0 for instant

    // internal
    private bool isWaterOn;
    private Coroutine togglerCoroutine;

    void Awake()
    {
        // Basic validation
        if (!useTag && waterLayer == null)
        {
            Debug.LogWarning("[WaterToggle] No waterLayer assigned and useTag is false. Please assign waterLayer or enable useTag.");
        }

        if (useTag && string.IsNullOrEmpty(waterTag))
        {
            Debug.LogWarning("[WaterToggle] useTag is true but waterTag is empty.");
        }
    }

    void Start()
    {
        isWaterOn = startOn;
        // initialize visible state
        ApplyState(isWaterOn, instant: true);

        // start toggling coroutine
        togglerCoroutine = StartCoroutine(ToggleLoop());
    }

    IEnumerator ToggleLoop()
    {
        float timer = 0f;
        while (true)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer >= toggleInterval)
            {
                timer = 0f;
                isWaterOn = !isWaterOn;
                ApplyState(isWaterOn, instant: false);
                Debug.Log($"[WaterToggle] Motor simulated -> {(isWaterOn ? "ON (water present) 🌊" : "OFF (no irrigation) 🏜️")}");
            }
        }
    }

    // Apply new state to either single water object or all tagged ones
    void ApplyState(bool on, bool instant)
    {
        if (useTag)
        {
            GameObject[] waterObjects = GameObject.FindGameObjectsWithTag(waterTag);
            if (waterObjects == null || waterObjects.Length == 0)
            {
                Debug.LogWarning("[WaterToggle] No objects found with tag '" + waterTag + "'.");
                return;
            }

            foreach (GameObject go in waterObjects)
            {
                ApplyToObject(go, on, instant);
            }
        }
        else
        {
            if (waterLayer == null)
            {
                Debug.LogWarning("[WaterToggle] No waterLayer assigned.");
                return;
            }
            ApplyToObject(waterLayer, on, instant);
        }
    }

    void ApplyToObject(GameObject go, bool on, bool instant)
    {
        if (go == null) return;

        // Option A: Toggle whole GameObject active/inactive
        if (!useRendererToggle && !liftWhenOn)
        {
            go.SetActive(on);
            return;
        }

        // Option B: Toggle Renderer(s) (keeps object active in hierarchy)
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        if (useRendererToggle && rends.Length > 0)
        {
            foreach (var r in rends)
            {
                r.enabled = on;
            }
        }

        // Option C: Lift object slightly to avoid being fully occluded by terrain
        if (liftWhenOn)
        {
            // record target position
            Vector3 basePos = go.transform.position;
            Vector3 targetPos = basePos;

            if (on)
                targetPos.y = basePos.y + liftAmount;
            else
                targetPos.y = basePos.y - liftAmount; // move back down (assumes initial stored as base)

            // If instant or very small transition, set position directly
            if (instant || smoothTransitionTime <= 0f)
            {
                go.transform.position = targetPos;
            }
            else
            {
                // ensure coroutine per object not stacking: start a smooth move coroutine
                StartCoroutine(SmoothMove(go.transform, targetPos, smoothTransitionTime));
            }
        }

        // If neither renderer toggling nor lift toggling done, fallback to SetActive
        if (!useRendererToggle && !liftWhenOn)
        {
            go.SetActive(on);
        }
    }

    IEnumerator SmoothMove(Transform t, Vector3 targetPos, float duration)
    {
        Vector3 start = t.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float f = Mathf.Clamp01(elapsed / duration);
            t.position = Vector3.Lerp(start, targetPos, f);
            yield return null;
        }
        t.position = targetPos;
    }

    // Optional: public control from other scripts (e.g., your real motor switch)
    public void SetWaterState(bool on)
    {
        isWaterOn = on;
        ApplyState(isWaterOn, instant: false);
    }

    void OnDisable()
    {
        if (togglerCoroutine != null) StopCoroutine(togglerCoroutine);
    }
}
