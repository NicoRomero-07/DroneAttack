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

    public GameObject ShellPrefab, GoombaPrefab;
    GameObject ShellInstance, GoombaInstance;
    public GameObject Goomba;
    public float Fz=350;
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
            casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_LanzarShell.arff"));
            saberPredecirFx = (Classifier)SerializationHelper.read("Assets/saberPredecirFxLanzarShellModelo");
            saberPredecirDistanciaFinal = (Classifier)SerializationHelper.read("Assets/saberPredecirDistanciaFinalLanzarShellModelo");
        }
            

        

    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vac?a:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias_LanzarShell.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del ?ltimo entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias_GiroDotYVelocidad.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {

            for (float Fx = -5f; Fx <= 100f;Fx = Fx + 2f)
            {
                for (float disGoombaX = -4; disGoombaX <= 4; disGoombaX += 1f)
                {
                    for (float disGoombaZ = 2 ; disGoombaZ <= 12; disGoombaZ += 1f)
                    {
                        GoombaInstance = Instantiate(GoombaPrefab, new Vector3(disGoombaX, 0.5f, disGoombaZ), Quaternion.Euler(0, 45, 0));
                        transform.LookAt(GoombaInstance.transform.position);

                        Vector3 forwardGoomba = GoombaInstance.transform.forward;
                        Vector3 forwardLanzador = transform.forward;
                        float angulo = Vector3.Angle(forwardGoomba, forwardLanzador);

                        ShellInstance = Instantiate(ShellPrefab, transform.position, transform.rotation);
                        Rigidbody rbShell = ShellInstance.GetComponent<Rigidbody>();
                        Vector3 fuerzaZ = transform.forward * Fz;
                        Vector3 fuerzaX = transform.right * Fx;
                        rbShell.AddForce(fuerzaX + fuerzaZ);
                        
                        float time = Time.time;
                        distanciaAnterior = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
                        distanciaInicial = distanciaAnterior;
                        
                        yield return new WaitUntil(() => Time.time - time > Time.deltaTime);
                        
                        yield return new WaitUntil(() => seAlejen(ShellInstance, GoombaInstance));

                        float finalDistanceToGoomba = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
                        Destroy(GoombaInstance);
                        Destroy(ShellInstance);

                        Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                        print("ENTRENAMIENTO: con Fx " + Fx + " y distancia a enemigo  " + distanciaInicial + " y angulo a enemigo " + angulo +" se alcanzo distancia a objetivo de " + finalDistanceToGoomba);
                        casoAaprender.setDataset(casosEntrenamiento);                          
                        casoAaprender.setValue(0, Fx);                                         
                        casoAaprender.setValue(1, distanciaInicial);
                        casoAaprender.setValue(2, angulo);
                        casoAaprender.setValue(3, finalDistanceToGoomba);
                        casosEntrenamiento.add(casoAaprender);                                 
                        
                    }
                }
            }


            File salida = new File("Assets/Finales_Experiencias_LanzarShell.arff");
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
        SerializationHelper.write("Assets/saberPredecirFxLanzarShellModelo", saberPredecirFx);

        saberPredecirDistanciaFinal = new M5P();
        casosEntrenamiento.setClassIndex(3);
        saberPredecirDistanciaFinal.buildClassifier(casosEntrenamiento);
        SerializationHelper.write("Assets/saberPredecirDistanciaFinalLanzarShellModelo", saberPredecirDistanciaFinal);

        ESTADO = "Con conocimiento";
        print("uwu");

        
    }

    private bool seAlejen(GameObject shellInstance, GameObject goombaInstance)
    {

        bool seAlejan = false;
        float distanciaActual = Vector3.Distance(ShellInstance.transform.position, GoombaInstance.transform.position);
        if (distanciaActual > distanciaAnterior) { 
            seAlejan = true;
        }
        distanciaAnterior = distanciaActual;
        return seAlejan;
    }

    void FixedUpdate()                                                                                 
    {
        if (Goomba != null)
        {
            transform.LookAt(Goomba.transform.position);
        }
        if ((ESTADO == "Con conocimiento") && hayEnemigo && Goomba!=null && Vector3.Distance(Goomba.transform.position, transform.position)<15 && objetivoDelante())
        {
            
            transform.LookAt(Goomba.transform.position);
            
            Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
            casoPrueba.setDataset(casosEntrenamiento);

            Vector3 forwardGoomba = Goomba.transform.forward;
            Vector3 forwardLanzador = transform.forward;
            float angulo = Vector3.Angle(forwardGoomba, forwardLanzador);

            casoPrueba.setValue(1, Vector3.Distance(transform.position, Goomba.transform.position));
            casoPrueba.setValue(2, angulo);
            casoPrueba.setValue(3, 0);
            float mejorFx = (float)saberPredecirFx.classifyInstance(casoPrueba);

            ShellInstance = Instantiate(ShellPrefab, transform.position, transform.rotation);
            Rigidbody rbShell = ShellInstance.GetComponent<Rigidbody>();
            Vector3 fuerzaZ = transform.forward * Fz;
            Vector3 fuerzaX = transform.right * mejorFx;
            rbShell.AddForce(fuerzaX + fuerzaZ);
            hayEnemigo = false;
            print("DECISION REALIZADA: Fx " + mejorFx);
            
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
