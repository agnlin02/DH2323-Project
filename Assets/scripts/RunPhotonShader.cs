using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.Rendering;

public class RunPhotonShader : MonoBehaviour
{
    public ComputeShader computeShader;
    public Light lightSource;
    public Camera camera;
    public MeshFilter[] groundObjects;
    public MeshFilter[] waterObjects;

    public float eta1 = 1.0003f;
    public float eta2 = 1.333f;


    private RenderTexture photonCount;
    private RenderTexture screenRender;
    private RenderTexture texRender;
    private Texture2D displayTex;
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

    bool needsDisplay = false;

    void Start()
    {
        photonCount = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.RInt);
        photonCount.enableRandomWrite = true;
        photonCount.Create();

        screenRender = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGBFloat);
        screenRender.enableRandomWrite = true;
        screenRender.Create();
        texRender = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGBFloat);
        texRender.enableRandomWrite = true;
        texRender.Create();

        BuildTriangleBuffers();
        CastLightRays();
        CastCameraRays();
        ApplyTex();

        needsDisplay = true;
    }

    void Update()
    {
        if (needsDisplay)
        {
            ConvertForDisplay();
            needsDisplay = false;
        }
    }

    void ConvertForDisplay()
    {
        RenderTexture.active = screenRender; // swap to photonCount
        displayTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false); // match photonCount format
        displayTex.ReadPixels(new Rect(0, 0, texWidth, texHeight), 0, 0);
        displayTex.Apply();
        RenderTexture.active = null;
    }

    void ApplyTex()
    {
        foreach (var obj in groundObjects)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            //Graphics.Blit(Texture2D.whiteTexture, photonCount);

            Material m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetTexture("_BaseMap", texRender);
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

    void CastLightRays()
    {
        int kernel = computeShader.FindKernel("PhotonRay");

        computeShader.SetTexture(kernel, "PhotonCount", photonCount);
        computeShader.SetBuffer(kernel, "GroundTriangles", groundTriangleBuffer);
        computeShader.SetBuffer(kernel, "WaterTriangles", waterTriangleBuffer);

        computeShader.SetInt("GroundTriangleCount", groundTriangleBuffer.count);
        computeShader.SetInt("WaterTriangleCount", waterTriangleBuffer.count);
        computeShader.SetInt("TextureWidth", texWidth);
        computeShader.SetInt("TextureHeight", texHeight);

        computeShader.SetVector("LightPosition", lightSource.transform.position);
        computeShader.SetFloat("eta1", eta1);
        computeShader.SetFloat("eta2", eta2);

        computeShader.Dispatch(kernel, texWidth / 8, texHeight / 8, 1);

        Debug.Log("Rays cast");

    }

    void CastCameraRays()
    {
        // TODO: Second pass for displaying light, first pass only for counting (might add more secondary passes)

        int kernel2 = computeShader.FindKernel("ScreenRay");
        computeShader.SetTexture(kernel2, "PhotonCount", photonCount);
        computeShader.SetBuffer(kernel2, "GroundTriangles", groundTriangleBuffer);

        computeShader.SetInt("GroundTriangleCount", groundTriangleBuffer.count);
        computeShader.SetInt("TextureWidth", texWidth);
        computeShader.SetInt("TextureHeight", texHeight);

        computeShader.SetTexture(kernel2, "Result", screenRender);
        computeShader.SetTexture(kernel2, "Result2", texRender);
        Camera cam = camera;
        computeShader.SetVector("CameraPosition", cam.transform.position);
        computeShader.SetVector("CameraForward", cam.transform.forward);
        computeShader.SetVector("CameraUp", cam.transform.up);
        computeShader.SetVector("CameraRight", cam.transform.right);
        computeShader.SetFloat("CameraFOV", cam.fieldOfView);


        computeShader.Dispatch(kernel2, texWidth / 8, texHeight / 8, 1);

        Debug.Log("Screen rays cast");
    }

    void OnGUI()
    {
    if (displayTex != null)
        GUI.DrawTexture(new Rect(0, 0, texWidth, texHeight), displayTex);
    }

    void OnDestroy()
    {
        waterTriangleBuffer?.Release();
        groundTriangleBuffer?.Release();

        screenRender?.Release();
        texRender?.Release();
        photonCount?.Release();
    }

}