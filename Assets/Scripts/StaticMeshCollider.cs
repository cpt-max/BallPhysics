using System;
using UnityEngine;

public class StaticMeshCollider : MonoBehaviour
{
    Triangle[] triangles;
    public Bounds bounds;

    void Start()
    {
        PhysicsManager.instance.RegisterStaticMeshCollider(this);

        // get all triangles from the mesh in world space
        var mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null)
        {
            int triCount = mesh.triangles.Length / 3;
            triangles = new Triangle[triCount];

            for (int i = 0; i < triCount; i++)
            {
                triangles[i] = new Triangle(
                    transform.TransformPoint(mesh.vertices[mesh.triangles[i * 3 + 0]]),
                    transform.TransformPoint(mesh.vertices[mesh.triangles[i * 3 + 1]]),
                    transform.TransformPoint(mesh.vertices[mesh.triangles[i * 3 + 2]]));
            }
        }

        // get bounding volume for optimized collision checks
        var renderer = GetComponent<Renderer>();
        if (renderer)
            bounds = renderer.bounds;
    }

    public bool SphereCollision(Vector3 sphereCenter, float sphereRadius, Vector3 vel, out float timeTillColl, out Vector3 collNormal) 
    {
        timeTillColl = -1;
        collNormal = Vector3.zero;

        var sphere = new Sphere();
        sphere.center = sphereCenter;
        sphere.radius = sphereRadius;

        float mindist = float.MaxValue;

        for (int i = 0; i < triangles.Length; i++)
        {
            var tri = triangles[i];

            if (SphereVsTriangle.TestCollision(sphere, vel, tri, out float dist, out Vector3 normal))
            {
                if (dist < mindist)
                {
                    mindist = dist;
                    collNormal = normal;
                }
            }
        }

        if (mindist < float.MaxValue)
        {
            timeTillColl = mindist / vel.magnitude;
            return true;
        }

        return false;
    }
}
