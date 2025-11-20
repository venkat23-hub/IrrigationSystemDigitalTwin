using UnityEngine;

public class WaterToggleSystem : MonoBehaviour
{
    [Header("Water Layer Settings")]
    public GameObject waterLayer;   // Assign your water plane or object here
    public float toggleInterval = 5f; // seconds (5 sec ON, 5 sec OFF)

    private bool isWaterOn = false;
    private float timer = 0f;

    void Start()
    {
        if (waterLayer == null)
        {
            Debug.LogError("❌ Assign your Water Layer GameObject in the Inspector!");
            return;
        }

        // Start with water OFF
        waterLayer.SetActive(false);
        Debug.Log("💧 Water system initialized — starting OFF");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= toggleInterval)
        {
            // Toggle water visibility
            isWaterOn = !isWaterOn;
            waterLayer.SetActive(isWaterOn);

            string state = isWaterOn ? "ON 🌊" : "OFF 🏜️";
            Debug.Log($"💡 Motor switched {state}");

            // Reset timer
            timer = 0f;
        }
    }
}
