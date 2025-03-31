using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeConstructor : MonoBehaviour
{
    [SerializeField] private GameObject lineRendererPrefab;
    [SerializeField] private int iterations = 1;
    [SerializeField] private float stepDistance = 1f;
    [SerializeField] private float angle = 25f;

    private string _dna = "X";
    private List<Vector3> linePoints = new List<Vector3>();
    private List<GameObject> sphereList = new List<GameObject>();

    public void Start()
    {
        _dna = "X";
        for (int i = 0; i < iterations; i++)
        {
            _dna = Generate();
        }

        BuildTree();
        DrawTree();
    }

    public string Generate()
    {
        var next = "";
        foreach (var c in _dna)
        {
            if (c.ToString() == nameof(Rules.X))
            {
                next += ChooseOne(Rules.X);
            }
            else if (c.ToString() == nameof(Rules.F))
            {
                next += ChooseOne(Rules.F);;
            }
            else
            {
                next += c;
            }
        }

        Debug.Log(next);
        return next;
    }

    private string ChooseOne(Dictionary<string, float> dictionary)
    {
        var r = UnityEngine.Random.Range(0f, 1f);
        var total = 0f;
        foreach (var probability in dictionary.Values)
        {
            total += probability;

            if (total > r)
            {
                return dictionary.Keys.ElementAt(dictionary.Values.ToList().IndexOf(probability));
            }
        }

        throw new Exception("No valid choice found");
    }

    private void BuildTree()
    {
        linePoints.Clear();

        Vector3 position = Vector3.zero;
        Vector3 heading = Vector3.up;
        Stack<(Vector3 pos, Vector3 head)> stateStack = new();

        foreach (char c in _dna)
        {
            switch (c)
            {
                case 'F':
                    Vector3 nextPosition = position + heading * stepDistance;
                    linePoints.Add(position);
                    position = nextPosition;
                    break;

                case '+':
                    heading = Quaternion.AngleAxis(angle, Vector3.forward) * heading;
                    break;

                case '-':
                    heading = Quaternion.AngleAxis(-angle, Vector3.forward) * heading;
                    break;

                case '[':
                    stateStack.Push((position, heading));
                    break;

                case ']':
                    if (stateStack.Count > 0)
                    {
                        (position, heading) = stateStack.Pop();
                    }

                    break;

                default:
                    break;
            }
        }
    }

    public void DrawTree()
    {
        DestroyPreviousTree();

        GameObject lineObj = Instantiate(lineRendererPrefab, transform);
        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints.Count;

        for (int i = 0; i < linePoints.Count; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = linePoints[i];
            sphere.transform.localScale = new Vector3(1f, 1f, 1f) * 0.1f;
            sphere.transform.SetParent(transform);
            sphere.name = "Joint_" + i.ToString();
            sphereList.Add(sphere);

            lineRenderer.SetPosition(i, linePoints[i]);
        }
    }

    private void DestroyPreviousTree()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}