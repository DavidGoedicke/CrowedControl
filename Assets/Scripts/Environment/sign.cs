using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sign : MonoBehaviour {
	public Vector3 direction;
	public bool active=true;
	public bool moveable =true;
	public List<char> gateTarget = new List<char>();

	//Texture2D drawArrow;
	// Use this for initialization
	void Start () {
		//drawArrow = transform.AssetDatabase.FindAssets ("sprite t:texture2d",new string[] {"Assets/interaction"});
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void setDirection(Vector3 setDirection){
		direction=setDirection;
	}

	public Vector3 getDirection(){
		if (active)
			return direction;
		else
			return Vector3.zero;
	}
	public void setActive(bool input){
		if (input) {
			transform.GetComponent<Renderer>().enabled = true;
		} else {
			transform.GetComponent<Renderer>().enabled = false;
		}
		active = input;

	}
	public float getSize(){
		if (active)
			return direction.magnitude;
		else
			return 0.0f;
	}
	public bool hasTarget(char a){
		//if (active)
			return gateTarget.Contains(a);
		//else
			//return false;
	}

	public void setTargets( List<char> input){
		gateTarget = input;
	}
	public void addTarget(char a){
		gateTarget.Add(a);
	}
	public bool removeTarget(char a){
		if (gateTarget.Contains(a)){
			gateTarget.Remove(a);
			return true;
		}
		else {return false;}

	}

}

