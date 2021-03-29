using UnityEngine;
using System.Collections;
using System.IO;

public class levelManager : MonoBehaviour {

	public int levelID=0;
	public bool loadNewLevel = false;
	// Use this for initialization
	void Start () {
	
	}
	// Update is called once per frame
	void Update () {
		if (loadNewLevel) {
			loadNewLevel = false;
			
		}
	}

	public void nextLevel(){
	}

	public void prevLevel(){
	}
	public void storeVectorField(tile[,] tiles){
   
	}


}
