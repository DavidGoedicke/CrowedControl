using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class gate : MonoBehaviour {

	public bool spawining =true;
	public List<char> gateTarget = new List<char>();

	public List<char> spawningUnitsTarget = new List<char>();
	public GameObject unitPrefab;
	public float rate;// units per second

	sign  mySign;
	// Use this for initialization

	float timer=0;
	void Start () {
		//rate = 1.0f;
		mySign= gameObject.AddComponent<sign>() as sign;
		mySign.setDirection( -transform.forward * transform.lossyScale.magnitude);
		mySign.setTargets( gateTarget);
		mySign.moveable = false;
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if (timer > 1 / rate) {
			timer = 0;



				Vector3 rndPosWithin;
				rndPosWithin = new Vector3(Random.Range(-1f, 1f),1f, Random.Range(-1f, 1f));
				rndPosWithin = transform.TransformPoint(rndPosWithin * .5f);
				GameObject newUnit= Instantiate(unitPrefab, rndPosWithin+transform.forward*1.5f, transform.rotation) as GameObject;    
			//newUnit.GetComponent<unit>().myTarget = spawningUnitsTarget[Random.Range(0,spawningUnitsTarget.Count)];
			//newUnit.GetComponent<unit>().setTargetDirection(transform.forward);

		}

	}

	void OnTriggerEnter(Collider other) {
//Debug.Log("The guest had the target"+other.transform.GetComponent<unit>().myTarget);
	//	Debug.Log(transform.name +"is my name, and my target list was" +gateTarget[0]+" and "+gateTarget[1]+ "  with a total so many entries: "+gateTarget.Count);

		//Debug.Break();
		//Debug.Break();
		/*if (other.transform.GetComponent<unit>() != null) {
			if(gateTarget.Contains(other.transform.GetComponent<unit>().myTarget)){
				
			Destroy(other.gameObject);
			}
		}*/
	}
}
