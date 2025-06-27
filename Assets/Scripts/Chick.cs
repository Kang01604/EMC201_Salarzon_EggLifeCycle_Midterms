using UnityEngine;
using System.Collections;

public class Chick : MonoBehaviour {
    private static bool firstChickCreated = false;
    private float matureTime = 10f;
    private SimulationManager manager;
    private Vector3 spawnPosition;
    private Vector3 wanderTarget;
    private Rigidbody rb;
    private bool isFallingOver = false;
    private float indicatorTime = 1f;
    private float elapsedTime = 0f;
    private Renderer rend;

    void Start() {
        manager = Object.FindAnyObjectByType<SimulationManager>();
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        spawnPosition = transform.position;
        SetNewWanderTarget();
        manager.AddChick();
    }

    void FixedUpdate() {
        if (!isFallingOver) {
            Wander();
            CheckWallCollision();
        }

        elapsedTime += Time.deltaTime * manager.SpeedMultiplier;
        
        if (elapsedTime >= matureTime - indicatorTime && rend != null) {
            rend.material.SetColor("_EmissionColor", Color.white);
        }
        
        if (elapsedTime >= matureTime) {
            if (manager != null) {
                manager.RemoveChick();
                GameObject nextGen = !firstChickCreated ? 
                    Instantiate(manager.henPrefab, transform.position, Quaternion.identity) :
                    Instantiate(Random.value < 0.5f ? manager.henPrefab : manager.roosterPrefab, transform.position, Quaternion.identity);
                
                firstChickCreated = true;
            }
            Destroy(gameObject);
        }
    }

    void SetNewWanderTarget() {
        if (manager != null) {
            wanderTarget = manager.GetRandomPointInRoamArea();
        }
    }

    void Wander() {
        if (isFallingOver) return;
        
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
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        
        while (transform.rotation != Quaternion.identity) {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                Quaternion.identity, 
                Time.deltaTime * 360f);
            yield return null;
        }
        
        while (!Physics.Raycast(transform.position, Vector3.down, 0.2f)) {
            yield return null;
        }
        
        isFallingOver = false;
        SetNewWanderTarget();
    }
}
