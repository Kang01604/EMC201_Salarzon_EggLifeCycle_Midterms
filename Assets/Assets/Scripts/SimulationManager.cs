using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour {
    [Header("Prefabs")]
    public GameObject eggPrefab, chickPrefab, henPrefab, roosterPrefab;

    [Header("UI Elements")]
    public TMP_Text eggText, chickText, henText, roosterText;
    public TMP_Text timerText;
    public TMP_Text speedDisplayText;
    public Slider speedSlider;

    [Header("Terrain")]
    public Terrain roamTerrain;

    [Header("Walls")] 
    public Collider[] wallColliders;

    private int eggs, chicks, hens, roosters;
    private bool firstEgg = true;
    private float elapsedTime = 0f;
    private float speedMultiplier = 1f;

    public float SpeedMultiplier => speedMultiplier;

    void Start() {
        UpdateUI();
        SpawnEgg();
        
        speedSlider.minValue = 0.1f;
        speedSlider.maxValue = 10f;
        speedSlider.value = 1f;
        speedSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void FixedUpdate() {
        elapsedTime += Time.fixedDeltaTime * speedMultiplier;
        UpdateTimerUI();
        speedDisplayText.text = $"Speed: {speedMultiplier:F1}x";
    }

    void OnSliderChanged(float value) {
        speedMultiplier = value;
    }

    void UpdateTimerUI() {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    public void AddEgg()     { eggs++; UpdateUI(); }
    public void RemoveEgg()  { eggs--; UpdateUI(); }
    public void AddChick()   { chicks++; UpdateUI(); }
    public void RemoveChick(){ chicks--; UpdateUI(); }
    public void AddHen()     { hens++; UpdateUI(); }
    public void RemoveHen()  { hens--; UpdateUI(); }
    public void AddRooster() { roosters++; UpdateUI(); }
    public void RemoveRooster() { roosters--; UpdateUI(); }

    void UpdateUI() {
        eggText.text = $"Eggs: {eggs}";
        chickText.text = $"Chicks: {chicks}";
        henText.text = $"Hens: {hens}";
        roosterText.text = $"Roosters: {roosters}";
    }

    public void SpawnEgg() {
        GameObject egg = Instantiate(eggPrefab, transform.position, Quaternion.identity);
        if (firstEgg) {
            egg.GetComponent<Egg>().isFirstEgg = true;
            firstEgg = false;
        }
    }

    public Vector3 GetRandomPointInRoamArea() {
        if (roamTerrain == null) {
            return transform.position + new Vector3(
                Random.Range(-5f, 5f),
                0f,
                Random.Range(-5f, 5f)
            );
        }

        Vector3 terrainSize = roamTerrain.terrainData.size;
        Vector3 terrainPos = roamTerrain.transform.position;

        float x = terrainPos.x + Random.Range(0f, terrainSize.x);
        float z = terrainPos.z + Random.Range(0f, terrainSize.z);
        float y = roamTerrain.SampleHeight(new Vector3(x, 0, z)) + 0.1f;

        return new Vector3(x, y, z);
    }

    public bool IsCollidingWithWall(Collider entityCollider) {
        if (wallColliders == null || wallColliders.Length == 0) return false;

        foreach (var wall in wallColliders) {
            if (wall != null && wall.bounds.Intersects(entityCollider.bounds)) {
                return true;
            }
        }
        return false;
    }
}
