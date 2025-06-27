using UnityEngine;
using System.Collections;

public class Egg : MonoBehaviour {
    private float hatchTime = 10f;
    private SimulationManager manager;
    public bool isFirstEgg = false;
    private float indicatorTime = 1f;
    private float elapsedTime = 0f;
    private Renderer rend;

    void Start() {
        manager = Object.FindAnyObjectByType<SimulationManager>();
        rend = GetComponent<Renderer>();
        if (manager != null) {
            manager.AddEgg();
        }
        
        if (manager != null && manager.roamTerrain != null) {
            float terrainHeight = manager.roamTerrain.SampleHeight(transform.position) + 
                                manager.roamTerrain.transform.position.y;
            if (transform.position.y <= terrainHeight) {
                transform.position = new Vector3(
                    transform.position.x,
                    terrainHeight + 0.2f,
                    transform.position.z
                );
            }
        }
    }

    void FixedUpdate() {
        elapsedTime += Time.deltaTime * (manager != null ? manager.SpeedMultiplier : 1f);
        
        if (elapsedTime >= hatchTime - indicatorTime && rend != null) {
            rend.material.SetColor("_EmissionColor", Color.yellow);
        }
        
        if (elapsedTime >= hatchTime) {
            if (manager != null) {
                manager.RemoveEgg();
                Instantiate(manager.chickPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
