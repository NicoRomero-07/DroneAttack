using System;
using System.Collections;
using System.Collections.Generic;
using java.io;
using UnityEngine;
using weka.classifiers;
using weka.classifiers.trees;
using weka.core;
using weka.core.converters;

public class AprenderLanzar : MonoBehaviour
{

    Classifier saberPredecirFx, saberPredecirDistanciaFinal;
    weka.core.Instances casosEntrenamiento;
    private string ESTADO = "Sin conocimiento";
    
    Rigidbody rb;

    public GameObject BulletPrefab, DronePrefab;
    GameObject BulletInstance, DroneInstance;
    public GameObject Drone;
    public float Fz=1000;
    public float Velocidad_Simulacion = 100;
    public bool lanzado = false, hayEnemigo = false;
    private float distanciaAnterior;
    private float distanciaInicial;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        ESTADO = "Con conocimiento";
        if (ESTADO == "Sin conocimiento")
        {
            Time.timeScale = Velocidad_Simulacion;
            StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento
        }
        else
        {
            casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Arf/Finales_Experiencias_LanzarBullet.arff"));
            saberPredecirFx = (Classifier)SerializationHelper.read("Assets/M5P/saberPredecirFxLanzarBulletModelo");
            saberPredecirDistanciaFinal = (Classifier)SerializationHelper.read("Assets/M5P/saberPredecirDistanciaFinalLanzarBulletModelo");
        }
            

        

    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vac?a:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Arf/Iniciales_Experiencias_LanzarBullet.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del ?ltimo entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Arf/Finales_Experiencias_GiroDotYVelocidad.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {

            for (float Fx = -100f; Fx <= 100f;Fx = Fx + 50f)
            {
                for(float Fy = 0f; Fy <= 200f; Fy = Fy + 50f)
                {
                    for (float disDroneX = -10; disDroneX <= 10; disDroneX += 5f)
                    {
                        for (float disDroneZ = 10; disDroneZ <= 50; disDroneZ += 10f)
                        {
                            for (float disDroneY = 2; disDroneY <= 20; disDroneY += 5f)
                            {
                                for (float rotacionY = -180; rotacionY <= 180; rotacionY += 60)
                                {
                                    Vector3 distanceDron = new Vector3(disDroneX, disDroneY, disDroneZ);
                                    Vector3 rotationDron = transform.rotation.eulerAngles + Quaternion.Euler(0, rotacionY, 0).eulerAngles;
                                    DroneInstance = Instantiate(DronePrefab, distanceDron+transform.position, Quaternion.Euler(rotationDron));
                                    transform.LookAt(DroneInstance.transform.position);

                                    BulletInstance = Instantiate(BulletPrefab, transform.position, transform.rotation);
                                    Rigidbody rbBullet = BulletInstance.GetComponent<Rigidbody>();
                                    rbBullet.useGravity = false;

                                    Vector3 forwardShot = transform.forward;
                                    Vector3 forwardDrone = DroneInstance.transform.forward;

                                    float angle = Vector3.Angle(forwardDrone, forwardShot);
                                    float inclinacion = Vector3.Angle(Vector3.forward, forwardShot);

                                    Vector3 fuerzaZ = transform.forward * Fz;
                                    Vector3 fuerzaY = transform.up * Fy;
                                    Vector3 fuerzaX = transform.right * Fx;
                                    rbBullet.AddForce(fuerzaX + fuerzaY + fuerzaZ );


                                    float time = Time.time;
                                    distanciaAnterior = Vector3.Distance(BulletInstance.transform.position, DroneInstance.transform.position);
                                    distanciaInicial = distanciaAnterior;

                                    yield return new WaitUntil(() => Time.time - time > Time.deltaTime);

                                    yield return new WaitUntil(() => seAlejen(BulletInstance, DroneInstance));

                                    float finalDistanceToDrone = Vector3.Distance(BulletInstance.transform.position, DroneInstance.transform.position);
                                    Destroy(DroneInstance);
                                    Destroy(BulletInstance);

                                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                                    print("ENTRENAMIENTO: con Fx: " + Fx + " Fy: " + Fy + " X: " + disDroneX + " Y: " + disDroneY + " Z: " + disDroneZ + "distancia: " + finalDistanceToDrone);
                                    casoAaprender.setDataset(casosEntrenamiento);

                                    casoAaprender.setValue(0, Fx);
                                    casoAaprender.setValue(1, Fy);
                                    casoAaprender.setValue(2, DroneInstance.transform.position.x - transform.position.x);
                                    casoAaprender.setValue(3, DroneInstance.transform.position.y - transform.position.y);
                                    casoAaprender.setValue(4, DroneInstance.transform.position.z - transform.position.z);
                                    casoAaprender.setValue(5, angle);
                                    casoAaprender.setValue(6, finalDistanceToDrone);

                                    casosEntrenamiento.add(casoAaprender);
                                }
                                
                            }
                        }
                    }
                }
            }


            File salida = new File("Assets/Arf/Finales_Experiencias_LanzarBullet.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }


        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirFx = new M5P();                                                
        casosEntrenamiento.setClassIndex(0);                                             
        saberPredecirFx.buildClassifier(casosEntrenamiento);                        
        SerializationHelper.write("Assets/M5P/saberPredecirFxLanzarBulletModelo", saberPredecirFx);

        saberPredecirDistanciaFinal = new M5P();
        casosEntrenamiento.setClassIndex(6);
        saberPredecirDistanciaFinal.buildClassifier(casosEntrenamiento);
        SerializationHelper.write("Assets/M5P/saberPredecirDistanciaFinalLanzarBulletModelo", saberPredecirDistanciaFinal);

        print("FIN");

        
    }

    private bool seAlejen(GameObject BulletInstance, GameObject DroneInstance)
    {

        bool seAlejan = false;
        float distanciaActual = Vector3.Distance(BulletInstance.transform.position, DroneInstance.transform.position);
        if (distanciaActual > distanciaAnterior) { 
            seAlejan = true;
        }
        distanciaAnterior = distanciaActual;
        return seAlejan;
    }

    void FixedUpdate()                                                                                 
    {
        if (Drone != null)
        {
            transform.LookAt(Drone.transform.position);
        }
        if ((ESTADO == "Con conocimiento") && Drone!=null && Vector3.Distance(Drone.transform.position, transform.position)<100 && objetivoDelante() && Input.GetMouseButtonDown(0))
        {
            
            float mejorFx = 0;
            float mejorFy = 0;
            float distanciaAlcanzada;
            float mejorDistancia = 1000;
            float Fx = 0;

            for (float Fy = -10; Fy < 120; Fy += 1)
            {
                transform.LookAt(Drone.transform.position);
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());

                Vector3 forwardShot = transform.forward;
                Vector3 forwardDrone = Drone.transform.forward;

                float angle = Vector3.Angle(forwardDrone, forwardShot);
                float inclinacion = Vector3.Angle(Vector3.forward, forwardShot);

                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(1, Fy);
                casoPrueba.setValue(2, Drone.transform.position.x - transform.position.x);
                casoPrueba.setValue(3, Drone.transform.position.y - transform.position.y);
                casoPrueba.setValue(4, Drone.transform.position.z - transform.position.z);
                casoPrueba.setValue(5, angle);
                casoPrueba.setValue(6, 0);
                    
                Fx = (float)saberPredecirFx.classifyInstance(casoPrueba);

                if (Fx > -10000 & Fx < 10000)
                {
                    Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba2.setDataset(casosEntrenamiento);
                    casoPrueba2.setValue(0, Fx);
                    casoPrueba2.setValue(1, Fy);
                    casoPrueba2.setValue(2, Drone.transform.position.x - transform.position.x);
                    casoPrueba2.setValue(3, Drone.transform.position.y - transform.position.y);
                    casoPrueba2.setValue(4, Drone.transform.position.z - transform.position.z);
                    casoPrueba2.setValue(5, angle);
                    
                    distanciaAlcanzada = (float)saberPredecirDistanciaFinal.classifyInstance(casoPrueba2);

                    if (distanciaAlcanzada < mejorDistancia)
                    {
                        mejorFx = Fx;
                        mejorFy = Fy;
                        mejorDistancia = distanciaAlcanzada;
                    }
                }

                
            }

            BulletInstance = Instantiate(BulletPrefab, transform.position, new Quaternion(0,0,0,0));
            Rigidbody rbBullet = BulletInstance.GetComponent<Rigidbody>();
            Vector3 fuerzaZ = transform.forward * Fz;
            Vector3 fuerzaY = transform.up * mejorFy;
            Vector3 fuerzaX = transform.right * mejorFx;
            rbBullet.AddForce(fuerzaX + fuerzaY + fuerzaZ);
            hayEnemigo = false;
            print("DECISION REALIZADA: Fx " + mejorFx + " Fy: " + mejorFy);
            
        }


       
            
        
    }

    private bool objetivoDelante()
    {
        float angulo = Vector3.Angle(transform.forward, transform.parent.transform.forward);
        if (angulo > 180)
        {
            angulo = 360 - angulo;
        }
        return angulo < 80;
    }
}
