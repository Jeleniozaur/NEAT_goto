using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetwork : MonoBehaviour
{
    [System.Serializable]
    public class Node
    {
        public float value;
        public float bias;
        public List<Weight> weights = new List<Weight>();
        public Transform uiElement;
    }

    [System.Serializable]
    public class Weight
    {
        public float value;
        public Node connectedTo;
        public Transform uiElement;
    }

    [System.Serializable]
    public class Layer
    {
        public List<Node> nodes = new List<Node>();
    }

    [Header("Neural Network settings")]
    public List<Layer> layers = new List<Layer>();
    public Layer inputLayer, outputLayer;

    [Header("UI settings")]
    public bool showNeuralNet = true;
    public float nodeSize = 50f;
    public float weightSize = 10f;
    public float xSpacing = 100f;
    public float padding = 10f;
    public float ySize = 200f;
    [Header("Fitness")]
    public float fitness = 0;
    public bool finished = false;
    public bool died = false;
    [Range(0,1)]
    public float weightAlpha = 0.5f;
    public Transform nodePrefab;
    public Transform weightPrefab;
    public Transform ui_parentPrefab;
    Transform uiParent;
    List<Transform> uiElements = new List<Transform>();

    private void OnDestroy()
    {
        Destroy(uiParent.gameObject);
    }

    public void connectNodes()
    {
        for (int i = 0; i < layers.Count - 1; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                for(int k = 0; k < layers[i+1].nodes.Count; k++)//next layer nodes
                {
                    layers[i].nodes[j].weights.Add(new Weight());
                    layers[i].nodes[j].weights[k].connectedTo = layers[i + 1].nodes[k];
                }
            }
        }
    }

    private void Update()
    {
        updateNeuralNetValues();
        uiParent.gameObject.SetActive(showNeuralNet);
        if(showNeuralNet)
        {
            UpdateNeuralNetUI();
        }
    }

    public void updateNeuralNetValues()
    {
        //sum
        for(int i = 0; i < layers.Count-1; i++)//layer
        {
            for(int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                for(int k = 0; k < layers[i].nodes[j].weights.Count; k++)//weight
                {
                    layers[i].nodes[j].weights[k].connectedTo.value += layers[i].nodes[j].value* layers[i].nodes[j].weights[k].value;
                }
            }
        }

        //add bias and sigmoid
        for (int i = 1; i < layers.Count; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                layers[i].nodes[j].value += layers[i].nodes[j].bias;
                layers[i].nodes[j].value = (Sigmoid(layers[i].nodes[j].value)*2f)-1;
            }
        }
    }

    public void removeBrain()
    {
        layers = new List<Layer>();
    }

    public static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Exp(-value));
    }

    public void disableWeight(List<Weight> wl, Weight w)
    {
        wl.Remove(w);
    }

    public void addHiddenLayer()
    {
        layers.Insert(layers.Count - 1, new Layer());
    }

    public void randomizeWeightsAndBiases()
    {
        //randomize weights
        for (int i = 0; i < layers.Count - 1; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                var br = Random.Range(-1f, 1f);
                layers[i+1].nodes[j].bias = br;
                for (int k = 0; k < layers[i].nodes[j].weights.Count; k++)//weight
                {
                    layers[i].nodes[j].weights[k].value = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public void addNodeToRandomHiddenLayer()
    {
        var r = Random.Range(1, layers.Count - 1);
        layers[r].nodes.Add(new Node());
        layers[r].nodes[layers[r].nodes.Count - 1].bias = Random.Range(-1f, 1f);
        connectNodeToRandomNode(r, layers[r].nodes.Count-1);
    }

    public void connectNodeToRandomNode(int layerIndex, int nodeIndex)
    {
        bool canCreateConnection = true;
        var rL = Random.Range(layerIndex + 1, layers.Count);
        var rN = Random.Range(0, layers[rL].nodes.Count);
        canCreateConnection = layers[rL].nodes.Count > 0;
        if (canCreateConnection)
        {
            for (int i = 0; i < layers[layerIndex].nodes[nodeIndex].weights.Count; i++)
            {
                if (layers[layerIndex].nodes[nodeIndex].weights[i].connectedTo == layers[rL].nodes[rN])
                {
                    canCreateConnection = false;
                    break;
                }
            }
        }
        if (canCreateConnection)
        {
            layers[layerIndex].nodes[nodeIndex].weights.Add(new Weight());
            layers[layerIndex].nodes[nodeIndex].weights[layers[layerIndex].nodes[nodeIndex].weights.Count - 1].connectedTo = layers[rL].nodes[rN];
            layers[layerIndex].nodes[nodeIndex].weights[layers[layerIndex].nodes[nodeIndex].weights.Count - 1].value = Random.Range(-1f, 1f);
        }
    }

    public void UpdateNeuralNetUI()
    {
        //update nodes
        for (int i = 0; i < layers.Count; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                layers[i].nodes[j].uiElement.GetChild(0).GetComponent<Text>().text = (Mathf.Round(layers[i].nodes[j].value * 100) / 100).ToString();
                var val = (layers[i].nodes[j].value+1)/2f;
                var col = new Color(val, val, val);
                layers[i].nodes[j].uiElement.GetComponent<Image>().color = col;
            }
        }
        //update weights
        for (int i = 0; i < layers.Count - 1; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                for (int k = 0; k < layers[i].nodes[j].weights.Count; k++)//weight
                {
                    if (layers[i].nodes[j].weights[k].value > 0)
                    {
                        var col = Color.green;
                        col.a = weightAlpha;
                        layers[i].nodes[j].weights[k].uiElement.GetComponent<Image>().color = col;
                    }
                    else
                    {
                        var col = Color.red;
                        col.a = weightAlpha;
                        layers[i].nodes[j].weights[k].uiElement.GetComponent<Image>().color = col;
                    }
                    var scl = layers[i].nodes[j].weights[k].uiElement.transform.localScale;
                    scl.y = Mathf.Abs(layers[i].nodes[j].weights[k].value);
                    layers[i].nodes[j].weights[k].uiElement.transform.localScale = scl;
                }
            }
        }
    }

    public void DrawNeuralNetUI()
    {
        inputLayer = layers[0];
        outputLayer = layers[layers.Count - 1];

        uiParent = Instantiate(ui_parentPrefab);
        uiParent.SetParent(GameObject.Find("NeuralNet_UI").transform);
        uiParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //draw nodes
        for (int i = 0; i < layers.Count; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                var img = Instantiate(nodePrefab);
                img.SetParent(uiParent);
                var rt = img.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(nodeSize, nodeSize);
                rt.anchoredPosition = new Vector2(padding + (xSpacing * i), -(j + 1) * (ySize / (layers[i].nodes.Count + 1)));
                layers[i].nodes[j].uiElement = img.transform;
                uiElements.Add(img.transform);
            }
        }
        //draw connections
        for (int i = 0; i < layers.Count - 1; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//node
            {
                for (int k = 0; k < layers[i].nodes[j].weights.Count; k++)//weight
                {
                    var img = Instantiate(weightPrefab);
                    img.SetParent(uiParent);
                    img.SetAsFirstSibling();
                    var rt = img.GetComponent<RectTransform>();
                    var nrt = layers[i].nodes[j].uiElement.transform.GetComponent<RectTransform>();
                    var ctrt = layers[i].nodes[j].weights[k].connectedTo.uiElement.transform.GetComponent<RectTransform>();
                    var wPos = nrt.anchoredPosition;
                    wPos.x += nodeSize/2f - weightSize/2f;
                    wPos.y -= nodeSize / 2f;
                    rt.anchoredPosition = wPos;
                    rt.right = ctrt.anchoredPosition - nrt.anchoredPosition;
                    rt.sizeDelta = new Vector3(Vector2.Distance(nrt.anchoredPosition, ctrt.anchoredPosition),weightSize);

                    layers[i].nodes[j].weights[k].uiElement = img.transform;
                    uiElements.Add(img.transform);
                }
            }
        }
    }
}
