using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlayerDirectionIndicator : MonoBehaviour
{
    public Transform player;       // プレイヤーのTransform
    public float radius = 5f;      // 放射状の扇の半径
    public float angle = 45f;      // 扇形の角度
    public int segments = 20;      // 滑らかさのためのセグメント数
    public Color innerColor = Color.red;  // 中心の色（赤色）
    public Color outerColor = new Color(1f, 0f, 0f, 0); // 外側の色（透明な赤）

    private Mesh mesh;

    void Start()
    {
        // メッシュとマテリアルを設定
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // カスタムマテリアルを適用
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Custom/UnlitVertexColor"));
        renderer.material = material;

        UpdateMesh();
    }

    void Update()
    {
        // プレイヤーの位置と向きに追従
        transform.position = player.position;
        transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
    }
private void UpdateMesh()
{
    // メッシュの頂点と三角形の生成
    int vertexCount = segments + 2;
    Vector3[] vertices = new Vector3[vertexCount];
    Color[] colors = new Color[vertexCount];
    int[] triangles = new int[segments * 3];

    // 中心点
    vertices[0] = Vector3.zero;
    colors[0] = innerColor;

    float angleStep = angle / segments;
    float halfAngle = angle / 2f;

    // 放射状の頂点を生成 (Z軸の負方向に向けるために -z を使用)
    for (int i = 0; i <= segments; i++)
    {
        float currentAngle = Mathf.Deg2Rad * (-halfAngle + i * angleStep);
        float x = Mathf.Sin(currentAngle) * radius;
        float z = Mathf.Cos(currentAngle) * radius; // Z軸を負方向に変更

        vertices[i + 1] = new Vector3(x, 0, z);
        colors[i + 1] = outerColor;
    }

    // 三角形を生成
    for (int i = 0; i < segments; i++)
    {
        int start = i * 3;
        triangles[start] = 0;
        triangles[start + 1] = i + 1;
        triangles[start + 2] = i + 2;
    }

    // メッシュに設定
    mesh.Clear();
    mesh.vertices = vertices;
    mesh.colors = colors; // 頂点カラーを設定
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
}
}
