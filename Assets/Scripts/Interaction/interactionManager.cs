using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum drawType
{
none,
wall,
barrier,
sign}
;

public class interactionManager : MonoBehaviour
{
	public GameObject LineHostPrefab;
	public Color selectedColor = Color.red;
	public List <Strip> myStrips;
	public float drawDistance = 5f;
	public drawType dt = drawType.sign;
	int pointer = 0;
	Vector3 lastPos;
	public	bool isDraging = false;
	public float heightAbovePeople=2;

	GameObject floor;
	public int clickCount=0;
	sign targetSign;
	Vector3 previousPos=Vector3.zero;
	public Sprite box;
	public Sprite arrow;
	GameObject plane;
	public List <char> avalibleGates =  new List<char>();
	// Use this for initialization
	void Start()
	{ 
		plane = new GameObject();
		plane.transform.RotateAround(plane.transform.position, Vector3.right, -90);
		plane.AddComponent<SpriteRenderer>();
		updateAvalibleGates();
		floor = GameObject.Find("Floor");
		myStrips = new List<Strip>();
	}
	void updateAvalibleGates(){
		avalibleGates.Clear();
		foreach (gate g in GameObject.FindObjectsOfType<gate>()) {
			foreach (char c in g.gateTarget) {
				if (!avalibleGates.Contains(c)) {
					avalibleGates.Add(c);
				}
			}
		}
		avalibleGates.Sort();
	}
	// Update is called once per frame
	void Update()
	{
		if (dt==drawType.sign && clickCount > 0) {
			RaycastHit hit;
			getClickPos(out hit);
			if (clickCount == 1) {
				plane.transform.position = new Vector3(hit.point.x, heightAbovePeople, hit.point.z);
				plane.transform.localScale = new Vector3(2, 2,1);
			} else if (clickCount == 2) {
				float distance = (hit.point - targetSign.transform.position).magnitude/2;
				plane.transform.localScale = new Vector3(distance, distance,1);

			}
			plane.transform.rotation = Quaternion.LookRotation(-new Vector3(hit.point.x, heightAbovePeople,hit.point.z)   + new Vector3(targetSign.transform.position.x,heightAbovePeople,targetSign.transform.position.z))*Quaternion.AngleAxis(-90,Vector3.right);


		}
	
		if(Input.GetMouseButtonDown(0)){
			OnMouseDown_();
		}
		else if(Input.GetMouseButtonUp(0)){
				OnMouseUp_();
			}
		else if(Input.GetMouseButton(0)){
					OnMouseDrag_();
				}


		if(Input.GetMouseButtonDown(1)){
			OnMousDownRight_();
		}
		else if(Input.GetMouseButtonUp(1)){
			
		}
		else if(Input.GetMouseButton(1)){
			
		}



	}
	void OnMousDownRight_(){
		RaycastHit hit;
		if (getClickPos(out hit)) {
			if (dt == drawType.barrier) {
			} else if (dt == drawType.wall) {
			} else if (dt == drawType.sign) {
				if (clickCount == 0) {
					targetSign = hit.transform.GetComponent<sign>();
					if (targetSign != null && targetSign.moveable) {
						clickCount = 10;

					} 

				}
				else if (clickCount == 10) {
					targetSign = null;
					clickCount = 0;		
				}
			}

		}
	}

	void OnMouseDown_()
	{
		if (isDraging) {
			isDraging = false;
			Debug.Log("This should not happen on mouseDown We should always be draging. Closing the current line");
			if (dt == drawType.barrier) {
				myStrips[pointer].stop();
			} else if (dt == drawType.wall) {

			}
		} else {
			
			RaycastHit hit;
			if (getClickPos(out hit)) {
				if (dt == drawType.barrier) {
					GameObject newLineHost = Instantiate(LineHostPrefab) as GameObject;
					Strip temp = newLineHost.transform.GetComponent<Strip>();
					newLineHost.transform.parent = transform;
					newLineHost.transform.position = Vector3.zero + (Vector3.up * 0.1f);
					temp.init(selectedColor, hit.point);
					isDraging = true;
					myStrips.Add(temp);
					pointer = myStrips.Count - 1;
					lastPos = hit.point;
					Debug.Log("Start: " + lastPos);
				} else if (dt == drawType.wall) {
				//	floor.transform.GetComponent<vectorField>().addWWC(hit.point); // interaction has to re implemented also recarding the recalculation of the VectorField
				} else if (dt == drawType.sign) {
					Debug.Log("clicking in sign draw type with click count: " + clickCount);
					if (clickCount == 0) {
						targetSign = hit.transform.GetComponent<sign>();
						if (targetSign != null && targetSign.moveable) {
							targetSign.setActive(false);
							plane.transform.GetComponent<SpriteRenderer>().enabled = true;
							plane.transform.GetComponent<SpriteRenderer>().sprite = box;
							plane.transform.position = new Vector3(hit.point.x, heightAbovePeople, hit.point.z);
							plane.transform.localScale = new Vector3(2, 2, 1);
							clickCount = 1;
							Debug.Log("Blub1");
						} 
					} else if (clickCount == 1) {
						targetSign.transform.position = new Vector3(hit.point.x, targetSign.transform.position.y, hit.point.z);

						float distance = (hit.point - targetSign.transform.position).magnitude / 2;
						plane.transform.localScale = new Vector3(distance, distance, 1);

						plane.transform.GetComponent<SpriteRenderer>().sprite = arrow;

						plane.transform.GetComponent<SpriteRenderer>().enabled = true;
						clickCount = 2;
						Debug.Log("Blub2");
					} else if (clickCount == 2) {
						targetSign.transform.rotation = Quaternion.LookRotation(-new Vector3(hit.point.x, heightAbovePeople, hit.point.z) + new Vector3(targetSign.transform.position.x, heightAbovePeople, targetSign.transform.position.z)) * Quaternion.AngleAxis(-90, Vector3.right);

						targetSign.setDirection(new Vector3(hit.point.x, 0, hit.point.z) - new Vector3(targetSign.transform.position.x, 0, targetSign.transform.position.z));
						plane.transform.GetComponent<SpriteRenderer>().enabled = false;
						clickCount = 0;
						targetSign.setActive(true);
						targetSign = null;
						Debug.Log("Blub3");
					} else if (clickCount == 10) {
						
						//targetSign = null;
						//clickCount = 0;		
					}
				}
			}
		}
	}

	bool getClickPos(out RaycastHit hit)
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
		if (Physics.Raycast(ray, out hit,transform.position.y+10)) {
			
			return true;
		} 
		Debug.Log("failiour to connect");
			
		return false;

	}

	void OnMouseDrag_()
	{
		RaycastHit hit;
		getClickPos(out hit);
		//Debug.Log ("Drag");
		if (dt == drawType.barrier) {
			if (isDraging) {
				if ((lastPos - hit.point).magnitude > drawDistance) {
					myStrips[pointer].addPoint(hit.point);
					lastPos = hit.point;
				}
			}
		} else if (dt == drawType.wall) {
		//	floor.transform.GetComponent<vectorField>().addWWC(hit.point);
		} else if (dt == drawType.sign) {
			//then we just move the sign // maybe not
		}
	}

	void OnMouseUp_()
	{
		isDraging = false;
		if (dt == drawType.barrier) {
			myStrips[pointer].stop();
			lastPos = Vector3.zero;
		} else if (dt == drawType.wall) {
		} else if (dt == drawType.sign) {
			//apply the changes to to thedirection and size
		}
	}

void OnGUI() {
		//Debug.Log("whatevers is mine");
		if (targetSign != null && clickCount==10) {
		int length= avalibleGates.Count*21;
		Vector3 screenPos = transform.GetComponent<Camera>().WorldToScreenPoint(targetSign.transform.position);
			Rect area = new Rect(screenPos.x - 25, Screen.height-(screenPos.y+length / 2), 50, length);
			GUI.Box(area,"");
				GUILayout.BeginArea(area);

			foreach (char c in avalibleGates) {
				
				if(targetSign.hasTarget(c)){
					
					if (!GUILayout.Toggle(true, c.ToString())) {
						targetSign.removeTarget(c);
					}

				}
				else{
					if (GUILayout.Toggle(false, c.ToString())) {
						targetSign.addTarget(c);
					}
				}
			}
			GUILayout.EndArea();
		}
	}
}
	

