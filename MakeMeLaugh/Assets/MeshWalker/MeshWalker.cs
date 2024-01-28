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
    public Transform pathPointRoot;
    public Transform debugFrontSphere;
    public Transform debugBackSphere;
    public Transform debugBlackSphere;
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
    HashSet<Vector3> currentPathPoints = new(new Vector3EqualityComparer(0.1f));
    List<Transform> debugPathPoints = new();

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

            for (int index = 0; index < 32; index++)
            {
                Transform obj = Instantiate(debugBlackSphere, Vector3.zero, Quaternion.identity, pathPointRoot);
                obj.gameObject.SetActive(false);
                debugPathPoints.Add(obj);
            }

            UpdatePathPoints();
        }
    }

    //---------------------------------------------------------------------------
    void UpdatePathPoints()
    {
        List<int> localTriangles = meshMate.GetAdjacentFaces(faceID);
        localTriangles.Add(faceID);
        // Debug.Log(string.Join(", ", neighborTriangles));

        SimplePlane plane = new();
        plane.normal = transform.right;
        plane.validPoint = transform.position;
        planeIntersections.Clear();
        currentPathPoints.Clear();

        foreach (int neighborTriangle in localTriangles)
        {
            meshMate.GetIntersections(plane, neighborTriangle, meshTransform, planeIntersections);
            if (planeIntersections.Count > 0)
            {
                // Debug.Log("intersecting " + neighborTriangle);
                currentPathPoints.Add(planeIntersections[0].point);
                currentPathPoints.Add(planeIntersections[1].point);
            }

            planeIntersections.Clear();
        }

        int index = 0;
        foreach (Vector3 point in currentPathPoints)
        {
            debugPathPoints[index].gameObject.SetActive(true);
            debugPathPoints[index].position = point;
            index++;
        }

        for (int i = currentPathPoints.Count; i < debugPathPoints.Count; i++)
        {
            debugPathPoints[i].gameObject.SetActive(false);
        }
    }

    //---------------------------------------------------------------------------
    void UpdateCurrentTriangle(int faceID, Vector3 currentPos)
    {
        this.faceID = faceID;

        Debug.Log("Entered face " + faceID);

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
            debugFrontSphere.position = planeIntersections[frontIndex].point;
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
            MoveTowardTarget(true, speed * Time.deltaTime, frontEdgeIndexPair);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveTowardTarget(false, speed * Time.deltaTime, backEdgeIndexPair);
        }
    }

    //---------------------------------------------------------------------------
    (bool, Vector3) GetClosestTarget(bool targetFront)
    {
        float closestDist = float.MaxValue;
        Vector3 closestTarget = Vector3.zero;
        Vector3 targetDir = targetFront ? transform.forward : -transform.forward;

        foreach (Vector3 pathPoint in currentPathPoints)
        {
            float dist = Vector3.Dot(pathPoint - transform.position, targetDir);
            if (dist > 0 && dist < closestDist)
            {
                closestDist = dist;
                closestTarget = pathPoint;
            }
        }
        return ((closestDist != float.MaxValue), closestTarget);
    }

    //---------------------------------------------------------------------------
    void MoveTowardTarget(bool targetFront, float moveAmount, EdgeIndexPair edgeIndexPair)
    {
        (bool result, Vector3 target) = GetClosestTarget(targetFront);
        if (!result)
        {
            UpdatePathPoints();
            Debug.LogWarning("missing1");
            return;
        }
        Vector3 fromUsToTarget = (target - transform.position);
        float distanceToTarget = fromUsToTarget.magnitude;

        if (moveAmount > distanceToTarget)
        {
            transform.position = target;
            moveAmount -= distanceToTarget;

            UpdatePathPoints();

            (bool result2, Vector3 nextTarget) = GetClosestTarget(targetFront);
            if (!result2)
            {
                Debug.LogWarning("wtf");
                return;
            }

            transform.forward = (nextTarget - transform.position).normalized;

            if (Physics.Raycast(transform.position + transform.up * 0.5f, -transform.up, out RaycastHit hit))
            {
                // Vector3 right = transform.right;
                // transform.up = hit.normal;
                // transform.forward = Vector3.Cross(transform.right, hit.normal);
                // transform.right = right;
                if (hit.triangleIndex != -1)
                {
                    UpdateCurrentTriangle(hit.triangleIndex, hit.point);
                }
                else
                {
                    Debug.Break();
                }
            }

            // if (meshMate.GetTriangleEdgeNeighbor(edgeIndexPair, faceID, out int newFaceID))
            // {
            //     Vector3 newFaceNormal = meshMate.GetNormalForFace(newFaceID, meshTransform);
            //     transform.up = newFaceNormal;
            //     UpdateCurrentTriangle(newFaceID, transform.position);
            //
            //     // Recursive until done
            //     EdgeIndexPair newEdgeIndexPair = targetFront ? frontEdgeIndexPair : backEdgeIndexPair;
            //     MoveTowardTarget(targetFront, moveAmount, newEdgeIndexPair);
            // }
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
