using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	void Start () {
        //Quaternion rotX = Quaternion.AngleAxis(90,new Vector3(1,0,0));
        //Quaternion rotY = Quaternion.AngleAxis(90,new Vector3(0,1,0));
        //Debug.Log("(rotX * rotY) * Vector3.up; \n" + (rotX * rotY) * Vector3.up);
        //Debug.Log("rotY * (rotX * Vector3.up); \n" + rotY * (rotX * Vector3.up));
        //Debug.Log("rotX * (rotY * Vector3.up); \n" + rotX * (rotY * Vector3.up));




        Quaternion x90 = Quaternion.AngleAxis(90, Vector3.right); // 90 degrees around the x axis
        Quaternion z90 = Quaternion.AngleAxis(90, Vector3.forward); // 90 degrees around the z axis

        Quaternion x90z90Concat = x90 * z90; // The concatenation of first 90 degrees around the x axis, then 90 degrees around the z axis
        Quaternion z90x90Concat = z90 * x90; // The opposite concatenation

        // Apply the two single rotations one by one, then print the rotation
        transform.rotation *= x90;
        transform.rotation *= z90;

        Debug.Log("Quaternion result of two rotatons in sequence: " + transform.rotation);

        // Reset to identity
        transform.rotation = Quaternion.identity;

        // Now apply the first concatenation, i.e. lhs then rhs, then print to see
        transform.rotation *= x90z90Concat;

        Debug.Log("Quaternion result of two rotations combined lhs then rhs: " + transform.rotation);

        // Reset to identity
        transform.rotation = Quaternion.identity;

        // Now apply the second concatenation, i.e. rhs then lhs, then print to see
        transform.rotation *= z90x90Concat;

        Debug.Log("Quaternion result of two rotations combined rhs then lhs: " + transform.rotation);

    }

    void Update () {
	
	}
}
