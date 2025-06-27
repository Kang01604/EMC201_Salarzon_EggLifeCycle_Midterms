using UnityEngine;
using System.Collections;

public class Hen : MonoBehaviour {
    private SimulationManager manager;
    private Vector3 spawnPosition;
    private Vector3 wanderTarget;
    private Rigidbody rb;
    private bool isFallingOver = false;
    private float layDelay = 29.5f;
    private float perishTime = 40f;
    private float jumpForce = 3f;
    private float eggSpawnHeight = 0.2f;
    private float indicatorTime = 1f;
    private float elapsedTime = 0f;
    private bool hasLaidEggs;
    private Renderer rend;

    void Start() {
        manager = Object.FindAnyObjectByType<SimulationManager>();
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        if (manager != null) {
            manager.AddHen();
        }
        spawnPosition = transform.position;
        SetNewWanderTarget();
    }

    void FixedUpdate() {
        if (!isFallingOver) {
            Wander();
            CheckWallCollision();
        }

        elapsedTime += Time.deltaTime * (manager != null ? manager.SpeedMultiplier : 1f);
        
        if (!hasLaidEggs && elapsedTime >= layDelay) {
            StartCoroutine(LayEggsWithJump());
            hasLaidEggs = true;
        }
        
        if (elapsedTime >= perishTime - indicatorTime && rend != null) {
            rend.material.SetColor("_EmissionColor", Color.red);
        }
        
        if (elapsedTime >= perishTime) {
            if (manager != null) {
                manager.RemoveHen();
            }
            Destroy(gameObject);
        }
    }

    IEnumerator LayEggsWithJump() {
        if (rb != null) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(0.5f);
        LayEggs();
    }

    void LayEggs() {
        if (manager == null || manager.roamTerrain == null || manager.eggPrefab == null) return;

        int count = Random.Range(2, 11);
        float terrainHeight = manager.roamTerrain.SampleHeight(transform.position) + 
                            manager.roamTerrain.transform.position.y;

        for (int i = 0; i < count; i++) {
            Vector3 pos = transform.position;
            pos.y = terrainHeight + eggSpawnHeight;
            pos += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Instantiate(manager.eggPrefab, pos, Quaternion.identity);
        }
    }

    void SetNewWanderTarget() {
        if (manager != null) {
            wanderTarget = manager.GetRandomPointInRoamArea();
        }
    }

    void Wander() {
        Vector3 direction = (wanderTarget - transform.position).normalized;
        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                Quaternion.LookRotation(direction), 
                Time.deltaTime * 5f);
        }
        transform.position = Vector3.MoveTowards(
            transform.position, 
            wanderTarget, 
            Time.deltaTime * 1.0f);
        
        if (Vector3.Distance(transform.position, wanderTarget) < 0.1f) {
            SetNewWanderTarget();
        }
    }

    void CheckWallCollision() {
        if (manager != null && GetComponent<Collider>() != null) {
            if (manager.IsCollidingWithWall(GetComponent<Collider>())) {
                SetNewWanderTarget();
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (!isFallingOver && collision.relativeVelocity.magnitude > 1f && rb != null) {
            StartCoroutine(OrientWhenFalling());
        }
    }

    IEnumerator OrientWhenFalling() {
        isFallingOver = true;
        rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
        
        while (transform.rotation != Quaternion.identity) {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                Quaternion.identity, 
                Time.deltaTime * 720f);
            yield return null;
        }
        
        while (!Physics.Raycast(transform.position, Vector3.down, 0.5f)) {
            yield return null;
        }
        
        isFallingOver = false;
        SetNewWanderTarget();
    }
}
