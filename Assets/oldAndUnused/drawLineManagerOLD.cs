using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
public class drawLineManager : MonoBehaviour {
	
	stripColor selectedColor;
	List <strip> myStrips;
	public float drawDistance=5f;

	int pointer=0;
	Vector3 lastPos;
	public	bool isDraging = false;

	// Use this for initialization
	void Start () {
		myStrips = new List<strip> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnMouseDown() {
		Debug.Log ("Down");
		if (isDraging) {
			isDraging = false;
			Debug.Log ("This should not happen on mouseDown We should always be draging");
		} else {
			RaycastHit hit;
			getClickPos(out hit);
			if (hit.transform.CompareTag ("floor")) {
				strip temp = new strip (selectedColor, hit.point);
				isDraging = true;
				myStrips.Add (temp);
				pointer = myStrips.Count - 1;
				lastPos = hit.point;
			} else if (hit.transform.CompareTag ("agent")) {
				Debug.Log ("You Clicked on an Agent, Info coming soon");

			} else {
				Debug.Log ("We don't know where you clicked on");
			}
		}
	}
	void getClickPos(out RaycastHit hit){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
			Physics.Raycast (ray, out hit);
	}

	void OnMouseDrag() {
		Debug.Log ("Drag");
		if (isDraging) {
			RaycastHit hit;
			getClickPos(out hit);
			if (hit.transform.CompareTag ("floor")) {
				if ((lastPos - hit.point).magnitude > drawDistance) {
					myStrips [pointer].addPoint (hit.point);
					lastPos = hit.point;
				}
			}


		}
		
	}
	void OnMouseUp(){
		Debug.Log ("Up");
		isDraging = false;
		lastPos = Vector3.zero;

	}
	void OnGUI(){

		//change selected color
	}
}

public enum stripColor{Red,Green,Blue};

public class strip{
	List <Vector3> points;
	stripColor myColor;

	public strip(){
		points = new List<Vector3> ();
		myColor = 0;
	}
	public strip(stripColor newCol,Vector3 firstPoint){
		points = new List<Vector3> ();
		myColor = newCol;
		points.Add (firstPoint);
	}

	public void addPoint(Vector3 nextPoint){
		points.Add (nextPoint);

	}



}
*/