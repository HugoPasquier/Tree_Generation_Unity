using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Metadata;

public class TreeDrawer : MonoBehaviour
{
    struct StructBranch {
        public Vector3 position;
        public Quaternion rotation;
        public float radius;
        public int depth;
        public List<StructBranch> children;
    }





    // Site to draw the tree : https://piratefsh.github.io/p5js-art/public/lsystems/
    // Test Tree : F[-F[-F[-F]+F]+F[-F]+F]+F[-F[-F]+F]+F[-F]+F
    [Header("Generation Settings")]
    [TextArea]
    public string SystemResult = "F[-F]+F[-F[-F]+F]+F[-F]+F"; public float Theta = 45f;
    public float Phi = 60f;

    public bool next;
    public bool isStepByStep = true;

    // Line renderer parameters
    private float LineWidth = 0.1f;
    
    [Header("Branch parameters")]
    public float radius = 0.1f;
    public float radiusReduction = 0.9f;
    private float actualRadius;
    private int depth = 0;
    public float branchLen = 1.0f;
    public float branchDeltaLen;
    public Material mat;
    public GameObject SpherePrefab;
    public float jointConst = 2.2f;

    [Header("Leaf parameters")]
    public float leafMinRadius = 1;
    public float leafMaxRadius = 5;
    public float leafDeltaScale = 0.1f;
    public GameObject FeuillesPrefab;


    // Intern Variables
    private StructBranch currentBranch = new StructBranch() { position = Vector3.zero, 
                                                            rotation = Quaternion.identity, 
                                                            radius = 0, 
                                                            depth = 0,
                                                            children = new List<StructBranch>()};


private void LeavesGeneration(StructBranch b) {
        if (b.children.Count == 0) {
            GameObject leaf = Instantiate(FeuillesPrefab, b.position, b.rotation);
            float r = Random.Range(leafMinRadius, leafMaxRadius);
            leaf.transform.localScale = new Vector3(r * Random.Range(1.0f - leafDeltaScale, 1.0f + leafDeltaScale),
                                                    r * Random.Range(1.0f - leafDeltaScale, 1.0f + leafDeltaScale),
                                                    r * Random.Range(1.0f - leafDeltaScale, 1.0f + leafDeltaScale));
            // Scale handling
        } else {
            foreach (StructBranch child in b.children) {
                LeavesGeneration(child);
            }
        }
    }
    
public void DrawTree() {
        StructBranch initBranch = currentBranch;
        actualRadius = radius;
        if (!isStepByStep) {
            
            GameObject Parent = new GameObject("Parent");

            Stack<StructBranch> positionStack = new Stack<StructBranch>();
            //gameObject.transform.forward = gameObject.transform.up;
            

            int branchIndex = 0;
            foreach (char c in SystemResult) {
                
                switch (c) {
                    case 'F':

                        GameObject branch = new GameObject("Branch_" + branchIndex);
                        branch.transform.parent = Parent.transform;

                        branchIndex++;

                        float realBranchLen = branchLen * Random.Range(1.0f - branchDeltaLen, 1.0f + branchDeltaLen);

                        //Render withe Line Renderer
                        LineRenderer b_lr = branch.AddComponent<LineRenderer>();
                        b_lr.startWidth = LineWidth;
                        b_lr.endWidth = LineWidth;
                        b_lr.positionCount = 2;
                        b_lr.SetPosition(0, gameObject.transform.position);
                        b_lr.SetPosition(1, gameObject.transform.position + gameObject.transform.up * realBranchLen);

                        Branche b = branch.AddComponent<Branche>();
                        
                        actualRadius = b.genererBranche(gameObject.transform.position, gameObject.transform.position + gameObject.transform.up * realBranchLen, actualRadius, mat, radiusReduction, 16, 1 );
                        depth++;
                        gameObject.transform.position += gameObject.transform.up * realBranchLen;

                        StructBranch newNode = new StructBranch();
                        newNode.children = new List<StructBranch>();
                        newNode.position = gameObject.transform.position;
                        newNode.rotation = gameObject.transform.rotation;
                        newNode.radius = actualRadius;
                        newNode.depth = depth;

                        currentBranch.children.Add(newNode);
                        currentBranch = newNode;
                        
                        //Create leaves
                        //if (depth >= MaxDepth) {
                        //    GameObject feuilles = Instantiate(FeuillesPrefab, gameObject.transform.position, Quaternion.identity);
                        //    float s = Random.Range(minLeafRadius, maxLeafRadius);
                        //    feuilles.transform.localScale = new Vector3(s, s, s);
                        //}

                        break;
                    case '+':
                        gameObject.transform.Rotate(new Vector3(0, 0, Theta), Space.Self);
                        float r1 = Random.Range(0.0f, 180.0f);
                        gameObject.transform.Rotate(new Vector3(0, r1, 0), Space.World);
                        Debug.Log("R1 : " + r1);
                        break;
                    case '-':
                        gameObject.transform.Rotate(new Vector3(0, 0, -Theta), Space.Self);
                        float r2 = Random.Range(0.0f, 180.0f );
                        gameObject.transform.Rotate(new Vector3(0, r2, 0), Space.World);
                        Debug.Log("R2 : " + r2);
                        break;
                    case '[':
                        positionStack.Push(currentBranch);
                        Instantiate(SpherePrefab, gameObject.transform.position, transform.rotation).transform.localScale = new Vector3(actualRadius * jointConst, actualRadius * jointConst, actualRadius * jointConst);
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

            LeavesGeneration(initBranch);
            
            //MeshFilter[] meshFilters = Parent.GetComponentsInChildren<MeshFilter>();
            //CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            //
            //int i = 0;
            //while (i < meshFilters.Length) {
            //    combine[i].mesh = meshFilters[i].sharedMesh;
            //    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            //    meshFilters[i].gameObject.SetActive(false);
            //
            //    i++;
            //}
            //// Destroy Children
            //foreach (Transform child in Parent.transform) {
            //    GameObject.Destroy(child.gameObject);
            //}
            //
            //MeshFilter meshFilterParent = Parent.AddComponent<MeshFilter>();
            //Parent.AddComponent<MeshRenderer>().material = mat;
            //
            //
            //meshFilterParent.mesh = new Mesh();
            //meshFilterParent.mesh.CombineMeshes(combine);
            //meshFilterParent.mesh.Optimize();
        } else {
            StartCoroutine(stepByStep());
        }
    }

    IEnumerator stepByStep() {

        GameObject Parent = new GameObject("Parent");

        Stack<StructBranch> positionStack = new Stack<StructBranch>();
        gameObject.transform.forward = gameObject.transform.up;

        int branchIndex = 0;
            foreach (char c in SystemResult) {
            Debug.Log(c);
            next = false;
            switch (c) {
                    case 'F':

                        GameObject branch = new GameObject("Branch_" + branchIndex);
                        branch.transform.parent = Parent.transform;

                        branchIndex++;
                        // Render withe Line Renderer

                        LineRenderer b_lr = branch.AddComponent<LineRenderer>();
                        b_lr.startWidth = LineWidth;
                        b_lr.endWidth = LineWidth;
                        b_lr.positionCount = 2;
                        b_lr.SetPosition(0, gameObject.transform.position);
                        b_lr.SetPosition(1, gameObject.transform.position + gameObject.transform.forward * branchLen);

                        Branche b = branch.AddComponent<Branche>();
                        actualRadius = b.genererBranche(gameObject.transform.position, gameObject.transform.position + gameObject.transform.forward * branchLen, actualRadius, mat, radiusReduction, 16, 1);
                        depth++;

                        gameObject.transform.position += gameObject.transform.forward * branchLen;

                        // Create leaves
                        //if (depth >= MaxDepth) {
                        //    GameObject feuilles = Instantiate(FeuillesPrefab, gameObject.transform.position, Quaternion.identity);
                        //    float s = Random.Range(minLeafRadius, maxLeafRadius);
                        //    feuilles.transform.localScale = new Vector3(s, s, s);
                        //}

                        break;
                    case '+':
                        gameObject.transform.Rotate(0, Theta, 0);
                        gameObject.transform.Rotate(Vector3.up, Random.Range(0, 360));
                        break;
                    case '-':
                        gameObject.transform.Rotate(0, -Theta, 0);
                        gameObject.transform.Rotate(Vector3.up, Random.Range(0, 360));
                        break;
                    case '[':
                        positionStack.Push(new StructBranch() { position = gameObject.transform.position, rotation = gameObject.transform.rotation, radius = actualRadius, depth = depth });
                        break;
                    case ']':
                        StructBranch newTransform = positionStack.Pop();
                        gameObject.transform.position = newTransform.position;
                        gameObject.transform.rotation = newTransform.rotation;
                        actualRadius = newTransform.radius;
                        depth = newTransform.depth;
                        break;
                }

            while (!next) 
                    yield return null;
            }
                
                
        }

    // Start is called before the first frame update
    void Start() {
        DrawTree();
    }


    

}

   



