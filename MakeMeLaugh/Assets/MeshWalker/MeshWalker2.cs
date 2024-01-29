using System.Collections.Generic;
using UnityEngine;

public class TriangleData
{
    public int faceID;
    public Vector3 v1, v2, v3;
    public int i1, i2, i3;
    public Vector3 normal;
    public List<Vector3> intersectionPoints = new();
    public int forwardIntersectionIndex = -1;
}

public class MeshWalker2 : MonoBehaviour
{
    public LineRenderer playerTriangleLines;
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

    Transform meshTransform;
    Mesh mesh;

    MeshMate meshMate;
    List<Transform> debugPathPoints = new();
    List<TriangleData> triangleDatas = new();

    float sanitizeTolerance = 0.001f;
    SimplePlane currentCuttingPlane = new();

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

            for (int index = 0; index < 50; index++)
            {
                Transform obj = Instantiate(debugBlackSphere, Vector3.zero, Quaternion.identity, pathPointRoot);
                obj.gameObject.SetActive(false);
                debugPathPoints.Add(obj);
            }

            currentCuttingPlane.normal = transform.right;
            currentCuttingPlane.validPoint = transform.position;

            UpdateTriangleData();
        }
    }

    //---------------------------------------------------------------------------
    void UpdateTriangleData()
    {
        // Don't modify this list!
        List<int> localTriangles = meshMate.GetAdjacentFaces(faceID);

        triangleDatas.Clear();
        foreach (int triangle in localTriangles)
        {
            TriangleData tData = meshMate.GetTriangleData(currentCuttingPlane, triangle, meshTransform);
            if (tData.intersectionPoints.Count == 2) triangleDatas.Add(tData);
        }

        TriangleData currentFaceData = meshMate.GetTriangleData(currentCuttingPlane, faceID, meshTransform);
        if (currentFaceData.intersectionPoints.Count == 2) triangleDatas.Add(currentFaceData);

        SanitizeTriangleData(triangleDatas);
        EnsureCurrentTriangleData();

        // Vector3 newNormal = meshMate.GetNormalForFace(faceID, meshTransform);
        Vector3 newForward = GetAndUpdateTriangleForward();

        // THIS DOESN'T WORK, need to debug the setting of character orientation when changing triangles

        // transform.forward = newForward;
        // transform.up = newNormal;
        transform.right = currentCuttingPlane.normal;
        transform.forward = newForward;
        transform.up = Vector3.Cross(transform.forward, transform.right);

        int debugPointIdx = 0;
        foreach (TriangleData data in triangleDatas)
        {
            // Debug.Log("Triangle " + data.faceID + " has " + data.intersectionPoints.Count + " path points");

            foreach (Vector3 point in data.intersectionPoints)
            {
                debugPathPoints[debugPointIdx].name = "Triangle: " + data.faceID;
                debugPathPoints[debugPointIdx].position = point;
                debugPathPoints[debugPointIdx++].gameObject.SetActive(true);
            }
        }

        meshMate.GetTriangleWorldPositions(faceID, meshTransform, out Vector3 v1, out Vector3 v2, out Vector3 v3);
        OutlineTriangle(v1, v2, v3);
    }

    //---------------------------------------------------------------------------
    TriangleData GetCurrentTriangleData(int faceID)
    {
        foreach (TriangleData data in triangleDatas)
        {
            if (data.faceID == faceID)
                return data;
        }

        return null;
    }

    //---------------------------------------------------------------------------
    Vector3 GetAndUpdateTriangleForward()
    {
        TriangleData data = GetCurrentTriangleData(faceID);
        float dist1 = Vector3.Dot(data.intersectionPoints[0] - transform.position, transform.forward);
        float dist2 = Vector3.Dot(data.intersectionPoints[1] - transform.position, transform.forward);
        data.forwardIntersectionIndex = dist1 > dist2 ? 0 : 1;
        return (data.intersectionPoints[data.forwardIntersectionIndex] - data.intersectionPoints[1 - data.forwardIntersectionIndex]).normalized;
    }

    //---------------------------------------------------------------------------
    TriangleData GetNeighborTriangleData(Vector3 startPoint, int excludeFaceID)
    {
        const float tolerance = 0.001f;
        foreach (TriangleData data in triangleDatas)
        {
            if (data.faceID == excludeFaceID) continue;
            if (Vector3.SqrMagnitude(data.intersectionPoints[0] - startPoint) < tolerance ||
                Vector3.SqrMagnitude(data.intersectionPoints[1] - startPoint) < tolerance)
                return data;
        }

        return null;
    }

    //---------------------------------------------------------------------------
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveTowardTarget(true, speed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveTowardTarget(false, speed * Time.deltaTime);
        }
    }

    //---------------------------------------------------------------------------
    void MoveTowardTarget(bool targetFront, float moveAmount)
    {
        TriangleData data = GetCurrentTriangleData(faceID);
        int targetIndex = targetFront ? data.forwardIntersectionIndex : 1 - data.forwardIntersectionIndex;
        Vector3 target = data.intersectionPoints[targetIndex];
        Vector3 fromUsToTarget = (target - transform.position);
        float distanceToTarget = fromUsToTarget.magnitude;

        if (moveAmount > distanceToTarget)
        {
            transform.position = target;
            moveAmount -= distanceToTarget;

            TriangleData neighborData = GetNeighborTriangleData(target, faceID);
            if (neighborData != null)
            {
                faceID = neighborData.faceID;
                UpdateTriangleData();
                // UpdateTriangleForward();
            }
        }
        else
        {
            Vector3 forward = fromUsToTarget.normalized;
            transform.position += forward * 1f * Time.deltaTime;
        }
    }

    //---------------------------------------------------------------------------
    void SanitizeTriangleData(List<TriangleData> triangleDatas)
    {
        int countHack = triangleDatas.Count;
        for (int i = triangleDatas.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < countHack; j++)
            {
                if (i == j) continue;

                // needs to be in inner loop
                if (triangleDatas[i].faceID == faceID) continue;

                TriangleData tData1 = triangleDatas[i];
                TriangleData tData2 = triangleDatas[j];
                if (tData1.intersectionPoints.Count != 2 || tData2.intersectionPoints.Count != 2)
                    continue;

                float diff = Vector3.SqrMagnitude(tData1.intersectionPoints[0] - tData2.intersectionPoints[0]);
                diff += Vector3.SqrMagnitude(tData1.intersectionPoints[1] - tData2.intersectionPoints[1]);

                if (diff < sanitizeTolerance)
                {
                    triangleDatas.RemoveAt(i--);
                    if (i < 0) return;
                    --countHack;
                    continue;
                }

                diff = Vector3.SqrMagnitude(tData1.intersectionPoints[0] - tData2.intersectionPoints[1]);
                diff += Vector3.SqrMagnitude(tData1.intersectionPoints[1] - tData2.intersectionPoints[0]);

                if (diff < sanitizeTolerance)
                {
                    triangleDatas.RemoveAt(i--);
                    if (i < 0) return;
                    --countHack;
                    continue;
                }
            }
        }
    }

    //---------------------------------------------------------------------------
    void EnsureCurrentTriangleData()
    {
        if (GetCurrentTriangleData(faceID) == null)
        {
            TriangleData closestTriangleData = null;
            float closestDistance = float.MaxValue;
            foreach (TriangleData data in triangleDatas)
            {
                float distance = Vector3.SqrMagnitude(data.intersectionPoints[0] - transform.position) +
                    Vector3.SqrMagnitude(data.intersectionPoints[1] - transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTriangleData = data;
                }
            }
            faceID = closestTriangleData.faceID;
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
