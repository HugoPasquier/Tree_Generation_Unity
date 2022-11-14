using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branche : MonoBehaviour
{
    [Header("Material")]
    public Material mat;

    [Header("Dimension")]
    [SerializeField]
    [Min(0)]
    float radius;

    public float radiusReduction;

    public Transform dep;
    public Transform fin;

    [Header("Pas")]
    [SerializeField]
    [Min(3)]
    int nbMeridien;

    public int nombrePas;

    [Header("Actualiser")]
    [SerializeField]
    bool update;
    public bool auto;

    Mesh msh;



    // Start is called before the first frame update
    void Start()
    {
        

        //genererBranche();
    }

    // Update is called once per frame
    void Update()
    {
        if (auto || update)
        {
            update = false;
            //genererBranche();
        }
    }
    public float genererBranche(Vector3 dep, Vector3 fin, float radius, Material mat, float radiusReduction = 0.9f, int nbMeridien = 16, int nombrePas = 5)
    {
        gameObject.AddComponent<MeshFilter>();          // Creation d'un composant MeshFilter qui peut ensuite être visualisé
        gameObject.AddComponent<MeshRenderer>();

        float r = radius;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float pas = 1.0f / (float)nombrePas;
        
        for (int j = 0; j < nombrePas + 1; j++)
        {
            //float angle = Vector3.Angle(Vector3.up, (fin.position - dep.position).normalized);
            float angle = Vector3.SignedAngle(Vector3.up, (fin - dep).normalized, Vector3.forward);

            for (int i = 0; i < nbMeridien + 1; i++)
            {
                float theta = (2 * Mathf.PI * i) / (float)nbMeridien;
                //vertices.Add(Vector3.Lerp(dep, fin, j * pas) + new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta)));
                Vector3 a = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
                a = Quaternion.AngleAxis(angle, Vector3.forward) * a;
                vertices.Add(Vector3.Slerp(dep, fin, j * pas) + a);
            }

            r *= radiusReduction;
        }

        vertices.Add(dep);    //centre couvercle dep
        vertices.Add(fin);    //centre couvercle fin


        for (int j = 0; j < nombrePas; j++)
            for (int i = 0; i < nbMeridien; i++)
            {
                triangles.Add((j * (nbMeridien)) + j + i);
                triangles.Add((j * (nbMeridien)) + j + i + 1 + nbMeridien);
                triangles.Add((j * (nbMeridien)) + j + i + 1);

                triangles.Add((j * (nbMeridien)) + j + i + 1);
                triangles.Add((j * (nbMeridien)) + j + i + 1 + nbMeridien);
                triangles.Add((j * (nbMeridien)) + j + i + 2 + nbMeridien);
            }


        // couvercles
        for (int i = 0; i < nbMeridien; i++)
        {
            triangles.Add(i);
            if (i == nbMeridien - 1)
                triangles.Add(0);
            else
                triangles.Add(i + 1);
            triangles.Add(vertices.Count - 2);

            triangles.Add(vertices.Count - 2 - nbMeridien + i);
            triangles.Add(vertices.Count - 1);
            if (i == nbMeridien - 1)
                triangles.Add(vertices.Count - 2 - nbMeridien);
            else
                triangles.Add(vertices.Count - 2 - nbMeridien + i + 1);

        }

        msh = new Mesh();                          // Création et remplissage du Mesh

        msh.vertices = vertices.ToArray();
        msh.triangles = triangles.ToArray();

        gameObject.GetComponent<MeshFilter>().mesh = msh;           // Remplissage du Mesh et ajout du matériel
        gameObject.GetComponent<MeshRenderer>().material = mat;

        return r;
    }
}
