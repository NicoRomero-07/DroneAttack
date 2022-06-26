using java.io;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using weka.classifiers;
using weka.classifiers.trees;
using weka.core;
using weka.core.converters;

public class JumpLearning : MonoBehaviour
{
    // Start is called before the first frame update
    Classifier saberPredecirFy, saberPredecirDistanciaFinal;
    weka.core.Instances casosEntrenamiento;

    GameObject edificio;
    float tamEdificioZ = 20;
    private string ESTADO = "Sin conocimiento";
    public float Fz = 0.01f;
    public float alturaAlcanzada;
    MovementInput movement;


    Rigidbody rb;
    CharacterController controller;

    public float Velocidad_Simulacion = 100;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<MovementInput>();

        controller = GetComponent<CharacterController>();
        ESTADO = "Con conocimiento";
        if (ESTADO == "Sin conocimiento")
        {
            Time.timeScale = Velocidad_Simulacion;
            StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento
        }
        else
        {
            casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Arf/Finales_Experiencias_Saltar.arff"));
            saberPredecirFy = (Classifier)SerializationHelper.read("Assets/M5P/saberPredecirFySaltar");
        }




    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vac?a:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Arf/Iniciales_Experiencias_Saltar.arff"));  //Lee fichero con variables. Sin instancias

        //Uso de una tabla con los datos del ?ltimo entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Arf/Finales_Experiencias_GiroDotYVelocidad.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {
            yield return new WaitUntil(() => controller.isGrounded);
            print("EMPIEZA");
            
            for (float Fy = 4f; Fy <= 100f; Fy = Fy + 5f)
            {
                for (float altura = 5f; altura <= 100f; altura = altura + 5f)
                {
                    
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.localScale += new Vector3(10,altura,tamEdificioZ);
                    cube.transform.position = gameObject.transform.position + new Vector3(0, altura/2, 5+(tamEdificioZ/2));

                    yield return new WaitUntil(() => controller.isGrounded);
                    movement.Fy = Fy;
                    movement.jump = true;

                    yield return new WaitUntil(() => empieceDescenso());

                    movement.jump = false;
                    movement.Fy = 0;

                    alturaAlcanzada = gameObject.transform.position.y;
                    
                    Destroy(cube);

                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                    print("ENTRENAMIENTO: con Fy: " + Fy + " altura: " + altura + " alcazada: "+ alturaAlcanzada);
                    casoAaprender.setDataset(casosEntrenamiento);

                    casoAaprender.setValue(0, Fy);
                    casoAaprender.setValue(1, altura);
                    casoAaprender.setValue(2, alturaAlcanzada);
                    
                    casosEntrenamiento.add(casoAaprender);
                    gameObject.transform.position = new Vector3(0, 0, 0);

                }
            }


            File salida = new File("Assets/Arf/Finales_Experiencias_Saltar.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }


        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirFy = new M5P();
        casosEntrenamiento.setClassIndex(0);
        saberPredecirFy.buildClassifier(casosEntrenamiento);
        SerializationHelper.write("Assets/M5P/saberPredecirFySaltar", saberPredecirFy);

        print("FIN");


    }

    private bool empieceDescenso()
    {
        return controller.velocity.y < -0.1;
    }


    void FixedUpdate()
    {
        if(ESTADO== "Con conocimiento")
        {
            movement.jump = false;
            RaycastHit hit;
            if (Input.GetKey(KeyCode.Space) && controller.isGrounded && Physics.Raycast(transform.position, transform.forward, out hit, 15))
            {
                float altura = calcularAltura();
                print("ALTURA POR PITAGORAS: " + altura);
                float Fy;
                if (altura == 0)
                {
                    print("NO HAY EDIFICIO");
                }
                else
                {
                    Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());

                    casoPrueba.setDataset(casosEntrenamiento);
                    casoPrueba.setValue(1, altura);
                    casoPrueba.setValue(2, altura);

                    Fy = (float)saberPredecirFy.classifyInstance(casoPrueba);

                    movement.Fy = Fy;

                    movement.jump = true;
                }
            }
        }
        
    }
    

    private float calcularAltura()
    {
        RaycastHit hit;
        float distanciaAEdificio = 0;
        float distanciaATop = 0;
        bool esEdificio;
        float height = 0;
        float i = 0;
        if (Physics.Raycast(transform.position , transform.forward, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.tag == "Edificio")
            {
                distanciaAEdificio = hit.distance;
                esEdificio = true;
                
                while (esEdificio && i<10)
                {
                    if(Physics.Raycast(transform.position + (transform.forward * 0.5f), transform.forward + (transform.up * i), out hit, 200))
                    {
                        print(hit.collider.gameObject.tag + " " +i);
                        UnityEngine.Debug.DrawRay(transform.position, transform.forward + (transform.up*i), Color.red);
                        if (hit.collider.gameObject.tag == "Edificio")
                        {
                            distanciaATop = hit.distance;
                        }
                        else
                        {
                            esEdificio = false;
                        }
                    }
                    else
                    {
                        esEdificio = false;
                    }

                    i += 0.01f;
                }
                print("Distancia a edificio: " + distanciaAEdificio + "Hipotenusa: " + distanciaATop);
                height = Mathf.Sqrt(Mathf.Pow(distanciaATop, 2) - Mathf.Pow(distanciaAEdificio, 2));
            }
            
        }

        return height;
    }
}
