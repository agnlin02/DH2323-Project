using UnityEngine;
using System;

public class photonMapping : MonoBehaviour
{
    LineRenderer lineRenderer;

    public float eta1 = 1.0003f;
    public float eta2 = 1.333f;

    LayerMask layerMask;

   void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
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
        
    }

    // Update is called once per frame
    void FixedUpdate()
    { 
        photonRay(); 
    }

    

    void photonRay()
    {
        // Ray from light to durface
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity)){
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
            
            print("normal"+normal); 
            print("E: "+ e);
            print("t"+ t);      
            /* Console.WriteLine("normal ", normal);
            Console.WriteLine("E: ", e);
            Console.WriteLine("t", t);
             */
            //  GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // dot.transform.position = hit.point;
            // dot.transform.localScale = Vector3.one * 0.1f;

            // Renderer renderer = dot.GetComponent<Renderer>();
            // renderer.material.color = Color.white;

            // Ray from surface to ground
            if (Physics.Raycast(hit.point, t, out RaycastHit hit2, Mathf.Infinity, layerMask)){
                lineRenderer.SetPosition(2, hit2.point);

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
