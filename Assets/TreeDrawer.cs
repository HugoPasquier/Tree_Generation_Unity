using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeDrawer : MonoBehaviour
{
    struct Branch {
        public Vector3 position;
        public Quaternion rotation;
        public float radius;
        public int depth;
    }

    

    // Site to draw the tree : https://piratefsh.github.io/p5js-art/public/lsystems/


    [TextArea]
    public string SystemResult = "F[-F]+F[-F[-F]+F]+F[-F]+F"; public float Theta = 45f;
    public float Phi = 60f;
    public List<GameObject> branches;
    public Material mat;
    
    // Line renderer parameters
    public float LineWidth = 0.1f;
    public float radius = 0.1f;
    private float actualRadius = 0.1f;
    private int depth = 0;

    public void DrawTree(){

        GameObject Parent = new GameObject("Parent");

        Stack<Branch> positionStack = new Stack<Branch>();
        gameObject.transform.forward = gameObject.transform.up;

        int branchIndex = 0;
        foreach ( char c in SystemResult){
            switch(c){
                case 'F':
                    
                    GameObject branch = new GameObject("Branch_" + branchIndex);
                    branch.transform.parent = Parent.transform;

                    branchIndex++;
                    //LineRenderer b_lr = branch.AddComponent<LineRenderer>();

                    //b_lr.startWidth = LineWidth;
                    //b_lr.endWidth = LineWidth;
                    //b_lr.positionCount = 2;
                    //b_lr.SetPosition(0, gameObject.transform.position);
                    //b_lr.SetPosition(1, gameObject.transform.position + gameObject.transform.forward);

                    Branche b = branch.AddComponent<Branche>();
                    actualRadius = b.genererBranche(gameObject.transform.position, gameObject.transform.position + gameObject.transform.forward, actualRadius, mat);
                    depth++;

                    gameObject.transform.position += gameObject.transform.forward;
                    break;
                case '+':
                    gameObject.transform.Rotate(0, Theta, 0);
                    gameObject.transform.Rotate(transform.right, Random.value * Phi);
                    break;
                case '-':
                    gameObject.transform.Rotate(0, -Theta, 0);
                    gameObject.transform.Rotate(transform.right, Random.value * -Phi);
                    break;
                case '[':
                    positionStack.Push(new Branch() {position = gameObject.transform.position, rotation = gameObject.transform.rotation, radius = actualRadius, depth = depth});
                    break;
                case ']':
                    Branch newTransform = positionStack.Pop();
                    gameObject.transform.position = newTransform.position;
                    gameObject.transform.rotation = newTransform.rotation;
                    actualRadius = newTransform.radius;
                    depth = newTransform.depth;
                    break;
            }
        }

        
        
        MeshFilter[] meshFilters = Parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        // Destroy Children
        foreach (Transform child in Parent.transform) {
            GameObject.Destroy(child.gameObject);
        }

        MeshFilter meshFilterParent = Parent.AddComponent<MeshFilter>();
        Parent.AddComponent<MeshRenderer>().material = mat;


        meshFilterParent.mesh = new Mesh();
        meshFilterParent.mesh.CombineMeshes(combine);
        meshFilterParent.mesh.Optimize();

    }

    // Start is called before the first frame update
    void Start()
    {
        DrawTree();
    }

}



