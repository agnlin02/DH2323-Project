using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.Rendering;

public class RunPhotonShader : MonoBehaviour
{
    public ComputeShader computeShader;
    public Light lightSource;
    public MeshFilter[] groundObjects;
    public MeshFilter[] waterObjects;

    public float eta1 = 1.0003f;
    public float eta2 = 1.333f;


    private RenderTexture result;
    private ComputeBuffer groundTriangleBuffer;
    private ComputeBuffer waterTriangleBuffer;


    [StructLayout(LayoutKind.Sequential)]
    struct Triangle
    {
        public Vector3 v0, v1, v2;
        public Vector3 normal;
        public Vector2 uv0, uv1, uv2;
    }

    int texWidth = 512;
    int texHeight = 512;

    void Start()
    {
        result = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.Create();

        BuildTriangleBuffers();
        CastRays();
        ApplyTex();
    }

    void ApplyTex()
    {
        foreach (var obj in groundObjects)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            //Graphics.Blit(Texture2D.whiteTexture, result);

            Material m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetTexture("_BaseMap", result);
            rend.material = m;
        }
    }


    void BuildTriangleBuffers()
    {
        groundTriangleBuffer = BuildBuffer(groundObjects);
        waterTriangleBuffer = BuildBuffer(waterObjects);
    }

    // Add all triangles to the buffer
    ComputeBuffer BuildBuffer(MeshFilter[] objects)
    {
        var triangles = new System.Collections.Generic.List<Triangle>();

        foreach (var obj in objects)
        {
            Mesh mesh = obj.mesh;

            Vector3[] vertices = mesh.vertices;
            int[] indices = mesh.triangles;
            Vector2[] uvs = mesh.uv;


            for (int i = 0; i < indices.Length; i += 3)
            {
                Triangle tri = new Triangle();
                tri.v0 = obj.transform.TransformPoint(vertices[indices[i]]);
                tri.v1 = obj.transform.TransformPoint(vertices[indices[i+1]]);
                tri.v2 = obj.transform.TransformPoint(vertices[indices[i+2]]);

                tri.uv0 = uvs[indices[i]];
                tri.uv1 = uvs[indices[i+1]];
                tri.uv2 = uvs[indices[i+2]];

                //Debug.Log(tri.uv0 + ", "+ tri.uv1+", "+tri.uv2);

                tri.normal = Vector3.Cross(tri.v1 - tri.v0, tri.v2 - tri.v0).normalized;
                triangles.Add(tri);
            }
        }

        ComputeBuffer buffer = new ComputeBuffer(triangles.Count, Marshal.SizeOf(typeof(Triangle)));
        buffer.SetData(triangles.ToArray());

/*         for (int i = 0; i < Mathf.Min(3, triangles.Count); i++)
        {
            Debug.Log($"Trangle {i} notmal {triangles[i].normal}");
        } */
        return buffer;
    }

    void CastRays()
    {
        int kernel = computeShader.FindKernel("PhotonRay");

        computeShader.SetTexture(kernel, "Result", result);
        computeShader.SetBuffer(kernel, "GroundTriangles", groundTriangleBuffer);
        computeShader.SetBuffer(kernel, "WaterTriangles", waterTriangleBuffer);

        computeShader.SetInt("GroundTriangleCount", groundTriangleBuffer.count);
        computeShader.SetInt("WaterTriangleCount", waterTriangleBuffer.count);

        computeShader.SetVector("LightPosition", lightSource.transform.position);
        computeShader.SetInt("TextureWidth", texWidth);
        computeShader.SetInt("TextureHeight", texHeight);
        computeShader.SetFloat("eta1", eta1);
        computeShader.SetFloat("eta2", eta2);

        computeShader.Dispatch(kernel, texWidth / 8, texHeight / 8, 1);

        // TODO: Second pass for displaying light, first pass only for counting (might add more secondary passes)

/*         int kernel2 = computeShader.FindKernel("PhotonDisplay");
        computeShader.SetTexture(kernel2, "PhotonCounts", PhotonCounts);
        computeShader.SetTexture(kernel2, "Result", result);
        computeShader.Dispatch(kernel2, texWidth / 8, texHeight / 8, 1); */
    }

    void OnGUI()
    {
        // Does not work in current version since UV is written to (using Result[tex_coord] instead of Result[id.xy])
        GUI.DrawTexture(new Rect(0, 0, texWidth, texHeight), result);
    } 
    void OnDestroy()
    {
        waterTriangleBuffer?.Release();
        groundTriangleBuffer?.Release();

        result?.Release();
        /* PhotonCounts?.Release(); */
    }

}