//this class returns the public variables shadow_points and shadow_distances
//respectively, a list of 2d vectors and list of floats.

using System.Collections.Generic;

using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEditor.SearchService;
using UnityEngine;

public class ray_cast : MonoBehaviour
{      
    private object_manager manager_script;
    private List<MeshFilter> objects;
    private List<Light> lights;
    private MeshFilter wall;
    private List<List<Vector3>> normals;
    private List<List<Vector3>> centers;

    public List<Vector2> shadow_points;
    public List<float> shadow_distances;
    public struct EdgeData
        {
            public int vertexA;
            public int vertexB;
            public int faceIndex1;
            public int faceIndex2;
        }

    private List<EdgeData[]> allObjectEdges = new List<EdgeData[]>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
        {
            manager_script = GetComponent<object_manager>();
            objects = manager_script.objects;
            lights = manager_script.lights;
            wall = manager_script.wall;
            shadow_points = new List<Vector2>();
            shadow_distances = new List<float>();
            foreach (MeshFilter meshfilter in objects)
            {
                if (meshfilter == null || meshfilter.sharedMesh == null)
                    continue;

                Mesh mesh = meshfilter.sharedMesh;
                int[] triangles = mesh.triangles;
                Vector3[] vertices = mesh.vertices;
                int triangleCount = triangles.Length / 3;

                List<Vector3> normals_object = new List<Vector3>();
                List<Vector3> centers_object = new List<Vector3>();


                Dictionary<Vector2Int, List<int>> edgeDictionary = new Dictionary<Vector2Int, List<int>>();

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int faceIndex = i / 3;

                    int idx1 = triangles[i];
                    int idx2 = triangles[i + 1];
                    int idx3 = triangles[i + 2];

                    Vector3 v1 = vertices[idx1];
                    Vector3 v2 = vertices[idx2];
                    Vector3 v3 = vertices[idx3];

                    Vector3 center = (v1 + v2 + v3) / 3f;
                    Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

                    normals_object.Add(normal);
                    centers_object.Add(center);

                    int[,] edges = new int[3, 2] { 
                        { idx1, idx2 }, 
                        { idx2, idx3 }, 
                        { idx3, idx1 } 
                    };

                    for (int e = 0; e < 3; e++)
                    {
                        int a = edges[e, 0];
                        int b = edges[e, 1];

                        Vector2Int edgeKey = new Vector2Int(Mathf.Min(a, b), Mathf.Max(a, b));

                        if (!edgeDictionary.TryGetValue(edgeKey, out List<int> faceList))
                        {
                            faceList = new List<int>();
                            edgeDictionary[edgeKey] = faceList;
                        }
                        faceList.Add(faceIndex);
                    }
                }

                normals.Add(normals_object);
                centers.Add(centers_object);

                EdgeData[] objectEdges = new EdgeData[edgeDictionary.Count];
                int edgeIdx = 0;

                foreach (KeyValuePair<Vector2Int, List<int>> kvp in edgeDictionary)
                {
                    Vector2Int edgeKey = kvp.Key;
                    List<int> adjacentFaces = kvp.Value;

                    int f1 = adjacentFaces[0];
                    
                    int f2 = (adjacentFaces.Count > 1) ? adjacentFaces[1] : f1;

                    objectEdges[edgeIdx] = new EdgeData
                    {
                        vertexA = edgeKey.x,
                        vertexB = edgeKey.y,
                        faceIndex1 = f1,
                        faceIndex2 = f2
                    };

                    edgeIdx++;
                }

                allObjectEdges.Add(objectEdges);
            }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        shadow_points.Clear();
        shadow_distances.Clear();
        foreach (Light light in lights)
        {
            int j=-1;

            float max_dist = (light.transform.position - wall.transform.position).magnitude*5f;
            foreach (MeshFilter mesh in objects)
            {
                j+=1;
                Vector3 light_pos =  mesh.transform.worldToLocalMatrix.MultiplyPoint(light.transform.position);
                
                //loop over each face in the mesh
                List<int> illuminated = new List<int>();
                for (int i=0; i< mesh.mesh.triangles.Length; i = i + 3)
                    {
                        Vector3 light_dir = light_pos - centers[j][i/3];
                        float dotprod = Vector3.Dot(light_dir, normals[j][i/3]);

                        if (dotprod > 0)
                            {
                                illuminated.Add(1);
                            }
                        else
                            {
                                illuminated.Add(0);
                            }

                    }
                
                //loop over every edge to find the silhouette vertices
                int k =0;
                List<Vector3> silhouette = new List<Vector3>();
                List<Vector2> shadow_pts_ob = new List<Vector2>();
                foreach(EdgeData edge in allObjectEdges[j])
                    {
                        k+=1;
                        if (illuminated[edge.faceIndex1] != illuminated[edge.faceIndex2])
                            {
                                silhouette.Add(mesh.transform.localToWorldMatrix.MultiplyPoint(mesh.sharedMesh.vertices[edge.vertexA]));
                                silhouette.Add(mesh.transform.localToWorldMatrix.MultiplyPoint(mesh.sharedMesh.vertices[edge.vertexB]));
                            }
                    }
                
                //removing duplicates
                List<Vector3> uniqueItems = new List<Vector3>(new HashSet<Vector3>(silhouette));

                //actual raycasting
                //wall object must be in layer 6, named "wall"

                foreach (Vector3 vert in uniqueItems){
                    Vector3 direction = -(light.transform.position - vert).normalized;
                    RaycastHit hit;

                    if  (Physics.Raycast(vert, direction, out hit, max_dist, 2^6))

                        {
                            Vector2 local_pt = wall.transform.worldToLocalMatrix.MultiplyPoint(hit.point);
                            shadow_points.Add(local_pt);
                            shadow_distances.Add(hit.distance);

                        }

                    }
            
            }
        
        
       
        }
    }
}
