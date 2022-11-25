using System;
using UnityEngine;

public class StaticMeshCollider : MonoBehaviour
{
    Triangle[] triangles;
    public Bounds bounds;

    [Range(0, 1)] public float elasticity = 0;
    [Range(0, 1)] public float surfaceFriction = 1;

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

    public bool SphereCollision(Vector3 sphereCenter, float sphereRadius, Vector3 vel, float dtDist, out float timeTillColl, out Vector3 contactPoint, out Vector3 collNormal)
    {
        timeTillColl = -1;
        collNormal = Vector3.zero;
        contactPoint = Vector3.zero;

        // early out if we are well outside the bounding volume
        float maxMoveRadius = sphereRadius + dtDist;
        if (bounds != null && bounds.SqrDistance(sphereCenter) > maxMoveRadius * maxMoveRadius + 0.01f)
            return false;

        // find closest colliding triangle
        float mindist = float.MaxValue;
        for (int i = 0; i < triangles.Length; i++)
        {
            var tri = triangles[i];
            if (SphereVsTriangle.TestCollision(sphereCenter, sphereRadius, vel, tri, out float dist, out Vector3 normal))
            {
                if (dist < mindist)
                {
                    mindist = dist;
                    collNormal = normal;
                    contactPoint = sphereCenter - normal * (dist + sphereRadius);
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
