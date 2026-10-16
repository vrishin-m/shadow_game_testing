using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;






public class clustering : MonoBehaviour
{

    struct points
        {
            public int index;
            public List<int> neighbours;
        }
    
    private ray_cast raycast;
    public float max_dist = 5f;
    public int min_neighbors = 3;

    private List<Vector2> shadow_pts;

    
    List<points> distances = new List<points>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        raycast = GetComponent<ray_cast>();
        shadow_pts = new List<Vector2>();
        
    }




    // Update is called once per frame
    void FixedUpdate()
    {
        shadow_pts = raycast.shadow_points;
        int count = shadow_pts.Count;
        find_distances(0, count);
        List<int> visited = new List<int>();

        foreach (points pt in distances)
            {
                
            }
    }

    void find_distances(int start_idx, int count)
        {
            if (start_idx< count){
                points point_struct = new points
                {
                    index = start_idx,
                    neighbours = new List<int>()
                };
                for (int i=start_idx; i<count; i++)
                    {
                            if ((shadow_pts[start_idx]-shadow_pts[i]).magnitude < max_dist){
                                point_struct.neighbours.Add(i);

                            }
                    }

                distances.Add(point_struct);
                find_distances(start_idx+1, count);

            }

        }
}
