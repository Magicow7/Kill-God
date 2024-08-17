using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public MeshCollider meshColldier;

    private Mesh worldMesh;
    public float scaleMultiplier = 10;
    
    public void DrawNoiseMap(float[,] noiseMap){
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for(int y = 0; y < height; y++){
            for(int x = 0; x < width; x++){
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x,y]);
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
       
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }

    public void DrawMesh(MeshData meshData){
        worldMesh = meshData.CreateMesh();
        meshFilter.sharedMesh = worldMesh;
        meshColldier.sharedMesh = worldMesh;
        
    }

    public void SetMeshBiomes(List<Vector3> biomePoints, int biomeSubPoints, float beachLevel){
        int[] triangles = worldMesh.triangles;
        Vector3[] vertices = worldMesh.vertices;
        List<List<int>> submeshes = new List<List<int>>();
        worldMesh.subMeshCount = (biomePoints.Count/(biomeSubPoints+1))+1;
        for(int i = 0; i < (biomePoints.Count/(biomeSubPoints+1))+1; i++){
            submeshes.Add(new List<int>());
        }

        //for each triangle, get center, find closest biomePoint, and add triangle to corrosponding submesh list
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];
            Vector3 centerPoint = ((v0 + v1 + v2) / 3)*scaleMultiplier;

            if(centerPoint.y < beachLevel){
                int temp = biomePoints.Count/(biomeSubPoints+1);
                submeshes[temp].Add(triangles[i]);
                submeshes[temp].Add(triangles[i+1]);
                submeshes[temp].Add(triangles[i+2]);
            }else{
                int biomeNum = -1;
                float biomeDist = 100000;
                for(int j = 0; j < biomePoints.Count; j++){
                    float temp = Vector3.Distance(biomePoints[j], centerPoint);
                    if(temp < biomeDist){
                        biomeDist = temp;
                        biomeNum = j;
                    }
                }
                biomeNum = biomeNum/(biomeSubPoints+1);
                submeshes[biomeNum].Add(triangles[i]);
                submeshes[biomeNum].Add(triangles[i+1]);
                submeshes[biomeNum].Add(triangles[i+2]);
            }

        }

        for(int i = 0; i < submeshes.Count; i++){
            worldMesh.SetTriangles(submeshes[i], i);
        }
        


    }
}

