using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class NestedGameObjectList
{
    public List<GameObject> sampleList;
}


public class MapGenerator : MonoBehaviour
{
    private PhotonView photonView;
    public int mapSize;
    public float scaleMultiplier = 10;
    [Header("Land")]
    public int waterLevel;
    public float beachLevel;
    public GameObject water;
    public bool randomSeed;
    public float noiseScale;

    public int octaves;
    public float persistance;
    public float lacunarity;

    public float meshHeight;
    public AnimationCurve meshHeightCurve;
    public bool useFalloff = true;
    public AnimationCurve falloffCurve;
    public int falloffType = 1;
    public int seed;
    public Vector2 offset;

    private float[,] falloffMap;

     [Header("Trees")]
    public List<GameObject> trees;
    public List<GameObject> spawnedTrees;

    public float noiseScaleTree;

    public int octavesTree;
    public float persistanceTree;
    public float lacunarityTree;
    public Vector2 offsetTree;
    public Vector2 treeBounds;
    public float treeDensity = 1;

    [Header("Biomes")]

    public float minBiomePointDist;
    public float maxBiomePointDist;

    public int biomeSubPoints = 3;
    public List<NestedGameObjectList> treeList = new List<NestedGameObjectList>();
    public List<Vector3> biomePoints;
    
    [Header("Misc")]
    public GameObject lobby;

    public void ClientGenerationRequest(){
        photonView = PhotonView.Get(this);

        // Generate a random seed for the world
        if(randomSeed){seed = Random.Range(0, int.MaxValue);}
        // generate same map across all players
        photonView.RPC("GenerateMap", RpcTarget.All, seed);
    }

    [PunRPC]
    public void GenerateMap(int seed, bool randSeed = true, bool testClient = false){
        //delete old map
        foreach (GameObject obj in spawnedTrees)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        // Optionally clear the list
        spawnedTrees.Clear();

        biomePoints.Clear();

        if(testClient && randomSeed){seed = Random.Range(0, int.MaxValue);}
        lobby.SetActive(false);
        if(falloffType == 1){
            falloffMap = FalloffGenerator.GenerateFalloffMapCircle(mapSize, falloffCurve);
        }else if(falloffType == 2){
            falloffMap = FalloffGenerator.GenerateFalloffMapSquare(mapSize, falloffCurve);
        }
        
        //if(randomSeed){seed =  Random.Range(1,1000000);}
        float[,] noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, offset ); 
        
        for(int y = 0; y < mapSize; y++){
            for(int x = 0; x < mapSize; x++){
                if(useFalloff){
                    noiseMap[x,y] = Mathf.Clamp01(noiseMap[x,y] - falloffMap[x,y]);
                }
            }
        }
        

        MapDisplay display = FindObjectOfType<MapDisplay>(); 
        //display.DrawNoiseMap(noiseMap);
        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeight, meshHeightCurve));


        // spawn trees
        GenerateBiomes(seed);
        display.SetMeshBiomes(biomePoints, biomeSubPoints, beachLevel);
        GenerateTrees(seed);
    }

    private void GenerateBiomes(int seed){
        Random.InitState(seed);
        (Vector3 centerPoint, bool temp) = GroundRayCast(new Vector3(0,300,0));
        biomePoints.Add(centerPoint);
        //add mainBiomePoints
        for(int i = 1; i < treeList.Count;){
            (Vector3 newPoint, bool validated) = GroundRayCast(new Vector3(Random.Range(-mapSize* scaleMultiplier,mapSize* scaleMultiplier) ,300,Random.Range(-mapSize* scaleMultiplier,mapSize* scaleMultiplier)));
            if(validated){
                bool tooClose = false;
                bool tooFar = true;
                foreach(Vector3 point in biomePoints){
                    if(Vector3.Distance(point,newPoint) < minBiomePointDist){
                        tooClose = true;
                    }
                    if(Vector3.Distance(point,newPoint) < maxBiomePointDist){
                        tooFar = false;
                    }
                }
                if(!tooClose && !tooFar){
                    biomePoints.Add(newPoint);
                    i++;
                }
            }
        }
        //add subbiomePoints
        int prevCount = biomePoints.Count;
        List<Vector3> prevList = new List<Vector3>();
        foreach(Vector3 pos in biomePoints){
            prevList.Add(pos);
        }
        for(int i = 0; i < prevCount; i++){
            for(int j = 0; j < biomeSubPoints;){
                (Vector3 newPoint, bool validated) = GroundRayCast(new Vector3(Random.Range(-mapSize* scaleMultiplier,mapSize* scaleMultiplier) ,300,Random.Range(-mapSize* scaleMultiplier,mapSize* scaleMultiplier)));
                if(validated){
                    float dist = Vector3.Distance(prevList[i],newPoint);
                    bool valid = true;
                    foreach(Vector3 point in prevList){
                        if(Vector3.Distance(point,newPoint) < dist){
                            valid = false;
                        }
                    }
                    if(valid == true){
                        biomePoints.Insert(i + (biomeSubPoints * i), newPoint);
                        //biomePoints.Add(newPoint);
                        j++;
                    }
                }
            }
            
        }
    }

    private (Vector3, bool) GroundRayCast(Vector3 position, bool ignoreWater = false){
        int layerMask = 1 << 6; //ground mesh layer
        RaycastHit hit;
        if (Physics.Raycast(position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            if(hit.point.y > waterLevel || ignoreWater){
                return (hit.point, true);
            }else{
                return (hit.point, false);
            }
                            
        }else{
            return (new Vector3(0,0,0), false);
        }
    }

    private void GenerateTrees(int seed){
        Random.InitState(seed);

        float[,] noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed, noiseScaleTree, octavesTree, persistanceTree, lacunarityTree, offsetTree);
        Random.InitState(seed);
        for(int y = 0; y < mapSize; y++){
            for(int x = 0; x < mapSize; x++){
                float rand = Random.Range(0f,1f);
                if((noiseMap[x,y] > rand && noiseMap[x,y] > treeBounds.x) || noiseMap[x,y] > treeBounds.y){
                    for(int i = 0; i < treeDensity; i++){
                        Vector3 currPos = (new Vector3(0-(mapSize/2f) + x, 300, 0-(mapSize/2f) + y) * scaleMultiplier);
                        currPos += new Vector3(Random.Range(-8, 8), 0 ,Random.Range(-8, 8));
                        (Vector3 groundPos, bool validated)= GroundRayCast(currPos);
                        if(validated == true && groundPos.y > beachLevel){
                            //find biome
                            int biomeNum = -1;
                            float biomeDist = 100000;
                            for(int j = 0; j < biomePoints.Count; j++){
                                float temp = Vector3.Distance(biomePoints[j], groundPos);
                                if(temp < biomeDist){
                                    biomeDist = temp;
                                    biomeNum = j;
                                }
                            }
                            
                            List<GameObject> choosenTreeList = treeList[biomeNum/(biomeSubPoints+1)].sampleList;
                            //spawn tree from biome
                            GameObject newTree = Instantiate(choosenTreeList[0/*Random.Range(0,trees.Count)*/], groundPos - new Vector3(0,0.2f,0), Quaternion.identity);
                            newTree.transform.eulerAngles = new Vector3(0, Random.Range(0,360), 0);
                            spawnedTrees.Add(newTree);
                        }
                        
                    }
                    
                }
                
                
            }
        }
    }
}
