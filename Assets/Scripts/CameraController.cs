using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; //what to follow
    public float smoothing = 5f; //camera speed
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position;
    }

    void FixedUpdate()
    {
        if (target != null) {
            Vector3 targetCamPos = target.position + offset;
            transform.position = Vector3.Lerp (transform.position,  targetCamPos,smoothing * Time.deltaTime);
        }
    }
}
