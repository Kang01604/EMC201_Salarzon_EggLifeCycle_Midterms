using UnityEngine;
using System.Collections;

public class Rooster : MonoBehaviour {
    private SimulationManager manager;
    private Vector3 spawnPosition;
    private Vector3 wanderTarget;
    private Rigidbody rb;
    private bool isFallingOver = false;
    private float perishTime = 40f;
    private float indicatorTime = 1f;
    private float jumpForce = 7f;
    private float elapsedTime = 0f;
    private Renderer rend;

    void Start() {
        manager = Object.FindAnyObjectByType<SimulationManager>();
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        if (manager != null) {
            manager.AddRooster();
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
        
        if (elapsedTime >= perishTime - indicatorTime && rend != null) {
            rend.material.SetColor("_EmissionColor", Color.red);
        }
        
        if (elapsedTime >= perishTime) {
            if (manager != null) {
                manager.RemoveRooster();
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
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
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
