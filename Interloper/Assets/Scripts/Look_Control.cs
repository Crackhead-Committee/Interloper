using UnityEngine;
using UnityEngine.InputSystem;

public class Look_Control : MonoBehaviour
{
    public GameObject Player;
    float xrotation;
    float yrotation;
    Vector2 mousemovement;
    float camera_y;
    Vector3 pos;
    public float mouse_sensitivity=10f;

    void Start()
    {
        camera_y=transform.position.y-Player.transform.position.y;
        Cursor.lockState=CursorLockMode.Locked;
    }
    

    // Update is called once per frame
    void Update()
    {
        //Update Mouse position to player position
        pos=Player.transform.position;
        pos.y+=camera_y;
        transform.position=pos;
        //Get mouse movement in new input system
        mousemovement=Mouse.current.delta.ReadValue();     
        xrotation-=mousemovement.y*Time.deltaTime*mouse_sensitivity;   
        xrotation=Mathf.Clamp(xrotation,-90,90);
        yrotation+=mousemovement.x*Time.deltaTime*mouse_sensitivity;
        transform.rotation=Quaternion.Euler(xrotation,yrotation,0);
        //Rotating the player
        Player.transform.localRotation=Quaternion.Euler(0,yrotation,0);
        
    }
}