using System;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    public Vector3 position { get => transform.position; set { transform.position = value; } }
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public float radius = 0.5f;
    public float mass = 1;

    void Start()
    {
        PhysicsManager.instance.RegisterBody(this);
    }
}
