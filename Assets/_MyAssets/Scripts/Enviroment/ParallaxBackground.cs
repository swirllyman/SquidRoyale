 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour {

	[SerializeField] float backgroundWidth;
	[SerializeField] float backgroundHeight;
	[SerializeField] float parallaxSpeed;
	[SerializeField] Transform cameraTransform;

	Vector3 prevCameraPosition;

	void Start () 
	{
		if(cameraTransform == null) cameraTransform = Camera.main.transform;
	}
	
	void LateUpdate () 
	{
		UpdateParallax();
		UpdateOffset();
	}

	void UpdateParallax()
    {
		float deltaX = cameraTransform.position.x - prevCameraPosition.x;
		float deltaY = cameraTransform.position.y - prevCameraPosition.y;

		transform.position += Vector3.right * (deltaX * parallaxSpeed);
		transform.position += Vector3.up * (deltaY * parallaxSpeed);

		prevCameraPosition = cameraTransform.position;
	}

	void UpdateOffset()
    {

		if (cameraTransform.position.x > backgroundWidth + transform.position.x)
		{
			ShiftRight();
		}

		if (cameraTransform.position.x < transform.position.x - backgroundWidth)
		{
			ShiftLeft();
		}

		if (cameraTransform.position.y > backgroundHeight + transform.position.y)
		{
			ShiftUp();
		}

		if (cameraTransform.position.y < transform.position.y - backgroundHeight)
		{
			ShiftDown();
		}
	}

	void ShiftRight()
    {
		transform.position = new Vector3(transform.position.x + backgroundWidth * 2, transform.position.y, transform.position.z);
    }

	void ShiftLeft()
	{
		transform.position = new Vector3(transform.position.x - backgroundWidth * 2, transform.position.y, transform.position.z);
	}

    void ShiftUp()
    {
		transform.position = new Vector3(transform.position.x, transform.position.y + backgroundHeight * 2, transform.position.z);
	}
	void ShiftDown()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y - backgroundHeight * 2, transform.position.z);
	}
}