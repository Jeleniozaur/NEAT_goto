using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetScript : MonoBehaviour
{
    NEAT neat;

    private void Start()
    {
        changePos();
        neat = GameObject.Find("NEAT").GetComponent<NEAT>();
    }

    private void OnEnable()
    {
        NEAT.OnNextGeneration += changePos;
    }

    void changePos()
    {
        transform.position = new Vector2(Random.Range(5f, 10f), Random.Range(-10f, 10f));
        if(Random.Range(0f,100f)<=50f)
        {
            transform.position *= -1f;
        }
    }
}
