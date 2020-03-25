using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levelHandler : MonoBehaviour {
	levelStorage loadedStorage;

	public vectorField myVectorField;
	public List<GameObject> gates;
	// Use this for initialization
	void Start () {
		
	}
	public void assignData(levelStorage loadedStorage_){
		loadedStorage = loadedStorage_;
		myVectorField=loadedStorage.myVectorField;
		gates=loadedStorage.gates;
		myVectorField.loadValues();


	
		int wallCount = myVectorField.wallMem.Count;
		int minimumX=0;
		int minimumZ =0;
		int maximumX =0;
		int maximumZ =0;
		int wallID = 0;
		int emergencyCounter=0;
		Vector2Int[] wallMemory;
		Vector2Int[] toBeDeleted
		while (wallCount > 0) {
			emergencyCounter++;
			Debug.Log("This wallId should go nothing but up and jump in between"+wallID);
			if (wallID  < myVectorField.wallMem.Count) {
					
				minimumX = Mathf.RoundToInt(myVectorField.wallMem[wallID].x);
				minimumZ = Mathf.RoundToInt(myVectorField.wallMem[wallID].y);
				maximumX = minimumX;
				maximumZ = minimumZ;

					
			} else {
				Debug.Log("Breaking since we are moving out of the bounds");
				break;

			}

			int targetX = minimumX;
			bool firstRun = true;
			bool borderFound = false;
			while (myVectorField.wallMem.Contains(new Vector2(targetX, maximumZ))) {
				while (myVectorField.wallMem.Contains(new Vector2(targetX, maximumZ))) {
					//Debug.Log("TargetX" + targetX + "maximumZ" + maximumZ);	
					targetX++;
					if (!firstRun && targetX >= maximumX) {
						borderFound = true;
						break;
					}
				}
				if (firstRun) {
					firstRun = false;
					maximumX = targetX-1;
					targetX = minimumX;
				} else if (borderFound) {
					borderFound = false;
					targetX = minimumX;
					maximumZ++;
				} else {
					break;	
				}
			}
			Debug.Log("Blub  new wall..."+maximumX+" z: " + maximumZ);
			//maximumX--;
			maximumZ--;
			Debug.Log("Wall Id retrieved from the last wall" + myVectorField.wallMem.FindIndex( v=> maximumX==v.x && maximumZ==v.y));

			wallID = myVectorField.wallMem.IndexOf(new Vector2(maximumX, maximumZ));
			wallID++;
			GameObject newWall = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
			newWall.transform.localScale = new Vector3(maximumX - minimumX, 2, maximumZ - minimumZ)/myVectorField.overSampling;
			newWall.transform.position = myVectorField.aSTWS((minimumX + (maximumX / 2)), (minimumZ + (maximumZ / 2)), transform.GetChild(0).gameObject);




			if (emergencyCounter > myVectorField.wallMem.Count) {
				Debug.Log("Pulling the emergency break this should not happen");
				break;
			}
		}


	}



	// Update is called once per frame
	void Update () {
		myVectorField.Update(transform.GetChild(0).gameObject);
	}
}
