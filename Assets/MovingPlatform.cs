using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {

	public float speed = 2.5f;
	public int frames = 120, currentframe = 0;
	public bool vertical = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		currentframe++;
		if (currentframe > frames) {
			currentframe = 0;
			speed *= -1;
		}
			
	}

	void Update(){
		if (vertical) {
			transform.position = new Vector3 (transform.position.x, 
				transform.position.y + speed * Time.deltaTime);
		} else {
			transform.position = new Vector3 (transform.position.x
				+ speed * Time.deltaTime, transform.position.y);
		}
	}
}
