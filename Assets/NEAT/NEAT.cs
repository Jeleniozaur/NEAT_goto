using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NEAT : MonoBehaviour
{
    public List<Transform> population = new List<Transform>();
    public int generation;
    public int generationCount = 10;
    public float simulationTime = 10f;
    [Range(0f, 100f)]
    public float disableWeightChance = 10f, newLayerChance = 1f, newNodeChance = 2f, newWeightChance = 4f, mutationChance = 10f;
    [Range(0f, 1f)]
    public float mutationRange = 0.1f;
    public Transform prefab;
    public bool fixedNeuralNetShape = false;
    float curTime;
    bool run = true;
    List<List<NeuralNetwork.Layer>> bestBrains = new List<List<NeuralNetwork.Layer>>();
    public Transform genTextPrefab;
    Text genTxt;

    public delegate void ng();
    public static event ng OnNextGeneration;

    private void Start()
    {
        var txt = Instantiate(genTextPrefab);
        txt.SetParent(GameObject.Find("NeuralNet_UI").transform);
        txt.transform.position = Vector2.zero;
        txt.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        genTxt = txt.GetComponent<Text>();
        curTime = simulationTime;
        if(generation % 2 != 0)
        {
            generationCount += 1;
        }
        createRandomGeneration();
        population[0].GetComponent<NeuralNetwork>().showNeuralNet = true;
    }

    private void Update()
    {
        if (run)
        {
            curTime -= Time.deltaTime;
            checkIfAllFinishedOrDied();
        }
        if(curTime <= 0 && run)
        {
            run = false;
            createNextGeneration();
        }
    }

    void checkIfAllFinishedOrDied()
    {
        bool val = true;
        for(int i = 0; i < population.Count; i++)
        {
            if(!population[i].GetComponent<NeuralNetwork>().finished && !population[i].GetComponent<NeuralNetwork>().finished)
            {
                val = false;
                break;
            }
        }

        if(val)
        {
            curTime = 0;
        }
    }

    void createNextGeneration()
    {
        sortPopulationByFitness();
        killHalfOfPopulation();
        saveBrains();
        killHalfOfPopulation();
        population = new List<Transform>();
        //recreate best half of population
        for(int i = 0; i < generationCount/2; i++)
        {
            var go = Instantiate(prefab);
            population.Add(go);
            var brain = go.GetComponent<NeuralNetwork>();
            brain.removeBrain();
            copyBrain(bestBrains[i], population[i].GetComponent<NeuralNetwork>().layers);
            brain.DrawNeuralNetUI();
        }
        //mutate rest of population based on best brains
        for (int i = generationCount / 2; i < generationCount; i++)
        {
            var go = Instantiate(prefab);
            population.Add(go);
            //copy brain
            var brain = population[i].GetComponent<NeuralNetwork>();
            brain.removeBrain();
            copyBrain(bestBrains[i - generationCount / 2], population[i].GetComponent<NeuralNetwork>().layers);
            if(!fixedNeuralNetShape)
            {
                //adding a new hidden layer
                if(chance(newLayerChance))
                {
                    brain.addHiddenLayer();
                }
                //adding a new node to hidden layer
                if(chance(newNodeChance) && brain.layers.Count > 2)
                {
                    brain.addNodeToRandomHiddenLayer();
                }
            }
            //mutate weights and biases
            for(int l = 0; l < brain.layers.Count; l++)//layer
            {
                for(int n = 0; n < brain.layers[l].nodes.Count; n++)//node
                {
                    if (l > 0)
                    {
                        brain.layers[l].nodes[n].bias += Random.Range(-mutationRange, mutationRange);
                    }
                    if(!fixedNeuralNetShape)
                    {
                        //creating new connection
                        if (chance(newWeightChance) && l < brain.layers.Count-1)
                        {
                            brain.connectNodeToRandomNode(l, n);
                        }
                    }
                    for (int w = 0; w < brain.layers[l].nodes[n].weights.Count; w++)//weight
                    {
                        if(chance(mutationChance))
                        {
                            brain.layers[l].nodes[n].weights[w].value += Random.Range(-mutationRange, mutationRange);
                            brain.layers[l].nodes[n].weights[w].value = Mathf.Clamp(brain.layers[l].nodes[n].weights[w].value, -1, 1);
                        }
                        if(!fixedNeuralNetShape)
                        {
                            //disabling weight
                            if (chance(disableWeightChance))
                            {
                                brain.disableWeight(brain.layers[l].nodes[n].weights, brain.layers[l].nodes[n].weights[w]);
                            }
                        }
                    }
                }
            }
            brain.DrawNeuralNetUI();
        }
        generation++;
        curTime = simulationTime;
        run = true;
        population[0].GetComponent<NeuralNetwork>().showNeuralNet = true;

        if(OnNextGeneration!=null)
        {
            OnNextGeneration();
        }
        genTxt.text = "Generation: " + generation + " ";
    }

    bool chance(float val)
    {
        return Random.Range(0f, 100f) <= val;
    }

    void saveBrains()
    {
        bestBrains = new List<List<NeuralNetwork.Layer>>();
        for(int i = 0; i < population.Count; i++)
        {
            bestBrains.Add(new List<NeuralNetwork.Layer>());
            copyBrain(population[i].GetComponent<NeuralNetwork>().layers, bestBrains[i]);
        }
    }

    void copyBrain(List<NeuralNetwork.Layer> donnor, List<NeuralNetwork.Layer> acceptor)
    {
        for (int i = 0; i < donnor.Count; i++)//layer
        {
            acceptor.Add(new NeuralNetwork.Layer());
            for(int j = 0; j < donnor[i].nodes.Count; j++)//node
            {
                acceptor[i].nodes.Add(new NeuralNetwork.Node());
                acceptor[i].nodes[j].bias = donnor[i].nodes[j].bias;
            }
        }
        //weights
        for (int i = 0; i < donnor.Count-1; i++)//layer
        {
            for (int j = 0; j < donnor[i].nodes.Count; j++)//node
            {
                for (int k = 0; k < donnor[i].nodes[j].weights.Count; k++)//weight
                {
                    acceptor[i].nodes[j].weights.Add(new NeuralNetwork.Weight());
                    acceptor[i].nodes[j].weights[k].value = donnor[i].nodes[j].weights[k].value;
                    for(int l = 0; l < donnor.Count; l++)
                    {
                        if(donnor[l].nodes.Contains(donnor[i].nodes[j].weights[k].connectedTo))
                        {
                            acceptor[i].nodes[j].weights[k].connectedTo = acceptor[l].nodes[donnor[l].nodes.IndexOf(donnor[i].nodes[j].weights[k].connectedTo)];
                            break;
                        }
                    }
                }
            }
        }
    }

    void createRandomGeneration()
    {
        for(int i = 0; i < generationCount; i++)
        {
            var go = Instantiate(prefab);
            population.Add(go);
            var brain = go.GetComponent<NeuralNetwork>();
            brain.connectNodes();
            brain.randomizeWeightsAndBiases();
            brain.DrawNeuralNetUI();
        }
    }

    void sortPopulationByFitness()
    {
        List<Transform> sortedList = new List<Transform>();
        while(population.Count > 0)
        {
            var highestValue = population[0];
            for (int i = 0; i < population.Count; i++)
            {
                if(population[i].GetComponent<NeuralNetwork>().fitness >= highestValue.GetComponent<NeuralNetwork>().fitness)
                {
                    highestValue = population[i];
                }
            }
            sortedList.Add(highestValue);
            population.Remove(highestValue);
        }
        population = sortedList;
    }

    void killHalfOfPopulation()
    {
        for(int i = 0; i < generationCount/2; i++)
        {
            Destroy(population[population.Count - 1].gameObject);
            population.Remove(population[population.Count - 1]);
        }
    }
}
