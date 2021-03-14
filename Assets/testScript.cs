using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour
{
    Transform target;
    Rigidbody2D rb;
    NeuralNetwork brain;
    public float speed = 2f;
    float timer = 0f;

    private void Start()
    {
        transform.position = Vector2.zero;
        target = GameObject.Find("Target").transform;
        rb = gameObject.GetComponent<Rigidbody2D>();
        brain = gameObject.GetComponent<NeuralNetwork>();
    }

    private void Update()
    {
        //update input values
        var dir = target.position - this.transform.position;
        dir.Normalize();
        brain.inputLayer.nodes[0].value = dir.x;
        brain.inputLayer.nodes[1].value = dir.y;
        //fitness
        brain.fitness = -Vector2.Distance(transform.position, target.position)-timer;
    }

    private void FixedUpdate()
    {
        if (!brain.finished)
        {
            float ix, iy;
            ix = brain.outputLayer.nodes[0].value;
            iy = brain.outputLayer.nodes[1].value;
            Vector2 input = new Vector2(ix, iy);
            rb.velocity = input * speed;
            timer -= Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        brain.finished = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
    }
}
