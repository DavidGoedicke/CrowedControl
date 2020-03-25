using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class unit : MonoBehaviour
{
	public AnimationCurve avoidance;
	Vector3 targetDirection = Vector3.forward;
	Vector3 newTargetDirection = Vector3.forward;
	public float firstRad = 1;
	public float secondRad = 4;
	public float thirdRad = 10;
	public float fourthRad=25;
	public int id;
	public float speed = 1;
	public char myTarget='-';

	public bool useStrip=false;
	public bool useSign = true;
	// Use this for initialization
	private List<sign> visitedSigns = new List<sign>();
	public sign target=null;
	public bool visited = false;


	private float turingValue=0;
	private float distanceToSign=0;
	private vectorField floor;
	void Start ()
	{
		//Keyframe[] ks = new Keyframe[2];
		//ks[0]= new Keyframe(0,0);
		//ks[1]= new Keyframe(1.0f,1.0f);
		//avoidance = new AnimationCurve(ks);
		//targetDirection = new Vector3 (Random.Range (-1.0f, 1.0f), 0, Random.Range (-1.0f, 1.0f)).normalized;
		//newTargetDirection = new Vector3 (Random.Range (-1.0f, 1.0f), 0, Random.Range (-1.0f, 1.0f)).normalized;
		floor=GameObject.Find ("Floor").transform.GetComponent<vectorField> ();

	}
	public void setTargetDirection(Vector3 input ){

		targetDirection = input;
		newTargetDirection=targetDirection;
	}
	// Update is called once per frame
	void Update ()
	{
		
		unit[] allUnits = Object.FindObjectsOfType<unit> ();//transform.GetComponents<unit> ();
		//	Debug.Log("allUnits.Length is "+ allUnits.Length);
		List<unit> first = new List<unit> ();
		List<unit> second = new List<unit> ();
		List<unit> third = new List<unit> ();
		foreach (unit u in allUnits) {
			if (u.transform != transform) {
				float distance = (u.transform.position - transform.position).magnitude;
				if (distance < thirdRad) {
					third.Add (u);
				}
				if (distance < secondRad) {
					second.Add (u);
				}
				if (distance < firstRad) {
					first.Add (u);
				}
			}
		}
		Vector3 avo = avoide (first);//.normalized;
		Vector3 alg = allign (second);//.normalized;
		Vector3 dir = direct (third).normalized;
		Vector3 stripG;

		Vector3 walls = floor.getMotionVector (transform.position,false);
		if (useStrip) {
			stripG = stripGuide();
		} else {
			 stripG = Vector3.zero;
		}
		Vector3 signG;
		if(useSign){
			signG = signGuide();

			
			}
		else{
			signG = Vector3.zero;
		}
			
		if (id == 0) {
			Debug.DrawLine (transform.position, transform.position + avo * 1f, Color.red);
			Debug.DrawLine (transform.position, transform.position + alg * 1f, Color.green);
			Debug.DrawLine (transform.position, transform.position + dir * 2f, Color.blue);
			Debug.DrawLine (transform.position, transform.position + stripG * 1f, Color.yellow);
			Debug.DrawLine (transform.position, transform.position + signG * 1f, Color.magenta);
			Debug.DrawLine (transform.position, transform.position + walls * 1f, Color.cyan);

			//Debug.Log ("Avo: " + avo + " alg:" + alg + " dir:" + dir); 
		}
		newTargetDirection = (targetDirection * 0.7f) + (avo * 0.025f) + (alg * 0.001f) + (dir * 0.001f)+(stripG*0.01f)+(signG*0.07f)+(walls*0.065f);

	
	
	}
	IEnumerator removeSignTimmer(sign s) {
		yield return new WaitForSeconds(2*60);
		eraseFromMemory(s);
	}

	void eraseFromMemory(sign s){
		if(visitedSigns.Contains(s)){
			visitedSigns.Remove(s);
			}
	}


	Vector3 signGuide(){

		if(target==null){
		sign[] allSigns = Object.FindObjectsOfType<sign> ();
	float dist = -1; 
			//Debug.Log("possible target count" + allSigns.Length);
			foreach (sign s in allSigns) {
				if(! visitedSigns.Contains(s)){
					if (s.hasTarget(myTarget)) {
						RaycastHit hit;
						if (Physics.Raycast(transform.position, (s.transform.position - transform.position).normalized, out hit, (s.transform.position - transform.position).magnitude)) {
							//Debug.DrawLine(transform.position, hit.point);
						//	Debug.Log(hit.transform.name);
						//	Debug.Break();

							if (hit.transform == s.transform) { 
								if (Vector3.Angle(transform.forward, s.transform.position - transform.position) < 45) {
									if (dist == -1) {
										target = s;
										dist = (s.transform.position - transform.position).magnitude;
									} else if (dist > (s.transform.position - transform.position).magnitude) {
										target = s;
										dist = (s.transform.position - transform.position).magnitude;
									}
								}
							}
						}
					}
				}
			}
		}
		Vector3 returnValue = Vector3.zero;
		if (target != null) {
			//if ((target.transform.position - transform.position).magnitude > (target.transform.position - (transform.position + (transform.forward))).magnitude) {  ///if we are on a trajectory out we need to just tell the sign content 
			//	if (Vector3.Angle(transform.forward, target.transform.position - transform.position) > target.getSize() * 0.01) { // if angle is smal enough

			distanceToSign = (target.transform.position - transform.position).magnitude;
			if (distanceToSign <= target.getSize() && !visited) {
				
				//Debug.Log("inside & turn");
				float newTurning= Mathf.Clamp01((target.getSize() - distanceToSign) / Mathf.Max((target.getSize() * 0.9f),1));
				if (newTurning >= turingValue) {
					turingValue = newTurning;
				} else if(newTurning < turingValue ){
					visited = true;
					turingValue = newTurning;
					visitedSigns.Add(target);
					StartCoroutine(removeSignTimmer(target));
				
				}
				returnValue = Vector3.Lerp((target.transform.position - transform.position).normalized, target.getDirection().normalized, turingValue);

			} else if (distanceToSign <= target.getSize() && visited) {
				returnValue =target.getDirection().normalized;
			//	Debug.Log("inside & leaving");

			}
			else if(distanceToSign > target.getSize() && !visited )  {
						//Debug.Log("outside & approach");
						returnValue = (target.transform.position - transform.position).normalized; 
					}

			else if(distanceToSign > target.getSize() && visited )  {
				//Debug.Log("outside & leaving");
				returnValue = target.getDirection().normalized;
				target = null;
				visited = false;
			}
			else{
				target = null;
				visited = false;
				//Debug.Log("Failed to disengage");
			}
		}
		return returnValue;
	}
	Vector3 stripGuide(){
		Strip[] allStrips = Object.FindObjectsOfType<Strip> ();

		if (allStrips.Length > 0) {
			Strip Candidate = null;
			float smallestApproach = (transform.position - allStrips [0].getColsestWaypoint ()).magnitude;
			if (smallestApproach <= fourthRad) {
				Candidate = allStrips [0];
			}
			foreach (Strip e in allStrips) {
				if ((transform.position - e.getColsestWaypoint ()).magnitude < smallestApproach && e.done) {


					if (fourthRad >= (transform.position - e.getColsestWaypoint ()).magnitude) {
						smallestApproach = (transform.position - e.getColsestWaypoint ()).magnitude;
						Candidate = e;
					}
				}
			}
			if (Candidate != null) {
				Debug.DrawLine (transform.position, Candidate.getColsestWaypoint (), Color.white);
				Vector3 vec = Candidate.getNextWaypoint (transform.position);
				vec.y = 0;
				return vec;//target- transform.position;
			} else {
				return Vector3.zero;
			}
		} else {

			return Vector3.zero;
		}


	}
	Vector3 avoide (List<unit> allUnits)
	{
		/// affect is inverse propotional
		if (allUnits.Count > 0) {
			Vector3 returnVector = Vector3.zero;
			foreach (unit u in allUnits) {

				Vector3 avoid = transform.position - u.transform.position;
				returnVector += (avoid* (1/avoid.magnitude));
				///old code that calculated the avoidance vector

				/*if (((transform.position + targetDirection * Time.deltaTime) - (u.transform.position + u.targetDirection * Time.deltaTime)).magnitude < firstRad / 2) {

				float angle = Vector3.Angle (new Vector3(targetDirection.x, 0, targetDirection.z), new Vector3 (u.targetDirection.x, 0, u.targetDirection.z));

				if (angle > 0) {
					
						returnVector = Quaternion.Euler(45,0,0)*targetDirection;
				} else {
						returnVector = Quaternion.Euler(-45,0,0)*targetDirection;
				}

			}*/
			}
			return returnVector/allUnits.Count;
		} else
			return new Vector3 (0, 0, 0);
	}

	Vector3 allign (List<unit> allUnits)
	{
		if (allUnits.Count > 0) {
			Vector3 returnVector = Vector3.zero;
			foreach (unit u in allUnits) {
				returnVector += u.targetDirection;

			}
			return returnVector/allUnits.Count;
		} else
			return new Vector3 (0, 0, 0);
	
	}

	Vector3 direct (List<unit> allUnits)
	{
		if (allUnits.Count > 0) {
			Vector3 returnVector = Vector3.zero;

			foreach (unit u in allUnits) {
				returnVector += u.transform.position - transform.position;
			


			}

			return returnVector / allUnits.Count;

		} else
			return new Vector3 (0, 0, 0);

	}

	void LateUpdate ()
	{
		
		targetDirection = new Vector3 (newTargetDirection.normalized.x, 0, newTargetDirection.normalized.z);
		//targetDirection.Normalize ();
		Debug.DrawLine (transform.position, transform.position + targetDirection * 1, Color.yellow);

		//	Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, Time.deltaTime*100,0f);
		//	transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (newDir), 90);
		transform.LookAt (transform.position + targetDirection, Vector3.up);
		//transform.rotation = Quaternion.Euler (newDir);//Quaternion.Lerp (transform.rotation, Quaternion.Euler (newDir), Time.deltaTime);
		transform.position = Vector3.Lerp (transform.position, transform.position + targetDirection, Time.deltaTime * speed);

	}
	void OnGUI(){

		//GUI.TextField(new Rect(50, 50, 100, 100), turingValue.ToString()+"\n"+distanceToSign.ToString());


	}


}
