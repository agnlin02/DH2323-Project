using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class photonMapping : MonoBehaviour
{
    LineRenderer lineRenderer;

    public float eta1 = 1.0003f;
    public float eta2 = 1.333f;
    public int texture_resolution = 512;
    public int num_bounces = 1;
    public bool singleRay = true;
    public int num_rays = 1;
    public float light_size = 6.0f;

    LayerMask groundMask;
    LayerMask waterMask;
    private Dictionary<Renderer, Texture2D> _texCache = new();

    private Dictionary<Vector2, int> textureMap = new Dictionary<Vector2, int>();

    private Texture2D groundTexture;


/*     private class Photon()
    {
        public required Vector3 Position { get; init; }
        public required char power { get; init; }
        public required char phi { get; init; }
        public required char theta { get; init; }
        public required short flag { get; init; }
        
    } */

   void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
        waterMask = LayerMask.GetMask("Water");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initTextures();

        if (singleRay) {
            // Create Line
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            // Set the material
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // Set the color
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.green;

            // Set the width
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.2f;

            // Set the number of vertices
            lineRenderer.positionCount = 2+num_bounces;
        }
        else {
            float min = -light_size / 2.0f;
            float max = light_size / 2.0f;
            float diff = max - min;
            float step_size = diff/num_rays;

            Stopwatch stopwatch = Stopwatch.StartNew();
            for(int i = 0; i <= texture_resolution; i++)
            {
                for (int j = 0; j <= texture_resolution; j++) 
                {
                    textureMap.Add(new Vector2(i,j), 0);
                }
            }

            for(float i = min; i < max; i+=step_size)
            {
                for (float j = min; j < max; j+=step_size) 
                {
                    photonRay(transform.position + new Vector3(i, 0, j));
                }
            }
            stopwatch.Stop();
            print("Time for"+ Mathf.Pow(num_rays, 2) + "rays: " + stopwatch.ElapsedMilliseconds);

            paintTexture();
        }
    }

    void initTextures()
    {
        // Set material texture 
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)

        {
            if (((1 << obj.layer) & groundMask.value) == 0) continue;

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null) continue;

            Texture2D tex = new Texture2D(texture_resolution, texture_resolution, TextureFormat.RGBA32, false);

            // Fill black
            Color[] pixels = new Color[texture_resolution * texture_resolution];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.black;

            tex.SetPixels(pixels);
            tex.Apply();

            Material mat = rend.material;
            mat.SetTexture("_BaseMap", tex);
            mat.SetTexture("_MainTex", tex);
            mat.color = Color.white;
            _texCache[rend] = tex;
        }
    }

    void paintTexture()
    {
        foreach (KeyValuePair<Vector2, int> pair in textureMap) 
        {
            if(pair.Value == 0) continue;
            int px = (int)pair.Key.x;
            int py = (int)pair.Key.y;

            float level = 0.1f + 0.4f * pair.Value;
            Color curr_color = groundTexture.GetPixel(px, py);

            Color new_color = new Color(curr_color.r + level, curr_color.g + level, curr_color.b + level);

            groundTexture.SetPixel(px, py, new_color);


    //Lovisas smooth funktion 
      /*       int search_radius = 2;
            float max_search_dist = Mathf.Sqrt(2* Mathf.Pow(search_radius, 2));
            for(int x = px - search_radius; x <= px + search_radius; x++)
            {
                for (int y = py - search_radius; y <= py + search_radius; y++)
                {
                    Vector2 neighbour_position = new Vector2(x, y);
                    float dist = Vector2.Distance(pair.Key, neighbour_position);

                    Color neighbour_color = groundTexture.GetPixel(x, y) + new_color * (max_search_dist - dist);
                    groundTexture.SetPixel(x, y, neighbour_color);
                }
                
            } */
        }
        groundTexture.Apply();
    }

    // Update is called once per frame
    void FixedUpdate()
    { 
        if (singleRay) {
            photonRay(transform.position);    
        }
    }

    

    void photonRay(Vector3 startPosition)
    {
        // Ray from light to surface
        if (! Physics.Raycast(startPosition, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity, waterMask)) return; 
    
        Vector3 normal = hit.normal;
        if (singleRay) {
            // Create a line render from light to water suface
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, hit.point);
        }

        // Calculate end points and direction for the ray
        Vector3 endPoint = hit.point + hit.normal * -2f;
        Vector3 direction = endPoint - hit.point;

        Vector3 e = (hit.point - startPosition).normalized;

        float eta_rel = eta1/eta2;
        float mult = Vector3.Dot(e, normal);

        Vector3 t = normal * (eta_rel* mult - Mathf.Sqrt(1+Mathf.Pow(eta_rel, 2.0f) * (Mathf.Pow(mult, 2.0f) - 1))) + eta_rel*e;

        RaycastHit last_hit = hit;
        Vector3 dir = t;

        for (int bounce = 0; bounce < num_bounces; bounce++) {
            // Ray from surface to ground
            if (! Physics.Raycast(last_hit.point, dir, out RaycastHit curr_hit, Mathf.Infinity, groundMask)) return;

            if (singleRay) {
                lineRenderer.SetPosition(2+bounce, curr_hit.point);
            }

            Renderer rend = curr_hit.transform.GetComponent<Renderer>();
            if (rend == null) return;
            if (!_texCache.TryGetValue(rend, out Texture2D tex))
            {
                print($"{curr_hit.transform.name} not in cache!");
                return;
            }
            if (! groundTexture)
            {
                groundTexture = tex;
            }

            Mesh mesh = curr_hit.collider.GetComponent<MeshFilter>().mesh;

            Vector2 pixelUV = curr_hit.textureCoord;                 
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;

            Vector2 positionTexture = new Vector2((int)pixelUV.x, (int)pixelUV.y);

            int curr_hits = textureMap[positionTexture];
            textureMap[positionTexture] = curr_hits + 1;


            // Reflect the ray at the surface
            Vector3 n = curr_hit.normal.normalized;
            dir = dir - 2 * Vector3.Dot(dir, n) * n;
            last_hit = curr_hit;

        }
    }

}
