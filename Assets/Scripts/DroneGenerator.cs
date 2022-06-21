using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject[] spots;
    private int randomSpot;
    private double timeUltLanzamiento;
    public GameObject Drone, DroneGenerado;
    private int numSpot;
    public int frecuenciaAparicion = 5;
    public GameObject robot;

    void Start()
    {
        timeUltLanzamiento = 0;
        numSpot = gameObject.transform.childCount;
        spots = new GameObject[numSpot];
        randomSpot = 3;// Random.Range(0, numSpot);
        for (int i = 0; i < numSpot; i++)
        {
            spots[i] = gameObject.transform.GetChild(i).gameObject;
        }
        Generar1Drone();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeUltLanzamiento > frecuenciaAparicion)
        {
            timeUltLanzamiento = Time.time;
            randomSpot = Random.Range(0, numSpot);
            Generar1Drone();
            Destroy(DroneGenerado, frecuenciaAparicion * 5);
        }
    }

    void Generar1Drone()
    {
        Quaternion rotation = spots[randomSpot].transform.rotation;
        Vector3 rotationEuler = rotation.eulerAngles;
        float rotY = Random.Range(0, 360);
        rotationEuler += new Vector3(0, rotY, 0);
        float x = Random.Range(-10, 10);
        float y = Random.Range(2, 20);
        float z = Random.Range(10, 50);
        DroneGenerado = Instantiate(Drone, new Vector3(x,y,z), Quaternion.Euler(rotationEuler));
        robot.GetComponentInChildren<AprenderLanzar>().Drone = DroneGenerado;
        robot.GetComponentInChildren<AprenderLanzar>().hayEnemigo = true;
    }
}
