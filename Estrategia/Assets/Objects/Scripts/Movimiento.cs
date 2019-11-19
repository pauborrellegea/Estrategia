using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movimiento : MonoBehaviour {


    public float velocidad;

    Vector3 movimiento;//contiene la x, y, z del personaje
    Rigidbody playerRigidbody;
    Animator anim;


    public static bool jugadorPillado;

    void Awake()
    {
        anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();

        jugadorPillado = false;

    }

    


    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");//input para la tecla de movimiento en x
        float v = Input.GetAxisRaw("Vertical");//input para la tecla de movimiento en z

        if(jugadorPillado == false)
        {
            Move(h, v);
            Turning();
            //Animating(h, v);
        }

    }

    void Move(float h, float v)
    {
        movimiento.Set(h, 0f, v);//asigna las teclas pulsadas al vector posición del personaje
        movimiento = movimiento.normalized * velocidad * Time.deltaTime;//normaliza el vector para que al ir en diagonal no se tenga ventaja
        playerRigidbody.MovePosition(transform.position + movimiento);//actualiza la posición del personaje según lo que se ha tecleado



    }

    void Turning()
    {
        /*Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit floorHit;

        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;
            playerToMouse.y = 0f;

            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            playerRigidbody.MoveRotation(newRotation);
        }*/

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, -45, 0);
            }

            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, 45, 0);
            }

            else
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, 0, 0);
            }       

        }

        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, -135, 0);
            }

            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, 135, 0);
            }

            else
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, 180, 0);
            }        

        }

        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, -45, 0);
            }

            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, -135, 0);
            }

            else
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, -90, 0);
            }


        }

        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, 45, 0);
            }

            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, 135, 0);
            }

            else
            {
                GetComponent<Transform>().eulerAngles = new Vector3(0, 90, 0);
            }


        }

        /*else if((Input.GetKeyDown(W) || Input.GetKeyDown(f_arriba)) && (Input.GetKeyDown(A) || Input.GetKeyDown(f_izquierda)))
        {

            GetComponent<Transform>().eulerAngles = new Vector3(0, -45, 0);

        }

        else if ((Input.GetKeyDown(W) || Input.GetKeyDown(f_arriba)) && (Input.GetKeyDown(D) || Input.GetKeyDown(f_derecha)))
        {

            GetComponent<Transform>().eulerAngles = new Vector3(0, 45, 0);

        }

        else if ((Input.GetKeyDown(S) || Input.GetKeyDown(f_abajo)) && (Input.GetKeyDown(A) || Input.GetKeyDown(f_izquierda)))
        {

            GetComponent<Transform>().eulerAngles = new Vector3(0, -135, 0);

        }

        else if ((Input.GetKeyDown(S) || Input.GetKeyDown(f_abajo)) && (Input.GetKeyDown(D) || Input.GetKeyDown(f_derecha)))
        {

            GetComponent<Transform>().eulerAngles = new Vector3(0, 135, 0);

        }*/

    }



    void Animating(float h, float v)
    {
        bool walking = h != 0f || v != 0f;
        anim.SetBool("isWalking", walking);
    }
}
