using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class TreeConstructor : MonoBehaviour
{
    [SerializeField] private GameObject lineRendererPrefab;
    [SerializeField] private int iterations = 1;
    [SerializeField] private float stepDistance = 1f;
    [SerializeField] private float angle = 25f;
    [SerializeField] private GameObject fruit;
    [SerializeField] private float baseWidth = 0.5f;
    [SerializeField] private float widthDecay = 0.7f;
    [SerializeField] private float taperFactor = 0.5f;

    private string _dna = "X";

    private readonly List<Branch> _branches = new();
    private readonly Stack<State> _stateStack = new();

    public void Start()
    {
        GenerateTree();
    }

    public void GenerateTree()
    {
        _dna = "X";
        for (int i = 0; i < iterations; i++)
        {
            _dna = GenerateDna();
        }

        DestroyPreviousTree();
        BuildTree();
        DrawTree();
    }

    public string GenerateDna()
    {
        var next = "";
        foreach (var c in _dna)
        {
            if (c.ToString() == nameof(Rules.X))
            {
                next += Helper.ChooseOne(Rules.X);
            }
            else if (c.ToString() == nameof(Rules.F))
            {
                next += Helper.ChooseOne(Rules.F);;
            }
            else
            {
                next += c;
            }
        }

        Debug.Log(next);
        return next;
    }

    private void BuildTree()
    {
        _branches.Clear();
        _branches.Add(new Branch { Depth = 0, Points = new List<Vector3>(), Fruits = new List<Fruit>() }); // Main branch
        int currentBranchIndex = 0;
        Vector3 position = Vector3.zero;
        Vector3 heading = Vector3.up;
        _stateStack.Clear();

        // Initialize main branch with starting position
        _branches[currentBranchIndex].Points.Add(position);

        foreach (char c in _dna)
        {
            switch (c)
            {
                case 'A':
                    _branches[currentBranchIndex].Fruits.Add(new Fruit { Position = _branches[currentBranchIndex].Points.Count - 1, FruitType = FruitType.Apple });
                    break;

                case 'B':
                    _branches[currentBranchIndex].Fruits.Add(new Fruit { Position = _branches[currentBranchIndex].Points.Count - 1, FruitType = FruitType.Banana });
                    break;

                case 'F':
                    Vector3 nextPosition = position + heading * stepDistance;
                    _branches[currentBranchIndex].Points.Add(nextPosition);
                    position = nextPosition;
                    break;

                case '+':
                    heading = Quaternion.AngleAxis(angle, Vector3.forward) * heading;
                    break;

                case '-':
                    heading = Quaternion.AngleAxis(-angle, Vector3.forward) * heading;
                    break;

                case '[':
                    var currentState = new State()
                    {
                        Pos = position,
                        Heading = heading,
                        BranchIndex = currentBranchIndex
                    };
                    _stateStack.Push(currentState);
                    _branches.Add(new Branch() { Depth = _branches[currentBranchIndex].Depth + 1, Points = new List<Vector3>(), Fruits = new List<Fruit>() });
                    currentBranchIndex = _branches.Count - 1; // Increase branch index
                    _branches[currentBranchIndex].Points.Add(position); // Start new branch
                    break;

                case ']':
                    if (_stateStack.Count > 0)
                    {
                        var state = _stateStack.Pop();
                        position = state.Pos;
                        heading = state.Heading;
                        currentBranchIndex = state.BranchIndex;
                    }
                    break;
            }
        }
    }

    public void DrawTree()
    {
        foreach (var branch in _branches)
        {
            if (branch.Points.Count < 2) continue; // Skip branches with less than 2 points

            GameObject lineObj = Instantiate(lineRendererPrefab, transform);
            lineObj.name = "Branch_" + branch.Depth + "_" + _branches.IndexOf(branch);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
            lineRenderer.positionCount = branch.Points.Count;
            lineRenderer.SetPositions(branch.Points.ToArray());

            float startWidth = baseWidth * Mathf.Pow(widthDecay, branch.Depth);
            float endWidth = startWidth * taperFactor;
            lineRenderer.widthMultiplier = startWidth;
            lineRenderer.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, endWidth / startWidth);

            foreach (var f in branch.Fruits)
            {
                Vector3 position = branch.Points[f.Position];
                GameObject fruitObj = Instantiate(fruit, transform);
                fruitObj.transform.localPosition = position;
                SpriteRenderer sr = fruitObj.GetComponent<SpriteRenderer>();
                if (f.FruitType == FruitType.Apple)
                {
                    sr.color = Color.red;
                    fruitObj.name = "FruitA";
                }
                else if (f.FruitType == FruitType.Banana)
                {
                    sr.color = Color.yellow;
                    fruitObj.name = "FruitB";
                }
                Debug.Log(fruitObj.name + " " + position);
            }
        }
    }

    private void DestroyPreviousTree()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }
}