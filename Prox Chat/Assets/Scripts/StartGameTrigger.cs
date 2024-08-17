using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartGameTrigger : MonoBehaviour
{
    public MapGenerator mapGenerator;

    private void OnTriggerEnter(){
        mapGenerator.ClientGenerationRequest();
    }
}
