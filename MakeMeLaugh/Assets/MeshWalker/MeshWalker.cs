using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct EdgeIndexPair
{
    public int index1;
    public int index2;

    public EdgeIndexPair(int index1, int index2)
    {
        this.index1 = index1;
        this.index2 = index2;
    }
}

public class MeshWalker : MonoBehaviour
{
    public LineRenderer playerTriangleLines;
    public LineRenderer playerTriangleNeighborLines;
    public Transform debugFrontSphere;
    public Transform debugBackSphere;
    public float speed = 1f;

    [Header("Debug")]
    public int faceID = -1;
    public Vector3 baryPos = Vector3.zero;
    public Vector3 baryFrontTarget = Vector3.zero;
    public Vector3 baryBackTarget = Vector3.zero;
    public Vector3 worldFrontTarget = Vector3.zero;
    public Vector3 worldBackTarget = Vector3.zero;
    public EdgeIndexPair frontEdgeIndexPair = new(-1, -1);
    public EdgeIndexPair backEdgeIndexPair = new(-1, -1);

    public int neighborIndex1;
    public int neighborIndex2;
    public int neighborIndex3;

    Transform meshTransform;
    Mesh mesh;
    int[] triangles;
    Vector3[] vertices;
    List<IntersectionData> planeIntersections = new();

    MeshMate meshMate;

    //---------------------------------------------------------------------------
    void Start()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return;

            transform.position = hit.point;
            transform.up = hit.normal;

            baryPos = hit.barycentricCoordinate;
            faceID = hit.triangleIndex;

            mesh = meshCollider.sharedMesh;
            meshTransform = meshCollider.transform;
            meshMate = new MeshMate(mesh);
            triangles = mesh.triangles;
            vertices = mesh.vertices;

            UpdateCurrentTriangle(faceID, hit.point);
        }
    }

    //---------------------------------------------------------------------------
    void UpdateCurrentTriangle(int faceID, Vector3 currentPos)
    {
        this.faceID = faceID;

        SimplePlane plane = new();
        plane.normal = transform.right;
        plane.validPoint = currentPos;
        planeIntersections.Clear();
        meshMate.GetIntersections(plane, faceID, meshTransform, planeIntersections);

        if (planeIntersections.Count == 2)
        {
           float dist1 = Vector3.Dot(planeIntersections[0].point - currentPos, transform.forward);
           float dist2 = Vector3.Dot(planeIntersections[1].point - currentPos, transform.forward);

           int frontIndex = dist1 > dist2 ? 0 : 1;
           int backIndex = 1 - frontIndex;
           worldFrontTarget = planeIntersections[frontIndex].point;
           debugFrontSphere.position =  planeIntersections[frontIndex].point;
           worldBackTarget = planeIntersections[backIndex].point;
           debugBackSphere.position = planeIntersections[backIndex].point;
           frontEdgeIndexPair.index1 = planeIntersections[frontIndex].EdgeIndexPair.index1;
           frontEdgeIndexPair.index2 = planeIntersections[frontIndex].EdgeIndexPair.index2;
           backEdgeIndexPair.index1 = planeIntersections[backIndex].EdgeIndexPair.index1;
           backEdgeIndexPair.index2 = planeIntersections[backIndex].EdgeIndexPair.index2;
        }

        // baryFrontTarget = WorldToBary(worldFrontTarget, v1, v2, v3);
        // baryBackTarget = WorldToBary(worldBackTarget, v1, v2, v3);

        meshMate.GetTriangleWorldPositions(faceID, meshTransform, out Vector3 v1, out Vector3 v2, out Vector3 v3);
        OutlineTriangle(v1, v2, v3);
    }

    //---------------------------------------------------------------------------
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveTowardTarget(worldFrontTarget, frontEdgeIndexPair);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveTowardTarget(worldBackTarget, backEdgeIndexPair);
        }
    }

    //---------------------------------------------------------------------------
    void MoveTowardTarget(Vector3 target, EdgeIndexPair edgeIndexPair)
    {
        Vector3 fromUsToTarget = (target - transform.position);
        float distanceToTarget = fromUsToTarget.magnitude;
        float distanceToMove = speed * Time.deltaTime;

        if (distanceToMove > distanceToTarget)
        {
            transform.position = target;
            distanceToMove -= distanceToTarget;

            if (meshMate.GetTriangleEdgeNeighbor(edgeIndexPair, faceID, out int newFaceID))
                UpdateCurrentTriangle(newFaceID, transform.position);
        }
        else
        {
            Vector3 forward = fromUsToTarget.normalized;
            transform.position += forward * 1f * Time.deltaTime;
        }
    }

    //---------------------------------------------------------------------------
    void OutlineTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        playerTriangleLines.positionCount = 4;
        playerTriangleLines.SetPosition(0, v1);
        playerTriangleLines.SetPosition(1, v2);
        playerTriangleLines.SetPosition(2, v3);
        playerTriangleLines.SetPosition(3, v1);
    }

    //---------------------------------------------------------------------------
    public static Vector3 WorldToBary(Vector3 worldPoint, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a, v1 = c - a, v2 = worldPoint - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;

        Vector3 barycentricCoords = new();
        barycentricCoords.y = (d11 * d20 - d01 * d21) / denom;
        barycentricCoords.z = (d00 * d21 - d01 * d20) / denom;
        barycentricCoords.x = 1.0f - barycentricCoords.y - barycentricCoords.z;

        return barycentricCoords;
    }
}
