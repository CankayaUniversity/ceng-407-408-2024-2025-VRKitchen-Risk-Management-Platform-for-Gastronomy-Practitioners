using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundIndicator : MonoBehaviour
{
    public float TriangleSize = 0.2f;
    public Color TriangleColor = Color.yellow;
    public float TriangleOffset = 0.1f;
    public float TriangleMoveSpeed = 0.2f;
    public float TriangleMoveLength = 0.1f;
    public float TriangleRotationSpeed = 90f;
    
    Transform m_TriangleTransform;

    const string k_TriangleName = "Triangle";
    const string k_ShaderPath = "Universal Render Pipeline/Unlit";

    void Start()
    {
        Mesh mesh = new Mesh();

        mesh.SetVertices(new []{
            Vector3.zero,
            new Vector3(0.5f, 1.0f, 0.0f) * TriangleSize,
            new Vector3(0.0f, 1.0f, 0.5f) * TriangleSize,
            new Vector3(-0.5f, 1.0f, 0.0f) * TriangleSize, 
            new Vector3(0.0f, 1.0f, -0.5f) * TriangleSize
        });
        
        mesh.SetIndices(new []
        {
            0,1,2,
            0,2,3,
            0,3,4,
            0,4,1,
            1,3,2,
            1,4,3
        }, MeshTopology.Triangles, 0);
        
        mesh.UploadMeshData(true);

        GameObject triangleObject = new GameObject(k_TriangleName);
        m_TriangleTransform = triangleObject.transform;
        m_TriangleTransform.SetParent(transform, false);
        
        MeshFilter meshFilter = triangleObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = triangleObject.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = mesh;
        
        Material material = new Material(Shader.Find(k_ShaderPath));
        material.color = TriangleColor;
        meshRenderer.sharedMaterial = material;
    }

    void Update()
    {
        m_TriangleTransform.localPosition = new Vector3(0, TriangleOffset + Mathf.PingPong(Time.time * TriangleMoveSpeed, TriangleMoveLength), 0);
        m_TriangleTransform.Rotate(Vector3.up, TriangleRotationSpeed * Time.deltaTime, Space.Self);
    }
}
