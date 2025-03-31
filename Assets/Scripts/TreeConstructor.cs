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

    private string _dna = "X";

    private readonly List<List<Vector3>> _branches = new();
    private readonly Stack<(Vector3 pos, Vector3 heading, int branchIndex)> _stateStack = new();

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
        _branches.Add(new List<Vector3>()); // Main branch
        int currentBranchIndex = 0;
        Vector3 position = Vector3.zero;
        Vector3 heading = Vector3.up;
        _stateStack.Clear();

        // Initialize main branch with starting position
        _branches[currentBranchIndex].Add(position);

        foreach (char c in _dna)
        {
            switch (c)
            {
                case 'A':
                    var fruitA = Instantiate(fruit, transform);
                    fruitA.transform.localPosition = position;
                    fruitA.GetComponent<SpriteRenderer>().color = Color.red;
                    fruitA.name = "FruitA";
                    Debug.Log(fruitA.name + " " + position);
                    break;

                case 'B':
                    var fruitB = Instantiate(fruit, transform);
                    fruitB.transform.localPosition = position;
                    fruitB.GetComponent<SpriteRenderer>().color = Color.blue;
                    fruitB.name = "FruitB";
                    Debug.Log(fruitB.name + " " + position);
                    break;

                case 'F':
                    Vector3 nextPosition = position + heading * stepDistance;
                    _branches[currentBranchIndex].Add(nextPosition);
                    position = nextPosition;
                    break;

                case '+':
                    heading = Quaternion.AngleAxis(angle, Vector3.forward) * heading;
                    break;

                case '-':
                    heading = Quaternion.AngleAxis(-angle, Vector3.forward) * heading;
                    break;

                case '[':
                    _stateStack.Push((position, heading, currentBranchIndex));
                    _branches.Add(new List<Vector3>());
                    currentBranchIndex = _branches.Count - 1;
                    _branches[currentBranchIndex].Add(position); // Start new branch
                    break;

                case ']':
                    if (_stateStack.Count > 0)
                    {
                        var state = _stateStack.Pop();
                        position = state.pos;
                        heading = state.heading;
                        currentBranchIndex = state.branchIndex;
                    }
                    break;
            }
        }
    }

    public void DrawTree()
    {
        foreach (var branch in _branches)
        {
            if (branch.Count < 2) continue; // Skip branches with less than 2 points

            GameObject lineObj = Instantiate(lineRendererPrefab, transform);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
            lineRenderer.positionCount = branch.Count;
            lineRenderer.SetPositions(branch.ToArray());
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