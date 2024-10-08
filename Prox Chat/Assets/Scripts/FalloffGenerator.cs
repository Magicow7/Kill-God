using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMapCircle(int size, AnimationCurve falloffCurve){
        float[,] map = new float[size,size];

        float center = (size - 1)/2f;
        float maxDistance = Mathf.Sqrt(Mathf.Pow((size - 1) / 2.0f, 2) + Mathf.Pow((size - 1) / 2.0f, 2));

        for(int i = 0; i < size; i++){
            for(int j = 0; j < size; j++){

                float value = Mathf.Sqrt(Mathf.Pow(i - center, 2) + Mathf.Pow(j - center, 2));   
                value = value / maxDistance;
                value = falloffCurve.Evaluate(value); 
                map[i, j] = value;
            }
        }

        return map;

        
    }

    public static float[,] GenerateFalloffMapSquare(int size, AnimationCurve falloffCurve){
        float[,] map = new float[size,size];
        for(int i = 0; i < size; i++){
            for(int j = 0; j < size; j++){
                float x = i / (float)size * 2 - 1; 
                float y = j / (float)size * 2 - 1;

                float value =  Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                value = falloffCurve.Evaluate(value); 
                map[i, j] = value;
            }
        }

        return map;
    }
}
