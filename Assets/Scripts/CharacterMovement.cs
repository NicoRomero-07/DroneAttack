using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private Vector3 move = Vector3.zero;
    CharacterController characterController;

    [Header("Opciones Personaje")]
    public float speed = 5.0f;
    public float runSpeed = 7.0f;
    public float jumpSpeed = 6.0f;
    public float gravity = 20.0f;

    [Header("Opciones Camara")]
    public Camera cam;
    public float maxRotation=65.0f;
    public float minRotation=-62.0f;
    public float mouseHorizontal=2.0f;
    public float mouseVertical=2.0f;

    float h_mouse, v_mouse;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; 
    }

    // Update is called once per frame
    void Update()
    {
        h_mouse = mouseHorizontal * Input.GetAxis("Mouse X");
        v_mouse += mouseVertical * Input.GetAxis("Mouse Y");

        v_mouse = Mathf.Clamp(v_mouse, minRotation, maxRotation);

        cam.transform.localEulerAngles = new Vector3(-v_mouse, 0, 0);
        transform.Rotate(0, h_mouse, 0);

        if(characterController.isGrounded){
            move = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

            if(Input.GetKey(KeyCode.LeftShift)){
                move = transform.TransformDirection(move) * runSpeed;
            } else {
                move = transform.TransformDirection(move) * speed;
            } 
            
            if(Input.GetKey(KeyCode.Space)){
                move.y = jumpSpeed;
            }


        }

        move.y -= gravity * Time.deltaTime; 

        characterController.Move(move * Time.deltaTime);
    }
}
