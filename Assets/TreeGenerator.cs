using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    [Header("Save system")]
    [SerializeField]
    string loadFile;
    [SerializeField]
    string saveFile;

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
    [Min(0)]
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
    [Min(0)]
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
            string[] rule = line.Split('=');
            List<(string, float)> r = new List<(string, float)>();
            string[] dif = rule[1].Split('|');
            foreach (string d in dif) {
                string[] dif2 = d.Split('%');
                if(dif2.Length == 2)
                    Debug.Log(float.Parse(dif2[1]));
                r.Add((dif2[0], dif2.Length == 1 ? 1.0f : float.Parse(dif2[1]) ));
            }
            RulesSet.Add(rule[0], r);
        }
    }

    string findMatchingRule(char c, IDictionary<string, List<(string, float)>> rs) {
        List<(string, float)> rules = new List<(string, float)>();
        string rule = null;
        float minDistance = 0;

        //try to find a list of rules matching the character
        if (rs.TryGetValue(c.ToString(), out rules)) {
            float choice = Random.Range(0f, 1f);
            minDistance = Mathf.Abs(Mathf.Abs(rules[0].Item2) - Mathf.Abs(choice));
            //randomly get a rule in the list
            foreach ((string, float) possibility in rules) {
                if (Mathf.Abs(Mathf.Abs(possibility.Item2) - Mathf.Abs(choice)) <= minDistance) {
                    rule = possibility.Item1;
                }
            }
            return rule;
        }
        return null;
        /*
        List<string> rules = new List<string,float>();
        //try to find a rule matching the character
        if(rs.TryGetValue(c.ToString(), out rules))
        {
            print(rules);
            return rules;
        }
        return null;*/
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

    public void savePresetInFile()
    {
        StreamWriter sr = File.CreateText("Assets//" + saveFile);

        sr.WriteLine("Rules:" + rules.Replace("\n", "<b>"));
        sr.WriteLine("Axiom:" + axiom);
        sr.WriteLine("Iterations:" + iterations);
        sr.WriteLine("Angle:" + angle);
        sr.WriteLine("Radius:" + radius);
        sr.WriteLine("Steps:" + steps);
        sr.WriteLine("Meridians:" + meridians);
        sr.WriteLine("RadiusRed:" + radiusReduction);
        sr.WriteLine("Len:" + branchLen);
        sr.WriteLine("LenDelta:" + branchDeltaLen);
        sr.WriteLine("JointConst:" + jointConst);
        sr.WriteLine("BranchCol:" + branchColor);
        sr.WriteLine("LeafMinRad:" + leafMinRadius);
        sr.WriteLine("LeafMaxRad:" + leafMaxRadius);
        sr.WriteLine("LeafDeltaScale:" + leafDeltaScale);
        sr.WriteLine("LeafColor:" + leavesColor);
        sr.Close();

        Debug.Log("Current preset has been saved in" + loadFile + "." );
    }

    public void loadPresetFromFile()
    {
        FileInfo theSourceFile = new FileInfo("Assets/" + loadFile);
        StreamReader reader = theSourceFile.OpenText();
        string readline;
        string[] readlines;

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if(readlines[0] == "Rules")
            rules = readlines[1].Replace("<b>", "\n");
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "Axiom")
            axiom = readlines[1];
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "Iterations")
            iterations = int.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "Angle")
            angle = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "Radius")
            radius = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "Steps")
           steps = int.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "Meridians")
            meridians = int.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "RadiusRed")
            radiusReduction = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "Len")
            branchLen = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "LenDelta")
            branchDeltaLen = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "JointConst")
            jointConst = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "BranchCol")
        {
            string[] rgba = readlines[1].Substring(5, readlines[1].Length - 6).Split(", ");
            branchColor = new Color(float.Parse(rgba[0].Replace('.', ',')), float.Parse(rgba[1].Replace('.', ',')), float.Parse(rgba[2].Replace('.', ',')), float.Parse(rgba[3].Replace('.', ',')));
        }
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "LeafMinRad")
            leafMinRadius = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "LeafMaxRad")
            leafMaxRadius = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "LeafDeltaScale")
            leafDeltaScale = float.Parse(readlines[1]);
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        readline = reader.ReadLine();
        readlines = readline.Split(":");
        if (readlines[0] == "LeafColor")
        {
            string[] rgba = readlines[1].Substring(5, readlines[1].Length - 6).Split(", ");
            leavesColor = new Color(float.Parse(rgba[0].Replace('.', ',')), float.Parse(rgba[1].Replace('.', ',')), float.Parse(rgba[2].Replace('.', ',')), float.Parse(rgba[3].Replace('.', ',')));
        }
        else
        {
            Debug.Log(loadFile + " is not valid.");
            return;
        }

        reader.Close();
        Debug.Log(loadFile + " has been loaded.");
    }

    public void loadPreset1()
    {
        rules = "F=FF\nX=F[+X][-X]FX";
        axiom = "X";
        iterations = 3;
        angle = 30;
        radius = 1.0f;
        steps = 64;
        meridians = 64;
        radiusReduction = 0.998f;
        branchLen = 2;
        branchDeltaLen = 0.684f;
        jointConst = 2.0f;
        Color newCol;
        if (ColorUtility.TryParseHtmlString("#413123", out newCol))
            branchColor = newCol;
        leafMinRadius = 2;
        leafMaxRadius = 8;
        leafDeltaScale = 0.1f;
        if (ColorUtility.TryParseHtmlString("#7FB143", out newCol))
            leavesColor = newCol;

        Debug.Log("Preset 1 has been loaded.");
    }

    public void loadPreset2()
    {
        rules = "F=F[+F]F[-F]F";
        axiom = "F";
        iterations = 2;
        angle = 25;
        radius = 0.8f;
        steps = 4;
        meridians = 32;
        radiusReduction = 0.94f;
        branchLen = 3;
        branchDeltaLen = 0.25f;
        jointConst = 2.0f;
        Color newCol;
        if (ColorUtility.TryParseHtmlString("#B7B6B6", out newCol))
            branchColor = newCol;
        leafMinRadius = 1;
        leafMaxRadius = 3;
        leafDeltaScale = 0.1f;
        if (ColorUtility.TryParseHtmlString("#FF9420", out newCol))
            leavesColor = newCol;

        Debug.Log("Preset 2 has been loaded.");
    }


}
