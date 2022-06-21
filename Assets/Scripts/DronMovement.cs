using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronMovement : MonoBehaviour
{
    Rigidbody rb;
    private float force;
    private float limSpeed;
    Vector3 fuerza;
    private float fuerzaLevitacion;

    // Start is called before the first frame update
    void Start()
    {
        force = (float)5;
        limSpeed = 10;
        rb = GetComponent<Rigidbody>();
        fuerza = transform.forward * force;
        fuerzaLevitacion = -(rb.mass * Physics.gravity.y);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float velocidad = Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2) + Mathf.Pow(rb.velocity.y, 2));
        if (velocidad < limSpeed)
        {
            rb.AddForce(fuerza);
        }
        rb.AddForce(Vector3.up * fuerzaLevitacion);

    }
}
