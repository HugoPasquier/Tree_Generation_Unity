using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPreview : MonoBehaviour
{

    [SerializeField]
    Transform camT;

    [SerializeField]
    Vector3 offSetCam;

    [SerializeField]
    float rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        camT.transform.localPosition = offSetCam;

        transform.Rotate(transform.up, rotationSpeed * Time.deltaTime);
    }
}
