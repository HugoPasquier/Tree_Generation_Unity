using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    struct StructBranch
    {
        public Vector3 position;
        public Quaternion rotation;
        public float radius;
        public int depth;
        public List<StructBranch> children;
        public bool hasJoint;
    }

    [Header("L-System Settings")]
    [SerializeField]
    [TextArea]
    string rules;
    [SerializeField]
    string axiom;
    [SerializeField]
    [Range(1, 6)]
    int iterations;

    //IDictionary<string, string> Presets = new Dictionary<string, string>();
    //Ruleset for a preset
    public IDictionary<string, List<(string, float)>> RulesSet = new Dictionary<string, List<(string, float)>>();

    [Header("Generation Settings")]
    [TextArea]
    public string SystemResult = "F[-F]+F[-F[-F]+F]+F[-F]+F";
    [Min(0)]
    public float angle = 45f;

    [Header("Branch parameters")]
    public float radius = 0.1f;
    [Range(1, 64)]
    public int steps = 1;
    [Range(3, 64)]
    public int meridians = 3;
    [Range(0.0f, 1.0f)]
    public float radiusReduction = 0.9f;
    private float actualRadius;
    private int depth = 0;
    [Min(0)]
    public float branchLen = 1.0f;
    [Range(0.0f, 0.99f)]
    public float branchDeltaLen;
    [Min(0)]
    public float jointConst = 2.2f;
    public Material mat;
    public Color branchColor;
    public GameObject SpherePrefab;

    [Header("Leaf parameters")]
    [Min(0)]
    public float leafMinRadius = 1;
    [Min(0)]
    public float leafMaxRadius = 5;
    public float leafDeltaScale = 0.1f;
    public Texture leavesTex;
    public Color leavesColor;
    public GameObject FeuillesPrefab;

    // Intern Variables
    private StructBranch currentBranch = new StructBranch()
    {
        position = Vector3.zero,
        rotation = Quaternion.identity,
        radius = 0,
        depth = 0,
        children = new List<StructBranch>(),
        hasJoint = false
    };

    GameObject Parent;
    GameObject ParentBranches;
    GameObject ParentLeaves;

    void InitRules()
    {
        RulesSet.Clear();
        string[] linesInRules = rules.Split('\n');
        foreach (string line in linesInRules)
        {
            List<(string, float)> r = new List<(string, float)>();
            string[] rule = line.Split('='); 
            string[] subRule = rule[1].Split('|');
            foreach (string sr in subRule) {
                string[] coefRule = sr.Split('%');
                if(coefRule.Length == 2)
                    Debug.Log(float.Parse(coefRule[1]));
                r.Add((coefRule[0], coefRule.Length == 1 ? 1.0f : float.Parse(coefRule[1]) ));
            }
            RulesSet.Add(rule[0], r);
        }
    }

    string findMatchingRule(char c, IDictionary<string, List<(string, float)>> rs) {
        List<(string, float)> rules = new List<(string, float)>();
        string rule = null;
        
        //try to find a list of rules matching the character
        if (rs.TryGetValue(c.ToString(), out rules)) {
            // if there is only one rule, return it
            if (rules.Count == 1){
                return rules[0].Item1;
            }

            // Checking if the sum of all rule coeff equal 1
            float sum = 0;
            foreach((string, float) r in rules){
                sum += r.Item2;
            }
            if (sum != 1.0f)
                return null;

            //randomly get a rule in the list
            float choice = Random.Range(0f, 1f);
            sum = 0;
            foreach ((string, float) possibility in rules){
                sum += possibility.Item2;
                if (choice <= sum){
                    rule = possibility.Item1;
                    break;
                }
            }
        }
        return rule;
    }

    //apply rules to sentence
    string ApplyRulesToSentence(string sentence, IDictionary<string, List<(string, float)>> rs) {
        string newSentence = "";
        string rule = null;
        for (int i = 0; i < sentence.Length; i++) {
            char c = sentence[i];
            rule = findMatchingRule(c, rs);
            if (rule != null) {
                newSentence += rule;
            } else {
                newSentence += c;
            }

        }
        return newSentence;
    }



    //take an axiom and an iterator to return a sentence
    string ApplyRules(string axiom, int iterator, IDictionary<string, List<(string, float)>> rs) {

        string sentence = axiom;
        for (int i = 0; i < iterator; i++) {
            sentence = ApplyRulesToSentence(sentence, rs);
        }
        return sentence;
    }

    void LeavesGeneration(StructBranch b, bool changeMat = true)
    {
        if (b.children.Count == 0)
        {
            GameObject leaf = Instantiate(FeuillesPrefab, b.position, b.rotation);
            float r = Random.Range(leafMinRadius, leafMaxRadius);
            leaf.transform.localScale = new Vector3(r * Random.Range(1.0f - leafDeltaScale, 1.0f + leafDeltaScale),
                                                    r * Random.Range(1.0f - leafDeltaScale, 1.0f + leafDeltaScale),
                                                    r * Random.Range(1.0f - leafDeltaScale, 1.0f + leafDeltaScale));
            leaf.transform.SetParent(ParentLeaves.transform);
            if(changeMat)
                foreach(Transform t in leaf.transform)
                    foreach(Transform t1 in t)
                        foreach (Transform t2 in t1)
                        {
                            Material m = t2.GetComponent<Renderer>().material;
                            m.SetColor("_Color", leavesColor);
                            m.SetTexture("_MainTex", leavesTex);
                        }
        }
        else
            foreach (StructBranch child in b.children)
                LeavesGeneration(child, changeMat);
    }

    public void DrawTree()
    {
        mat.color = branchColor;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        if (Parent != null)
            Destroy(Parent.gameObject);
        if (ParentBranches != null)
            Destroy(ParentBranches.gameObject);
        if (ParentLeaves != null)
            Destroy(Parent.gameObject);

        Parent = new GameObject("Parent");
        ParentBranches = new GameObject("Branch");
        ParentLeaves = new GameObject("Leaves");

        ParentBranches.transform.SetParent(Parent.transform);
        ParentLeaves.transform.SetParent(Parent.transform);

        StructBranch initBranch = currentBranch;
        actualRadius = radius;
        Stack<StructBranch> positionStack = new Stack<StructBranch>();

        int branchIndex = 0;
        foreach (char c in SystemResult)
        {
            switch (c)
            {
                case 'F':
                    GameObject branch = new GameObject("Branch_" + branchIndex);
                    branch.transform.parent = ParentBranches.transform;

                    branchIndex++;

                    float realBranchLen = branchLen * Random.Range(1.0f - branchDeltaLen, 1.0f + branchDeltaLen);

                    Branche b = branch.AddComponent<Branche>();

                    actualRadius = b.genererBranche(gameObject.transform.position, gameObject.transform.position + gameObject.transform.up * realBranchLen, actualRadius, mat, radiusReduction, meridians, steps);
                    depth++;
                    gameObject.transform.position += gameObject.transform.up * realBranchLen;

                    StructBranch newNode = new StructBranch();
                    newNode.children = new List<StructBranch>();
                    newNode.position = gameObject.transform.position;
                    newNode.rotation = gameObject.transform.rotation;
                    newNode.radius = actualRadius;
                    newNode.depth = depth;
                    newNode.hasJoint = false;

                    currentBranch.children.Add(newNode);
                    currentBranch = newNode;

                    break;
                case '+':
                    gameObject.transform.Rotate(new Vector3(0, 0, angle), Space.Self);
                    gameObject.transform.Rotate(new Vector3(0, Random.Range(0.0f, 180.0f), 0), Space.World);
                    break;
                case '-':
                    gameObject.transform.Rotate(new Vector3(0, 0, -angle), Space.Self);
                    gameObject.transform.Rotate(new Vector3(0, Random.Range(0.0f, 180.0f), 0), Space.World);
                    break;
                case '[':
                    positionStack.Push(currentBranch);
                    if (!currentBranch.hasJoint)
                    {
                        GameObject newSphere = Instantiate(SpherePrefab, gameObject.transform.position, transform.rotation);
                        newSphere.transform.localScale = new Vector3(actualRadius * jointConst, actualRadius * jointConst, actualRadius * jointConst);
                        newSphere.transform.SetParent(ParentBranches.transform);
                        newSphere.GetComponent<Renderer>().material.color = branchColor;
                        currentBranch.hasJoint = true;
                    }
                    break;
                case ']':
                    currentBranch = positionStack.Pop();
                    gameObject.transform.position = currentBranch.position;
                    gameObject.transform.rotation = currentBranch.rotation;
                    actualRadius = currentBranch.radius;
                    depth = currentBranch.depth;
                    break;
            }
        }

        LeavesGeneration(initBranch, true);
    }

    public void UpdateSystemResult()
    {
        InitRules();
        SystemResult = ApplyRules(axiom, iterations, RulesSet);
    }

    // Start is called before the first frame update
    void Start()
    {
        InitRules();
        UpdateSystemResult();
        DrawTree();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
