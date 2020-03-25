// using UnityEngine;
// using System.Collections;
// public enum fType {none,floor,wall,exit};
// public struct tile{
// 	public fType type;
// 	public Vector3 vec;


// };

// public class vectorField : MonoBehaviour {
// 	public 	tile[,] tiles;
// 	public GameObject wallPrefab;

// 	// Use this for initialization
// 	void Start () {

// 		tiles = new tile[(int)transform.GetComponent<Collider>().bounds.size.x, (int)transform.GetComponent<Collider>().bounds.size.z];

// 		for(int i=0;i<  tiles.GetLength(0);i++){
// 			for (int j=0;j<tiles.GetLength(1); j++){
// 				tiles[i, j].vec = Vector3.zero;
// 				tiles[i, j].type = fType.floor;
// 			}
// 		}
// 		//Debug.Log(tiles[249,249].vec+"  and   "+tiles[249,249].type);

// 		addWall(0, 0);
// 		addWall(9, 0);
// 		addWall(0, 9);
// 		addWall(8, 8);
// 		addWall(9, 9);
// 		addWall(4, 4);
// 		recalcVectorField ();
// 		//Debug.Log(tiles[199, 199].vec.ToString() + "  199 and now 201" + tiles[201, 201].vec.ToString());
// 	}
	
// 	// Update is called once per frame
// 	void Update () {
// 		for (int i = 0; i < tiles.GetLength(0); i++) {
// 			for (int j = 0; j < tiles.GetLength(1); j++) {
// 				if (tiles[i, j].vec != Vector3.zero) {
// 					Debug.DrawLine(new Vector3(0.5f+i-((int)transform.GetComponent<Collider>().bounds.size.x/2), 1, 0.5f+j-((int)transform.GetComponent<Collider>().bounds.size.z/2)), new Vector3(0.5f+i-((int)transform.GetComponent<Collider>().bounds.size.x/2), 1, 0.5f+j-((int)transform.GetComponent<Collider>().bounds.size.z/2)) + tiles[i, j].vec);
// 				}
			
// 			}
// 		}

// 	}
// 	public Vector3 worldSpaceToArray(Vector3 vec){
// 		return new Vector3 (vec.x - 0.5f + (transform.GetComponent<Collider> ().bounds.size.x / 2), vec.y, vec.z - 0.5f + (transform.GetComponent<Collider> ().bounds.size.z / 2));
// 	}

// 	public void addWWC(Vector3 vecIn){
// 		Vector3 localVec = worldSpaceToArray (vecIn);
// 		int x = Mathf.RoundToInt (localVec.x);
// 		int z = Mathf.RoundToInt (localVec.z);
	
// 		addWall (x-1,z-1);
// 		addWall (x,z-1);
// 		addWall (x+1,z-1);

// 		addWall (x-1,z);
// 		addWall (x,z);
// 		addWall (x+1,z);

// 		addWall (x-1,z+1);
// 		addWall (x,z+1);
// 		addWall (x+1,z+1);

// 		recalcVectorField();
// 	}

// 	bool addVector(int x, int z, Vector3 vecIn){
// 		if (( x >= 0 && x <= tiles.GetLength(0) - 1)&& ( z >= 0 && z <= tiles.GetLength(1) - 1)) {
// 			if (! (tiles[x, z].type == fType.wall)) {
// 				tiles[x, z].vec = vecIn.normalized;
// 				return true;
// 			}
// 		}
// 		return false;
// 	}
// 	Vector3 scaledMotionVector(int x, int z, Vector3 pos){
// 		if (( x >= 0 && x <= tiles.GetLength(0) - 1)&& ( z >= 0 && z <= tiles.GetLength(1) - 1)) {
// 			float magnitude = (new Vector2 (x, z) - new Vector2 (pos.x, pos.z)).magnitude;
// 			return tiles [x, z].vec * Mathf.Clamp01 (1 - magnitude);
// 		}
// 		else {
// 			return Vector3.zero;
// 		}
// 	}
// 	public Vector3 getMotionVector(Vector3 pos){
// 		pos = worldSpaceToArray (pos);
		 
// 		int x = Mathf.RoundToInt (pos.x);
// 		int z = Mathf.RoundToInt (pos.z);

// 		Vector3[] output = new Vector3[9];

// 		output [0] = scaledMotionVector (x-1, z-1, pos);
// 		output [1] = scaledMotionVector (x,   z-1, pos);
// 		output [2] = scaledMotionVector (x+1, z-1, pos);

// 		output [3] = scaledMotionVector (x-1, z, pos);
// 		output [4] = scaledMotionVector (x,   z, pos);
// 		output [5] = scaledMotionVector (x+1, z, pos);

// 		output [6] = scaledMotionVector (x-1, z+1, pos);
// 		output [7] = scaledMotionVector (x,   z+1, pos);
// 		output [8] = scaledMotionVector (x+1, z+1, pos);

// 		int counter = 0;
// 		Vector3 product = Vector3.zero;
// 		foreach (Vector3 vec in output) {
// 			if (vec != Vector3.zero) {
// 				counter++;
// 				product += vec;
// 			}
// 		}
// 		if (counter > 0) {
// 			product /= counter;
// 		}
// 		return product; 
// 	}


// 	 void addWall(int x, int z){
// 		if ((x >= 0 && x <= tiles.GetLength(0) - 1) && (z >= 0 && z <= tiles.GetLength(1) - 1) &&(! (tiles[x, z].type == fType.wall)) ) {
// 			tiles[x, z].type = fType.wall;
// 			tiles[x, z].vec = Vector3.zero;

// 			/*
// 			addVector(x - 1, z - 1, new Vector3(-1, 0,-1));
// 			addVector(x, z - 1, new Vector3(0, 0, -1));
// 			addVector(x + 1, z - 1, new Vector3(1, 0, -1));

// 			addVector(x - 1, z, new Vector3(-1, 0, 0));
// 			addVector(x + 1, z, new Vector3(1, 0, 0));

// 			addVector(x - 1, z + 1, new Vector3(-1, 0, 1));
// 			addVector(x, z + 1, new Vector3(0, 0, 1));
// 			addVector(x + 1, z + 1, new Vector3(1, 0, 1));
// 			*/

// 			GameObject thingy = GameObject.Instantiate(wallPrefab, new Vector3(0.5f+x-((int)transform.GetComponent<Collider>().bounds.size.x/2), 1, 0.5f+ z-((int)transform.GetComponent<Collider>().bounds.size.z/2)), Quaternion.identity) as GameObject;


// 			//thingy.transform.position = new Vector3 (0.5f + x - ((int)transform.GetComponent<Collider> ().bounds.size.x / 2), 1, 0.5f + z - ((int)transform.GetComponent<Collider> ().bounds.size.z / 2));
// 			thingy.transform.parent = this.transform;
// 		}
// 	}
// 	bool getWall(int x, int z){
// 		if (( x >= 0 && x <= tiles.GetLength(0) - 1)&& ( z >= 0 && z <= tiles.GetLength(1) - 1)) {
// 			if ((tiles[x, z].type == fType.wall)) {
// 				return true;
// 			}
// 		}
// 		return false;
// 	}
// 	public bool recalcVectorField(){ /// returns true if changes where made to the Vectorfield false if not.
// 		bool result = false;
// 		for (int i = 0; i < tiles.GetLength(0); i++) {
// 			for (int j = 0; j < tiles.GetLength(1); j++) {
// 				if (tiles[i, j].type == fType.floor) {
// 					Vector3 newVec = Vector3.zero;
// 					if(getWall(i - 1, j - 1)) newVec += new Vector3 (1, 0, 1);
// 					if(getWall(i 	, j - 1)) newVec += new Vector3 (0, 0, 1);
// 					if(getWall(i + 1, j - 1)) newVec += new Vector3 (-1, 0, 1);

// 					if(getWall(i - 1, j )) newVec += new Vector3 (1, 0, 0);
// 					if(getWall(i + 1, j )) newVec += new Vector3 (-1, 0, 0);

// 					if(getWall(i - 1, j + 1)) newVec += new Vector3 (1, 0, -1);
// 					if(getWall(i 	, j + 1)) newVec += new Vector3 (0, 0, -1);
// 					if(getWall(i + 1, j + 1)) newVec += new Vector3 (-1, 0, -1);
// 					if (newVec.normalized != tiles [i, j].vec) {
// 						result = true;
// 						addVector (i, j, newVec);
// 					}

// 				}

// 			}
// 		}

// 		return result;
// 	}
// }


// /*
// if (x == 0) {
// 	if (y == 0) {
// 		tiles[x+1,y+1]

// 	} else if (y > 0 && y < tiles.GetLength(1) - 1) {


// 	} else if (y == tiles.GetLength(1) - 1) {


// 	}
// 	else {
// 		Debug.Break("This should not happen ");
// 	}

// } else if (x > 0 && x < tiles.GetLength(0) - 1) {
// 	if (y == 0) {


// 	} else if (y > 0 && y < tiles.GetLength(1) - 1) {


// 	} else if (y == tiles.GetLength(1) - 1) {


// 	}
// 	else {
// 		Debug.Break("This should not happen ");
// 	}



// } else if (x == tiles.GetLength(0) - 1) {
// 	if (y == 0) {


// 	} else if (y > 0 && y < tiles.GetLength(1) - 1) {


// 	} else if (y == tiles.GetLength(1) - 1) {


// 	}
// 	else {
// 		Debug.Break("This should not happen ");
// 	}



// } else {
// 	Debug.Break("This should not happen ");
// }

// */