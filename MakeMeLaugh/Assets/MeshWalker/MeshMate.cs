using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

public class EdgeTriangles
{
    public int triangleIdx1;
    public int triangleIdx2;
    public IntPair vertIdx1;
    public IntPair vertIdx2;
}

public class MeshMate
{
    Mesh mesh;
    readonly int[] triangles;
    Vector3[] vertices;
    Dictionary<Vector3Pair, EdgeTriangles> edgeAdjaceny = new();
    Dictionary<int, List<int>> triangleNeighbors = new();

    //---------------------------------------------------------------------------
    public MeshMate(Mesh mesh)
    {
        this.mesh = mesh;
        triangles = mesh.triangles;
        vertices = mesh.vertices;

        CalculateEdgeAdjaceny();
        CalculateTriangleAdjacency(vertices, triangles);

        // Vector3 v3 = mesh.vertices[9];
        // Vector3 v4 = mesh.vertices[118];
        // Vector3Pair e1 = new(v3, v4);
        // _AddEdge(e1, 224, 9, 118);

        // get the position for vertex 405
        // Vector3 v1 = mesh.vertices[405];
        // Vector3 v2 = mesh.vertices[404];
        // Vector3Pair e2 = new(v1, v2);
        // _AddEdge(e2, 224, 405, 404);

        // if (edgeAdjaceny.TryGetValue(e2, out EdgeTriangles index))
        // {
        //     Debug.Log($"Edge triangles found: {index.triangleIdx1}, {index.triangleIdx2}");
        //     Debug.Log($"Edge verts found: {index.vertIdx1}, {index.vertIdx2}");
        // }
        // else
        // {
        //     Debug.Log("Edge not found");
        // }
    }

    //---------------------------------------------------------------------------
    public List<int> GetAdjacentFaces(int faceIdx)
    {
        return triangleNeighbors[faceIdx];
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
    public TriangleData GetTriangleData(SimplePlane plane, int faceIdx, Transform MeshTransform)
    {
        int startIdx = faceIdx * 3;
        int vertIdx1 = triangles[startIdx + 0];
        int vertIdx2 = triangles[startIdx + 1];
        int vertIdx3 = triangles[startIdx + 2];

        Vector3 v1 = MeshTransform.TransformPoint(vertices[vertIdx1]);
        Vector3 v2 = MeshTransform.TransformPoint(vertices[vertIdx2]);
        Vector3 v3 = MeshTransform.TransformPoint(vertices[vertIdx3]);

        Vector3 side1 = v2 - v1;
        Vector3 side2 = v3 - v1;
        Vector3 normal = Vector3.Cross(side1, side2).normalized;

        TriangleData triangleData = new()
        {
            normal = normal,
            v1 = v1,
            v2 = v2,
            v3 = v3,
            i1 = vertIdx1,
            i2 = vertIdx2,
            i3 = vertIdx3,
            faceID = faceIdx
        };

        bool result1, result2, result3;
        Vector3 point1, point2, point3;
        (result1, point1) = GetLinePlaneIntersection(plane, v1, v2);
        (result2, point2) = GetLinePlaneIntersection(plane, v2, v3);
        (result3, point3) = GetLinePlaneIntersection(plane, v3, v1);

        if ((point2 - point1).sqrMagnitude < 0.05f) result2 = false;
        if ((point3 - point1).sqrMagnitude < 0.05f) result3 = false;
        if ((point2 - point3).sqrMagnitude < 0.05f) result2 = false;

        if (result1) triangleData.intersectionPoints.Add(point1);
        if (result2) triangleData.intersectionPoints.Add(point2);
        if (result3) triangleData.intersectionPoints.Add(point3);

        return triangleData;
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
        const float tolerance = 1e-5f;
        float numerator = Vector3.Dot(plane.normal, plane.validPoint) - Vector3.Dot(plane.normal, p1);
        float denominator = Vector3.Dot(plane.normal, p2 - p1);

        if (Mathf.Approximately(denominator, 0f)) return (false, Vector3.zero);
        float alpha = numerator / denominator;
        if (alpha < -tolerance || alpha > 1f + tolerance) return (false, Vector3.zero);

        return (true, p1 + alpha * (p2 - p1));
    }

    //---------------------------------------------------------------------------
    public bool GetTriangleEdgeNeighbor(EdgeIndexPair edgeIndices, int currentFaceIdx, out int faceIdx)
    {
        Vector3 v1 = vertices[edgeIndices.index1];
        Vector3 v2 = vertices[edgeIndices.index2];

        if (edgeAdjaceny.TryGetValue(new Vector3Pair(v1, v2), out EdgeTriangles edgeTriangles))
        {
            if (edgeTriangles.triangleIdx1 == currentFaceIdx)
            {
                faceIdx = edgeTriangles.triangleIdx2;
                return true;
            }

            if (edgeTriangles.triangleIdx2 == currentFaceIdx)
            {
                faceIdx = edgeTriangles.triangleIdx1;
                return true;
            }
        }

        faceIdx = -1;
        return false;
    }

    //---------------------------------------------------------------------------
    public Vector3 GetNormalForFace(int faceIdx, Transform meshTransform)
    {
        int startIdx = faceIdx * 3;
        int vertIdx1 = triangles[startIdx + 0];
        int vertIdx2 = triangles[startIdx + 1];
        int vertIdx3 = triangles[startIdx + 2];

        Vector3 v1 = meshTransform.TransformPoint(vertices[vertIdx1]);
        Vector3 v2 = meshTransform.TransformPoint(vertices[vertIdx2]);
        Vector3 v3 = meshTransform.TransformPoint(vertices[vertIdx3]);

        // calculate normal
        Vector3 side1 = v2 - v1;
        Vector3 side2 = v3 - v1;
        Vector3 normal = Vector3.Cross(side1, side2).normalized;
        return normal;
    }

    //---------------------------------------------------------------------------
    void CalculateTriangleAdjacency(Vector3[] vertices, int[] triangles)
    {
        int triangleCount = triangles.Length / 3;
        triangleNeighbors = new Dictionary<int, List<int>>();
        float epsilon = 0.0001f; // Adjust this value as needed for your precision requirements

        // Initialize all triangle indices in the dictionary
        for (int i = 0; i < triangleCount; i++)
        {
            triangleNeighbors[i] = new List<int>();
        }

        // Function to compare vertex positions considering precision
        bool AreVerticesClose(Vector3 vert1, Vector3 vert2)
        {
            return Vector3.Distance(vert1, vert2) < epsilon;
        }

        // Iterate through each triangle
        for (int i = 0; i < triangleCount; i++)
        {
            // Get vertices of the current triangle
            Vector3 vert1 = vertices[triangles[i * 3]];
            Vector3 vert2 = vertices[triangles[i * 3 + 1]];
            Vector3 vert3 = vertices[triangles[i * 3 + 2]];

            // Check against all other triangles
            for (int j = 0; j < triangleCount; j++)
            {
                if (i == j) continue; // Skip the same triangle

                // Get vertices of the triangle to compare
                Vector3 compareVert1 = vertices[triangles[j * 3]];
                Vector3 compareVert2 = vertices[triangles[j * 3 + 1]];
                Vector3 compareVert3 = vertices[triangles[j * 3 + 2]];

                // Check if they share at least one vertex position
                if (AreVerticesClose(vert1, compareVert1) || AreVerticesClose(vert1, compareVert2) || AreVerticesClose(vert1, compareVert3) ||
                    AreVerticesClose(vert2, compareVert1) || AreVerticesClose(vert2, compareVert2) || AreVerticesClose(vert2, compareVert3) ||
                    AreVerticesClose(vert3, compareVert1) || AreVerticesClose(vert3, compareVert2) || AreVerticesClose(vert3, compareVert3))
                {
                    // Add j to the neighbors of i if not already present
                    if (!triangleNeighbors[i].Contains(j))
                    {
                        triangleNeighbors[i].Add(j);
                    }
                }
            }
        }
    }




    //---------------------------------------------------------------------------
    void CalculateEdgeAdjaceny()
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int vertIdx1 = triangles[i];
            int vertIdx2 = triangles[i + 1];
            int vertIdx3 = triangles[i + 2];

            Vector3Pair edge1 = new(vertices[vertIdx1], vertices[vertIdx2]);
            Vector3Pair edge2 = new(vertices[vertIdx2], vertices[vertIdx3]);
            Vector3Pair edge3 = new(vertices[vertIdx3], vertices[vertIdx1]);

            _AddEdge(edge1, i / 3, vertIdx1, vertIdx2);
            _AddEdge(edge2, i / 3, vertIdx2, vertIdx3);
            _AddEdge(edge3, i / 3, vertIdx3, vertIdx1);
        }

        void _AddEdge(Vector3Pair edge, int triangleIdx, int v1, int v2)
        {
            if (edgeAdjaceny.TryGetValue(edge, out EdgeTriangles index))
            {
                index.triangleIdx2 = triangleIdx;
                index.vertIdx1 = new IntPair(v1, v2);
            }
            else
            {
                edgeAdjaceny.Add(edge, new EdgeTriangles
                {
                    triangleIdx1 = triangleIdx,
                    triangleIdx2 = -1,
                    vertIdx1 = new IntPair(v1, v2),
                    vertIdx2 = null
                });
            }
        }
    }
}
