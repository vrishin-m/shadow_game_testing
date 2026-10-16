using System.Collections.Generic;
using UnityEngine;

public class convex_hull : MonoBehaviour
{   
    private List<List<Vector2>> clusters;
    private clustering clustering;
    public List<List<Vector2>> hulls;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clustering = GetComponent<clustering>();
        hulls = new List<List<Vector2>>();
        
    }

    // Update is called once per frame
    void Update()
    {
        clusters = clustering.clusters;
        hulls.Clear();

        foreach (List<Vector2> cluster in clusters)
        {
            if (cluster == null || cluster.Count < 3)
                continue;

            List<Vector2> pts = new List<Vector2>(cluster);
            Vector2 anchor = pts[0];
            for (int i = 1; i < pts.Count; i++)
            {
                if (pts[i].y < anchor.y)
                {
                    anchor = pts[i];
                }
            }

            // sort by polar angle using Atan2
            pts.Remove(anchor);
            pts.Sort((a, b) =>
            {
                float angleA = Mathf.Atan2(a.y - anchor.y, a.x - anchor.x);
                float angleB = Mathf.Atan2(b.y - anchor.y, b.x - anchor.x);
                return angleA.CompareTo(angleB);
            });
            
            List<Vector2> stack = new List<Vector2>();
            stack.Add(anchor);
            stack.Add(pts[0]);

            for (int j = 1; j < pts.Count; j++)
            {
               
                while (stack.Count >= 2 && anticw(stack[stack.Count - 2], stack[stack.Count - 1], pts[j]) != 1)
                {
                    stack.RemoveAt(stack.Count - 1);
                }
                stack.Add(pts[j]);
            }

            stack.Add(anchor);
            hulls.Add(stack);
        }
    }

    int anticw(Vector2 a, Vector2 b, Vector2 c)
    {
        float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        return cross > 0 ? 1 : 0;
    }
}
