using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public struct SimplePlane
{
    public Vector3 normal;
    public Vector3 validPoint;
}

public struct IntersectionData
{
    public bool result;
    public Vector3 point;
    public EdgeIndexPair EdgeIndexPair;
    public Vector3 edgeStartPos;
    public Vector3 edgeEndPos;
}

public class MeshMate
{
    Mesh mesh;
    readonly int[] triangles;
    Vector3[] vertices;

    //---------------------------------------------------------------------------
    public MeshMate(Mesh mesh)
    {
        this.mesh = mesh;
        triangles = mesh.triangles;
        vertices = mesh.vertices;
    }

    //---------------------------------------------------------------------------
    public void GetTriangleWorldPositions(int triangleIdx, Transform meshTransform, out Vector3 v1, out Vector3 v2, out Vector3 v3)
    {
        int startIdx = triangleIdx * 3;
        int vertIdx1 = triangles[startIdx + 0];
        int vertIdx2 = triangles[startIdx + 1];
        int vertIdx3 = triangles[startIdx + 2];

        v1 = meshTransform.TransformPoint(vertices[vertIdx1]);
        v2 = meshTransform.TransformPoint(vertices[vertIdx2]);
        v3 = meshTransform.TransformPoint(vertices[vertIdx3]);
    }

    //---------------------------------------------------------------------------
    public void GetIntersections(SimplePlane plane, int triangleIdx, Transform meshTransform, List<IntersectionData> intersectionList)
    {
        int startIdx = triangleIdx * 3;
        int vertIdx1 = triangles[startIdx + 0];
        int vertIdx2 = triangles[startIdx + 1];
        int vertIdx3 = triangles[startIdx + 2];

        Vector3 v1 = meshTransform.TransformPoint(vertices[vertIdx1]);
        Vector3 v2 = meshTransform.TransformPoint(vertices[vertIdx2]);
        Vector3 v3 = meshTransform.TransformPoint(vertices[vertIdx3]);

        bool result;
        Vector3 point;
        (result, point) = GetLinePlaneIntersection(plane, v1, v2);
        if (result) _AddIntersection(point, new EdgeIndexPair(vertIdx1, vertIdx2), v1, v2);
        (result, point) = GetLinePlaneIntersection(plane, v2, v3);
        if (result) _AddIntersection(point, new EdgeIndexPair(vertIdx2, vertIdx3), v2, v3);
        (result, point) = GetLinePlaneIntersection(plane, v3, v1);
        if (result) _AddIntersection(point, new EdgeIndexPair(vertIdx3, vertIdx1), v3, v1);

        void _AddIntersection(Vector3 intersectionPoint, EdgeIndexPair edgeIndexPair, Vector3 edgeStartPos, Vector3 edgeEndPos)
        {
            intersectionList.Add(new IntersectionData
            {
                result = true,
                point = intersectionPoint,
                EdgeIndexPair = edgeIndexPair,
                edgeStartPos = edgeStartPos,
                edgeEndPos = edgeEndPos
            });
        }
    }

    //---------------------------------------------------------------------------
    (bool, Vector3) GetLinePlaneIntersection(SimplePlane plane, Vector3 p1, Vector3 p2)
    {
        float numerator = Vector3.Dot(plane.normal, plane.validPoint) - Vector3.Dot(plane.normal, p1);
        float denominator = Vector3.Dot(plane.normal, p2 - p1);

        if (Mathf.Approximately(denominator, 0f)) return (false, Vector3.zero);
        float alpha = numerator / denominator;
        if (alpha < 0f || alpha > 1f) return (false, Vector3.zero);

        return (true, p1 + alpha * (p2 - p1));
    }

    //---------------------------------------------------------------------------
    public bool GetTriangleEdgeNeighbor(EdgeIndexPair edgeIndices, int currentFaceIdx, out int faceIdx)
    {
        // int startIdx = triangles[edgeIndices.index1];
        // int endIdx = triangles[edgeIndices.index2];
        int startIdx = edgeIndices.index1;
        int endIdx = edgeIndices.index2;

        int triIndex = -1;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (i == (currentFaceIdx * 3)) continue;
            if (triangles[i] == startIdx && triangles[i + 1] == endIdx)
            {
                triIndex = i;
                break;
            }

            if (triangles[i] == endIdx && triangles[i + 1] == startIdx)
            {
                triIndex = i;
                break;
            }

            if (triangles[i + 1] == startIdx && triangles[i + 2] == endIdx)
            {
                triIndex = i;
                break;
            }

            if (triangles[i + 1] == endIdx && triangles[i + 2] == startIdx)
            {
                triIndex = i;
                break;
            }

            if (triangles[i + 2] == startIdx && triangles[i] == endIdx)
            {
                triIndex = i;
                break;
            }

            if (triangles[i + 2] == endIdx && triangles[i] == startIdx)
            {
                triIndex = i;
                break;
            }
        }

        faceIdx = triIndex / 3;
        return (triIndex != -1);
    }

    //---------------------------------------------------------------------------
    // void GetNeighborTriangle(int triangleIndex, out int neighborIndex1, out int neighborIndex2, out int neighborIndex3)
    // {
        // neighborIndex1 = -1;
        // neighborIndex2 = -1;
        // neighborIndex3 = -1;
        //
        // int neighborTriangleIndex = -1;
        // for (int i = 0; i < triangles.Length; i += 3)
        // {
        //     if (i == triangleIndex) continue;
        //
        //     if (triangles[i] == index1 || triangles[i] == index2 || triangles[i] == index3)
        //     {
        //         neighborTriangleIndex = i;
        //         break;
        //     }
        // }
        //
        // if (neighborTriangleIndex == -1) return;
        //
        // neighborIndex1 = triangles[neighborTriangleIndex];
        // neighborIndex2 = triangles[neighborTriangleIndex + 1];
        // neighborIndex3 = triangles[neighborTriangleIndex + 2];
    // }
}
