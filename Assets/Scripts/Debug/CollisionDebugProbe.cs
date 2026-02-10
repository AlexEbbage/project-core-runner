using System.Collections;
using UnityEngine;

public class CollisionDebugProbe : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"[CollisionDebugProbe] OnCollisionEnter with {other.collider.name} (layer {other.gameObject.layer})");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CollisionDebugProbe] OnTriggerEnter with {other.name} (layer {other.gameObject.layer})");
    }
}