using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    private double timeUltLanzamiento;
    public GameObject Drone, DroneGenerado;
    private int numSpot;
    public int frecuenciaAparicion = 3;
    public GameObject robot;

    void Start()
    {
        timeUltLanzamiento = 0;
      
        Generar1Drone();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeUltLanzamiento > frecuenciaAparicion)
        {
            timeUltLanzamiento = Time.time;
            Generar1Drone();
            Destroy(DroneGenerado, frecuenciaAparicion * 5);
        }
    }

    void Generar1Drone()
    {
        Quaternion rotation = transform.rotation;
        Vector3 rotationEuler = rotation.eulerAngles;
        float rotY = Random.Range(-15, 15);
        rotationEuler += new Vector3(0, rotY, 0);
        float x = Random.Range(-3, 3);
        float y = Random.Range(2, 4);
        float z = Random.Range(5, 15);
        Vector3 desplazamiento = transform.forward*z + transform.right* x +transform.up * y;
        DroneGenerado = Instantiate(Drone, transform.position + desplazamiento, Quaternion.Euler(rotationEuler));
        robot.GetComponentInChildren<AprenderLanzar>().Drone = DroneGenerado;
        robot.GetComponentInChildren<AprenderLanzar>().hayEnemigo = true;
    }
}
