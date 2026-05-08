using UnityEngine;
using System;
using System.Collections.Generic;

public class photonMapping : MonoBehaviour
{
    LineRenderer lineRenderer;

    public float eta1 = 1.0003f;
    public float eta2 = 1.333f;
    public int num_rays = 100;
    public int resolution = 512;

    LayerMask groundMask;
    LayerMask waterMask;
    private Dictionary<Renderer, Texture2D> _texCache = new();

   void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
        waterMask = LayerMask.GetMask("Water");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       /*  // Create Line
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
        lineRenderer.positionCount = 3; */
        
        initTextures();

        float min = -5.0f;
        float max = 5.0f;
        float diff = max - min;
        float step_size = diff/num_rays;
        for(float i = min; i < max; i+=step_size)
        {
            for (float j = min; j < max; j+=step_size) 
            {
                photonRay(new Vector3(i, 16.0f, j));
            }
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

            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

            // Fill black
            Color[] pixels = new Color[resolution * resolution];
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

    // Update is called once per frame
    void FixedUpdate()
    { 
      
    }

    

    void photonRay(Vector3 startPosition)
    {
        // Ray from light to surface
        if (! Physics.Raycast(startPosition, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity, waterMask)) return; 
    
        Vector3 normal = hit.normal;
        // Create a line render from light to water suface
      /*   lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, hit.point); */

        // Calculate end points and direction for the ray
        Vector3 endPoint = hit.point + hit.normal * -2f;
        Vector3 direction = endPoint - hit.point;

        Vector3 e = (hit.point - startPosition).normalized;

        float eta_rel = eta1/eta2;
        float mult = Vector3.Dot(e, normal);

        Vector3 t = normal * (eta_rel* mult - Mathf.Sqrt(1+Mathf.Pow(eta_rel, 2.0f) * (Mathf.Pow(mult, 2.0f) - 1))) + eta_rel*e;
        
        /*   print("normal"+normal); 
        print("E: "+ e);
        print("t"+ t);  */     
        /* Console.WriteLine("normal ", normal);
        Console.WriteLine("E: ", e);
        Console.WriteLine("t", t);
        */

        // Ray from surface to ground
        if (! Physics.Raycast(hit.point, t, out RaycastHit hit2, Mathf.Infinity, groundMask)) return;

        MeshCollider mc = hit2.collider as MeshCollider;
        Mesh m = mc?.sharedMesh;

        // print($"textureCoord: {hit2.textureCoord}");
        // print($"isReadable: {m?.isReadable}");
        // print($"uv.Length: {m?.uv.Length}");
        // print($"triangleIndex: {hit2.triangleIndex}");
        // print($"point: {hit2.point}");



        //lineRenderer.SetPosition(2, hit2.point);
        //Color GetPixelBilinear(float u, float v, int mipLevel = 0);

        Renderer rend = hit2.transform.GetComponent<Renderer>();
        if (rend == null) return;
        if (!_texCache.TryGetValue(rend, out Texture2D tex))
        {
            Debug.LogWarning($"{hit2.transform.name} not in cache!");
            return;
        }

        PaintHit(hit2, tex);

        // Renderer rend = hit2.transform.GetComponent<Renderer>();     // Get hit component
        // MeshCollider meshCollider = hit2.collider as MeshCollider;
        // Texture2D tex = rend.material.GetTexture("_BaseMap") as Texture2D;     // Get component material texture

        // if (rend == null || rend.material == null || tex == null || meshCollider == null) {
        //     print("rend " + (rend == null));
        //     print("rend.material " + (rend.material == null));
        //     print("rend.material.Maintexture " + (tex == null));
        //     print("meshcollider "+(meshCollider == null));
        //     return;
        // }

        // Mesh mesh = hit2.collider.GetComponent<MeshFilter>().mesh;

        // print(mesh.uv.Length);

        // Vector2 pixelUV = hit2.textureCoord;                 
        // pixelUV.x *= tex.width;
        // pixelUV.y *= tex.height;

        // tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.white);
        // tex.Apply();

        // print("PixelUV.x: " + pixelUV.x);
        // print("PixelUV.y: " + pixelUV.y);

        // create sphere
        /* GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        dot.transform.position = hit2.point;
        dot.transform.localScale = Vector3.one * 0.1f;

        Renderer renderer = dot.GetComponent<Renderer>();
        renderer.material.color = Color.white;
        */
        
    
    }


    void PaintHit(RaycastHit hit2, Texture2D tex)
    {
        Bounds bounds = hit2.collider.bounds;
        Vector3 localPoint = hit2.point - bounds.min;
        Vector3 size = bounds.size;
        Vector3 normal = hit2.normal;

        float u, v;
        if (Mathf.Abs(normal.y) > 0.5f)
        {
            u = localPoint.x / size.x;
            v = localPoint.z / size.z;
        }
        else if (Mathf.Abs(normal.x) > 0.5f)
        {
            u = localPoint.z / size.z;
            v = localPoint.y / size.y;
        }
        else
        {
            u = localPoint.x / size.x;
            v = localPoint.y / size.y;
        }

        int px = Mathf.Clamp((int)(u * tex.width),  0, tex.width  - 1);
        int py = Mathf.Clamp((int)(v * tex.height), 0, tex.height - 1);

        // Debug every call
        // Debug.Log($"Painting at u:{u:F3} v:{v:F3} → px:{px} py:{py} on tex:{tex.GetInstanceID()}");
        // Debug.Log($"Renderer material tex: {hit2.transform.GetComponent<Renderer>().material.GetTexture("_BaseMap")?.GetInstanceID()}");

        tex.SetPixel(px, py, Color.white);
        tex.Apply();

        //Color check = tex.GetPixel(px, py);
        //Debug.Log($"Pixel color after set: {check}");
    }
}
