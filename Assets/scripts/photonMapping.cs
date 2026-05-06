using UnityEngine;
using System;

public class photonMapping : MonoBehaviour
{
    LineRenderer lineRenderer;

    public float eta1 = 1.0003f;
    public float eta2 = 1.333f;

    LayerMask groundMask;
    LayerMask waterMask;

   void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
        waterMask = LayerMask.GetMask("Water");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        lineRenderer.positionCount = 3;

        // // Set material texture 
        // GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // foreach (GameObject obj in allObjects)
        // {
        //     if (obj.layer != groundMask) continue;

        //     Renderer rend = obj.GetComponent<Renderer>();
        //     if (rend == null) continue;

        //     Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);

        //     // Fill black
        //     Color[] pixels = new Color[256 * 256];
        //     for (int i = 0; i < pixels.Length; i++)
        //         pixels[i] = Color.black;

        //     tex.SetPixels(pixels);
        //     tex.Apply();

        //     rend.material.SetTexture("_BaseMap", tex);
        // }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    { 
        photonRay(); 
    }

    

    void photonRay()
    {
        // Ray from light to durface
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity, waterMask)){
            Vector3 normal = hit.normal;
            // Create a line render from light to water suface
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hit.point);

            // Calculate end points and direction for the ray
            Vector3 endPoint = hit.point + hit.normal * -2f;
            Vector3 direction = endPoint - hit.point;

            Vector3 e = hit.point - transform.position;

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
            // GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // dot.transform.position = hit.point;
            // dot.transform.localScale = Vector3.one * 0.1f;

            // Renderer renderer = dot.GetComponent<Renderer>();
            // renderer.material.color = Color.white;

            // Ray from surface to ground
            if (Physics.Raycast(hit.point, t, out RaycastHit hit2, Mathf.Infinity, groundMask)){
                lineRenderer.SetPosition(2, hit2.point);

                //Color GetPixelBilinear(float u, float v, int mipLevel = 0);



                Renderer rend = hit2.transform.GetComponent<Renderer>();     // Get hit component
                MeshCollider meshCollider = hit2.collider as MeshCollider;

                if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                    print("rend " + (rend == null));
                    print("rend.sharedMaterial " + (rend.sharedMaterial == null));
                    print("rend.sharedMaterial.Maintexture " + (rend.sharedMaterial.mainTexture == null));
                    print("meshcollider "+(meshCollider == null));
                    return;

                Texture2D tex = rend.material.mainTexture as Texture2D;     // Get component material texture
                Vector2 pixelUV = hit2.textureCoord;                 
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;

                tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.white);
                tex.Apply();

                // create sphere
                /* GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                dot.transform.position = hit2.point;
                dot.transform.localScale = Vector3.one * 0.1f;

                Renderer renderer = dot.GetComponent<Renderer>();
                renderer.material.color = Color.white;
 */
            }
        } 
    }
}
