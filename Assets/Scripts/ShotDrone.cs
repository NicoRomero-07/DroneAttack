using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using weka.classifiers;
using weka.core;

public class ShotDrone : MonoBehaviour
{
    Classifier saberPredecirFx, saberPredecirDistanciaFinal;
    weka.core.Instances casosEntrenamiento;

    Rigidbody rb;

    public GameObject BulletPrefab, DronePrefab;
    GameObject BulletInstance, DroneInstance;
    public GameObject Drone;
    public float Fz = 350;
    public bool lanzado = false, hayEnemigo = false;
    private float distanciaAnterior;
    private float distanciaInicial;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Arf/Finales_Experiencias_LanzarBullet.arff"));
        saberPredecirFx = (Classifier)SerializationHelper.read("Assets/M5P/saberPredecirFxLanzarBulletModelo");
        saberPredecirDistanciaFinal = (Classifier)SerializationHelper.read("Assets/M5P/saberPredecirDistanciaFinalLanzarBulletModelo");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Drone != null)
        {
            transform.LookAt(Drone.transform.position);
        }
        if (Input.GetMouseButtonDown(0) && Drone != null && Vector3.Distance(Drone.transform.position, transform.position) < 30)
        {
            Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
            casoPrueba.setDataset(casosEntrenamiento);
            float mejorFx = 0;
            float mejorFy = 0;
            float distanciaAlcanzada;
            float mejorDistancia = 1000;
            float Fx = 0;

            for (float Fy = 0; Fy < 200; Fy += 1)
            {
                transform.LookAt(Drone.transform.position);

                casoPrueba.setValue(1, Fy);
                casoPrueba.setValue(2, transform.position.x - Drone.transform.position.x);
                casoPrueba.setValue(3, transform.position.y - Drone.transform.position.y);
                casoPrueba.setValue(4, transform.position.z - Drone.transform.position.z);
                casoPrueba.setValue(5, transform.rotation.y - Drone.transform.rotation.y);
                casoPrueba.setValue(6, 0);

                Fx = (float)saberPredecirFx.classifyInstance(casoPrueba);
                casoPrueba.setValue(0, Fx);

                distanciaAlcanzada = (float)saberPredecirDistanciaFinal.classifyInstance(casoPrueba);
                if (distanciaAlcanzada < mejorDistancia)
                {
                    mejorFx = Fx;
                    mejorFy = Fy;
                }

            }


            BulletInstance = Instantiate(BulletPrefab, transform.position, transform.rotation);
            Rigidbody rbBullet = BulletInstance.GetComponent<Rigidbody>();
            Vector3 fuerzaZ = transform.forward * Fz;
            Vector3 fuerzaY = transform.up * mejorFy;
            Vector3 fuerzaX = transform.right * mejorFx;
            rbBullet.AddForce(fuerzaX + fuerzaZ + fuerzaY);
            hayEnemigo = false;
            print("DECISION REALIZADA: Fx " + mejorFx + " Fy: " + mejorFy);

        }
    }
}
