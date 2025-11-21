using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class FirebasePumpController : MonoBehaviour
{
    public TMP_Text waterQuantityText;
    public TMP_Text pumpStatusText;

    public TMP_Text temperatureText;      // NEW
    public TMP_Text humidityText;         // NEW
    public TMP_Text soilMoistureText;     // NEW

    public GameObject waterLayer;

    private DatabaseReference db;
    private bool firebaseReady = false;

    void Start()
    {
        Debug.Log("Initializing Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError("Firebase Init Failed: " + task.Result);
                return;
            }

            Debug.Log("Firebase Dependencies OK");

            FirebaseApp app = FirebaseApp.DefaultInstance;

            FirebaseDatabase dbInstance = FirebaseDatabase.GetInstance(
                app,
                "https://ml-firebase-demo-default-rtdb.firebaseio.com/"
            );

            db = dbInstance.RootReference;
            firebaseReady = true;

            Debug.Log("Database Ready!");

            ListenPumpStatus();
            ListenWaterQuantity();

            ListenTemperature();    // NEW
            ListenHumidity();       // NEW
            ListenSoilMoisture();   // NEW
        });
    }

    // ----------------------------------------------------
    // LISTENER: PUMP STATUS
    // ----------------------------------------------------
    void ListenPumpStatus()
    {
        db.Child("irrigation").Child("pump_status").ValueChanged += (sender, e) =>
        {
            if (!e.Snapshot.Exists) return;

            string status = e.Snapshot.Value.ToString();
            pumpStatusText.text = "Pump Status: " + status;

            if (status == "ON")
                waterLayer.SetActive(true);
            else
                waterLayer.SetActive(false);
        };
    }

    // ----------------------------------------------------
    // LISTENER: WATER QUANTITY
    // ----------------------------------------------------
   // ----------------------------------------------------
// LISTENER: WATER QUANTITY
// ----------------------------------------------------
void ListenWaterQuantity()
{
    db.Child("irrigation").Child("water_quantity_lph").ValueChanged += (sender, e) =>
    {
        if (!e.Snapshot.Exists) return;

        float qty = float.Parse(e.Snapshot.Value.ToString());
        waterQuantityText.text = "Water Qty Required: " + qty + " L";

        // AUTO LOGIC: If qty > 0 → pump ON
        if (qty > 0.01f)
        {
            Debug.Log("Water quantity > 0 → turning pump ON");
            db.Child("irrigation").Child("pump_status").SetValueAsync("ON");

            // turn on water visual layer
            waterLayer.SetActive(true);
        }

        // AUTO LOGIC: If qty == 0 → pump OFF
        if (qty <= 0.01f)
        {
            Debug.Log("Water quantity is zero → turning pump OFF");
            db.Child("irrigation").Child("pump_status").SetValueAsync("OFF");

            // hide water visual layer
            waterLayer.SetActive(false);
        }
    };
}


    // ----------------------------------------------------
    // LISTENER: TEMPERATURE  (NEW)
    // ----------------------------------------------------
    void ListenTemperature()
    {
        db.Child("arduino").Child("live_data").Child("temperature")
            .ValueChanged += (sender, e) =>
        {
            if (!e.Snapshot.Exists) return;

            string value = e.Snapshot.Value.ToString();
            temperatureText.text = "Temperature: " + value + " °C";
        };
    }

    // ----------------------------------------------------
    // LISTENER: HUMIDITY  (NEW)
    // ----------------------------------------------------
    void ListenHumidity()
    {
        db.Child("arduino").Child("live_data").Child("humidity")
            .ValueChanged += (sender, e) =>
        {
            if (!e.Snapshot.Exists) return;

            string value = e.Snapshot.Value.ToString();
            humidityText.text = "Humidity: " + value + " %";
        };
    }

    // ----------------------------------------------------
    // LISTENER: SOIL MOISTURE  (NEW)
    // ----------------------------------------------------
    void ListenSoilMoisture()
    {
        db.Child("arduino").Child("live_data").Child("soil_moisture")
            .ValueChanged += (sender, e) =>
        {
            if (!e.Snapshot.Exists) return;

            string value = e.Snapshot.Value.ToString();
            soilMoistureText.text = "Soil Moisture: " + value + " %";
        };
    }

    // ----------------------------------------------------
    // BUTTON: TURN PUMP ON
    // ----------------------------------------------------
    public void SetPumpOn()
    {
        if (!firebaseReady) return;
        db.Child("irrigation").Child("pump_status").SetValueAsync("ON");
        waterLayer.SetActive(true);
    }

    // ----------------------------------------------------
    // BUTTON: TURN PUMP OFF
    // ----------------------------------------------------
    public void SetPumpOff()
    {
        if (!firebaseReady) return;
        db.Child("irrigation").Child("pump_status").SetValueAsync("OFF");
        waterLayer.SetActive(false);
    }
}
