using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Models;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Utils;

public class TreeConstructor : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private float stepDistance = 1f;
    [SerializeField] private float angle = 25f;
    [SerializeField] private GameObject fruit;
    [SerializeField] private float baseWidth = 0.5f;
    [SerializeField] private float widthDecay = 0.7f;
    [SerializeField] private float taperFactor = 0.5f;
    [SerializeField] private float growthRate = 0.04f;

    [SerializeField] private int currentGeneration = 0;
    [SerializeField] private int maxGenerations = 5;
    [SerializeField] public float growthPercent = 0f;
    public float GrowthPercent => growthPercent;
    public int CurrentGeneration => currentGeneration;

    private List<LineRenderer> lineRendererPool = new List<LineRenderer>();
    private List<GameObject> fruitPool = new List<GameObject>();

    private string _dna = "X";

    private readonly List<Branch> _branches = new();
    private readonly Stack<State> _stateStack = new();

    public void Start()
    {
        GenerateTree();
    }

    private void Update()
    {
        if (growthPercent < 1f)
        {
            float mod = currentGeneration + growthPercent;
            growthPercent += growthRate / (mod > 0 ? mod : 1);
            Profiler.BeginSample("BuildTree");
            BuildTree();
            Profiler.EndSample();
            Profiler.BeginSample("DrawTree");
            DrawTree();
            Profiler.EndSample();
        }
        else
        {
            GenerateNextDna();
            growthPercent = 0f;
            BuildTree();
            DrawTree();
        }
    }

    public void GenerateTree()
    {
        _dna = "X";
        currentGeneration = 0;
        growthPercent = 0f;
        GenerateNextDna();
    }

    public void GenerateTreeInstantly()
    {
        _dna = "X";
        currentGeneration = 0;
        growthPercent = 0f;
        for (var i = 0; i < 5; i++)
        {
            _dna = GenerateDna();
        }

        GenerateNextDna();
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
                next += Helper.ChooseOne(Rules.F);
                ;
            }
            else if (c.ToString() == "(" || c.ToString() == ")")
            {
                // Ignore parentheses
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
        _branches.Add(new Branch
            { Depth = 0, Points = new List<Vector3>(), Fruits = new List<Fruit>() }); // Main branch
        int currentBranchIndex = 0;
        Vector3 position = Vector3.zero;
        Vector3 heading = Vector3.up;
        _stateStack.Clear();

        // Initialize main branch with starting position
        _branches[currentBranchIndex].Points.Add(position);
        bool lerpOn = false;

        foreach (char c in _dna)
        {
            switch (c)
            {
                case '(':
                    lerpOn = true;
                    continue;
                case ')':
                    lerpOn = false;
                    continue;
                case 'A':
                    if (lerpOn)
                        _branches[currentBranchIndex].Fruits.Add(new Fruit
                        {
                            Position = _branches[currentBranchIndex].Points.Count - 1, FruitType = FruitType.Apple,
                            Lerp = growthPercent
                        });
                    else
                        _branches[currentBranchIndex].Fruits.Add(new Fruit
                        {
                            Position = _branches[currentBranchIndex].Points.Count - 1, FruitType = FruitType.Apple,
                            Lerp = 1f
                        });
                    break;
                case 'B':
                    if (lerpOn)
                        _branches[currentBranchIndex].Fruits.Add(new Fruit
                        {
                            Position = _branches[currentBranchIndex].Points.Count - 1, FruitType = FruitType.Banana,
                            Lerp = growthPercent
                        });
                    else
                        _branches[currentBranchIndex].Fruits.Add(new Fruit
                        {
                            Position = _branches[currentBranchIndex].Points.Count - 1, FruitType = FruitType.Banana,
                            Lerp = 1f
                        });
                    break;
                case 'F':
                    float step = lerpOn ? stepDistance * growthPercent : stepDistance;
                    Vector3 nextPosition = position + heading * step;
                    _branches[currentBranchIndex].Points.Add(nextPosition);
                    position = nextPosition;
                    break;
                case '+':
                    float rotAngle = lerpOn ? angle * growthPercent : angle;
                    heading = Quaternion.AngleAxis(rotAngle, Vector3.forward) * heading;
                    break;
                case '-':
                    float negAngle = lerpOn ? -angle * growthPercent : -angle;
                    heading = Quaternion.AngleAxis(negAngle, Vector3.forward) * heading;
                    break;
                case '[':
                    _stateStack.Push(new State { Pos = position, Heading = heading, BranchIndex = currentBranchIndex });
                    _branches.Add(new Branch
                    {
                        Depth = _branches[currentBranchIndex].Depth + 1, Points = new List<Vector3>(),
                        Fruits = new List<Fruit>()
                    });
                    currentBranchIndex = _branches.Count - 1;
                    _branches[currentBranchIndex].Points.Add(position);
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

    private void DrawTree()
    {
        int branchIndex = 0;
        foreach (var branch in _branches)
        {
            if (branch.Points.Count < 2) continue;

            LineRenderer lineRenderer;
            // Reuse existing LineRenderer if available
            if (branchIndex < lineRendererPool.Count)
            {
                lineRenderer = lineRendererPool[branchIndex];
            }
            else
            {
                lineRenderer = Instantiate(lineRendererPrefab, transform);
                lineRendererPool.Add(lineRenderer);
            }

            // Update LineRenderer properties
            lineRenderer.gameObject.name = $"Branch_{branch.Depth}_{branchIndex}";
            lineRenderer.positionCount = branch.Points.Count;
            lineRenderer.SetPositions(branch.Points.ToArray());

            Profiler.BeginSample("MathOps");
            float startWidth = baseWidth * Mathf.Pow(widthDecay, branch.Depth);
            float endWidth = startWidth * taperFactor;
            lineRenderer.widthMultiplier = startWidth;
            lineRenderer.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, endWidth / startWidth);
            Profiler.EndSample();

            branchIndex++;
        }

        int fruitIndex = 0;
        foreach (var branch in _branches)
        {
            foreach (var f in branch.Fruits)
            {
                GameObject fruitObj;
                SpriteRenderer sr;

                if (fruitIndex < fruitPool.Count && fruitPool[fruitIndex] != null)
                {
                    fruitObj = fruitPool[fruitIndex];
                    sr = fruitObj.GetComponent<SpriteRenderer>();
                }
                else
                {
                    fruitObj = Instantiate(fruit, transform);
                    sr = fruitObj.GetComponent<SpriteRenderer>();
                    if (fruitIndex >= fruitPool.Count)
                    {
                        fruitPool.Add(fruitObj);
                    }
                    else
                    {
                        fruitPool[fruitIndex] = fruitObj;
                    }
                }

                fruitObj.transform.localPosition = branch.Points[f.Position];
                fruitObj.transform.localScale = Vector3.one * f.Lerp * 0.5f;
                sr.color = f.FruitType == FruitType.Apple ? Color.red : Color.yellow;
                sr.enabled = true;
                fruitIndex++;
            }
        }
    }

    private void GenerateNextDna()
    {
        if (currentGeneration >= maxGenerations)
        {
            _dna = "X";
            currentGeneration = 0;
            growthPercent = 0f;
            return;
        }

        _dna = GenerateDna();
        currentGeneration++;
    }
}