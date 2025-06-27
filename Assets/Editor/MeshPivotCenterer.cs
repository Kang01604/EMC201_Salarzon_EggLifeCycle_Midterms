using UnityEngine;
using UnityEditor;

public class MeshPivotCenterer : MonoBehaviour
{
    [MenuItem("Tools/Center Selected Mesh Pivot")]
    static void CenterPivot()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogWarning("Select a GameObject"); return; }

        var mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("No MeshFilter or Mesh found."); 
            return;
        }

        // Duplicate mesh so we don't overwrite the original asset
        var mesh = Instantiate(mf.sharedMesh);
        mesh.name = mf.sharedMesh.name + "_Centered";
        mesh.RecalculateBounds();
        Vector3 center = mesh.bounds.center;

        var verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
            verts[i] -= center;
        mesh.vertices = verts;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        // Move the GameObject so it stays in place
        go.transform.position += go.transform.TransformVector(center);

        Debug.Log($"Pivot centered for {go.name}");
    }
}
