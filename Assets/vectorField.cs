using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

[System.Serializable]
public enum fType
{
	none,
	floor,
	wall,
	exit
};
[System.Serializable]
public struct tile 
{
	public int x, z;
	public fType type;   //0 for floor ; 1 for wall
	public Vector3 vec;

};

[System.Serializable]
public class vectorField 
{


	private	tile[,] tiles;

	public List<tile> tilesStorage;
	//public Vector2[,] location;
	//public fType[,] wallType;
	//public Vector3[,] vec;


	[SerializeField]
	public int overSampling;
	public List<Vector2> wallMem = new List<Vector2>();
	// This is a helper list to move from processing to memory since our game assets willl only use very limited  memory we can use it to stopre additional infos abou the scene

	//private vectorFHelper myHelper;
	float vecHeight;
	//float wallRadius;
	[SerializeField]
	private int width;
	[SerializeField]
	private int height;

	public bool recalcVecField = false;
	public bool debug = false;


	// Use this for initialization
	public vectorField(int width_, int height_,int overSampling_, float  vecHeight_, float  wallRadius_){

		//tiles= tiles_;
	
		vecHeight = vecHeight_;
		//wallRadius = wallRadius_;
		width = width_;
		height = height_;
		overSampling = overSampling_;
		//myHelper = new vectorFHelper(this, wallRadius, vecHeight, overSampling, Mathf.RoundToInt(transform.GetComponent<Collider>().bounds.size.x) * overSampling, Mathf.RoundToInt(transform.GetComponent<Collider>().bounds.size.z) * overSampling);
	
	tiles = new tile[(width * overSampling),( height * overSampling)];


		for (int i = 0; i < tiles.GetLength(0); i++) {
			for (int j = 0; j < tiles.GetLength(1); j++) {
				tiles[i, j].vec = Vector3.zero;
				tiles[i, j].type = fType.floor;
				tiles[i, j].x = i;
				tiles[i, j].z = j;

				//location[i, j] = new Vector2(i, j);
				//wallType[i, j]=fType.floor;
				//vec[i,j] = Vector3.zero;
			}
		}

		//Debug.Log("This needs to say (1.0,1.0,1.0): " + arraySpaceToWorld(worldSpaceToArray(new Vector3(1, 1, 1))));// sort of a unit test

	}
	public void Update(GameObject floor)
	{
		if (debug) {
			tile[] walls = (from w in tiles.Cast<tile>()
			                where w.vec != Vector3.zero
			                select w).ToArray();
			foreach (tile w in walls) {
				Debug.DrawLine(aSTWS(w.x, w.z, floor), aSTWS(w.x, w.z, floor) + (Vector3.up * 0.1f), Color.green);
				Debug.DrawLine(aSTWS(w.x, w.z, floor), aSTWS(w.x, w.z, floor) + w.vec);

			}
		}

	}
	public void storeValues(){
		Debug.Log("Safe was called");
		//Debug.Log(tiles[96,96].type);
		tilesStorage = new List<tile>();

			for (int x = 0; x < tiles.GetLength(0); x++) {
				for (int z = 0; z < tiles.GetLength(1); z++) {
					tilesStorage.Add(tiles[x,z]);
				}

			}


	
	
	}// call this in editor time after everything is done... and calculated;
	public void loadValues(){



		tiles = new tile[(width * overSampling),( height * overSampling)];
		tile[] temp =new tile[tilesStorage.Count];
		tilesStorage.CopyTo(temp);
		foreach (tile t in temp) {
			tiles[t.x, t.z] = t;
			//Debug.Log("While Copy t.type is"+t.type+"   and tiles.type is"+tiles[t.x, t.z].type);
		}
		/*
		for (int x = 0; x < tiles.GetLength(0); x++) {
			for (int z = 0; z < tiles.GetLength(1); z++) {
				
				tiles[x,z] = tilesStorage[x+z*width];
			}

		}*/

		//for (int x = 0; x < tiles.GetLength(0); x++) {
	//		for (int z = 0; z < tiles.GetLength(1); z++) {
	//			Debug.Log(tiles[x,z].type);
	//		}

	//	}

	

	}//call this in run time to load the data into the tiles agains

	public Vector3 worldSpaceToArray(Vector3 vec)
	{/// one aspect that both of these functions do not take into account is offset from the center if the floor is not at 0 0 0  then it gets confused probably  its an easy fix

		Debug.Log("NOT IMPLEMENTED STOP");
		//return new Vector3((vec.x - (0.5f / overSampling) + (transform.GetComponent<Collider>().bounds.size.x / 2)) * overSampling, vec.y, (vec.z - (0.5f / overSampling) + (transform.GetComponent<Collider>().bounds.size.z / 2)) * overSampling);
		return  Vector3.zero;
	}

	public Vector3 arraySpaceToWorld(Vector3 vec, GameObject floor)
	{
		//return new Vector3((vec.x / overSampling + (0.5f / overSampling) - (transform.GetComponent<Collider>().bounds.size.x / 2)), vec.y, (vec.z / overSampling + (0.5f / overSampling) - (transform.GetComponent<Collider>().bounds.size.z / 2)));
	
		return new Vector3(vec.x / overSampling + (0.5f / overSampling) - (width / 2), vec.y, (vec.z / overSampling + (0.5f / overSampling) - (height / 2))) + (floor.transform.position / 2);
	}

	public Vector3 aSTWS(int x, int z,GameObject floor)
	{
		return arraySpaceToWorld(new Vector3(x, 1, z),floor);
	}

	public void addWallGameobjects(GameObject wallPrefab, GameObject floor){

		foreach (Vector2 v in wallMem){
			GameObject wall = GameObject.Instantiate(wallPrefab, arraySpaceToWorld(new Vector3(v.x, vecHeight, v.y ),floor), Quaternion.identity) as GameObject;
				wall.transform.localScale=new Vector3((1.0f/overSampling), 1.0f,(1.0f/overSampling));
		wall.transform.parent = floor.transform;

		}
	}
	public void addWIC(int x, int z ,vectorFHelper myHelper)
	{
		int startx = (x*overSampling);
		int endx = (x*overSampling)+overSampling;
		int startz = (z*overSampling);
		int endz =(z*overSampling)+overSampling;

		for (int i = startx; i < endx; i++) {
			for (int j = startz; j < endz; j++) {
				addWall(i, j,false ,myHelper);
			}
		}

		recalcVecField = true;
	}

	public Vector3 getMotionVector(Vector3 pos, bool complex=false)
	{

			pos = worldSpaceToArray(pos);
			int x = Mathf.RoundToInt(pos.x);
			int z = Mathf.RoundToInt(pos.z);
			if (validValues(x, z)) {
				return tiles[x,z].vec*overSampling;

			} else {
				return Vector3.zero;
			}
	
	}
	bool addVector(int x, int z, Vector3 vecIn)
	{
		if (validValues(x, z)) {
			if (!(tiles[x, z].type == fType.wall)) { //
				tiles[x, z].vec = vecIn;
				return true;
			}
		}
		return false;
	}

	Vector3 scaledMotionVector(int x, int z, Vector3 pos)
	{
		if (validValues(x, z)) {
			float magnitude = (new Vector2(x, z) - new Vector2(pos.x, pos.z)).magnitude;
			return tiles[x, z].vec * Mathf.Clamp01(1 - magnitude);
		} else {
			return Vector3.zero;
		}
	}

	void addWall(int x, int z,bool instansiateWall  ,vectorFHelper myHelper)
	{
		if (validValues(x, z) && (!(tiles[x, z].type == fType.wall))) { //if (validValues(x, z) && (!(tiles[x, z].type == fType.wall))) {
			
			wallMem.Add(new Vector2(x, z));
			myHelper.addWall(new Vector2(x, z));

			tiles[x, z].type =  fType.wall;
			tiles[x, z].vec = Vector3.zero;
			if (instansiateWall) {
				// we need to add code here that adds just one wall.

				///This is old code
				//GameObject wall = GameObject.Instantiate(wallPrefab, arraySpaceToWorld(new Vector3(x, 1, z)), Quaternion.identity) as GameObject;
				//wall.transform.localScale=new Vector3((1.0f / overSampling), 1,(1.0f / overSampling));
				//wall.transform.parent = this.transform;


			}
		}
	}


	bool getWall(int x, int z)
	{
		if (validValues(x, z)) {
			if ((tiles[x, z].type == fType.wall)) { //if ((tiles[x, z].type == fType.wall)) {
				return true;
			}
		}
		return false;
	}

	public bool validValues(int x, int z)
	{
		if ((x >= 0 && x <= tiles.GetLength(0) - 1) && (z >= 0 && z <= tiles.GetLength(1) - 1)) {
			return true;
		} else {
			return false;
		}


	}

	public tile[,] getVectors()
	{

		return tiles;

	}

	public void applyNewVectors(tile[,] incomming)
	{
		if (tiles.GetLength(0) == incomming.GetLength(0) && tiles.GetLength(1) == incomming.GetLength(1)) {
			tiles = (tile[,])incomming.Clone();
			Debug.Log("we just got new Vectors");
		}


	}
// public void saveVectors(){


//		GameObject.FindObjectOfType<levelManager>().storeVectorField(tiles);
//	}

	public void applyNewVectorList(Vector2[] newPos, Vector3[] newVectors)
	{
		Debug.Log("We are saving the new Vectors. Ammount: "+ newPos.Count()+ "  ");
		if (newPos.Length == newVectors.Length) {
			for (int i = 0; i < newPos.Length; i++) {
				tiles[(int)newPos[i].x, (int)newPos[i].y].vec = newVectors[i];
			//	Debug.Log("we got new Vectors" + newVectors[i].magnitude);
			}
		} else {
			Debug.Log("Miss matche between positions and incomming new vectors ");
			Debug.Break();
		}

	}
	void OnApplicationQuit()
	{
	//	float endTimer = Time.time;
		//myHelper.stop();  /// Old Version where we needed to close it on run time

	//	Debug.Log("Application ending after: " + Time.time + " seconds. The ending seqeunc took: " + (Time.time - endTimer));
	}


}

public class vectorFHelper
{
	float vecHeight = 1;
	int overSampling = 4;

	private volatile bool active = true;
	//private volatile bool runCalculation = false;
	private volatile bool changeOccured = false;
	private volatile List<Vector2> inCommingWall = new List<Vector2>();
	int maxX;
	int maxZ;
	vectorField host;
	float wallRadius;

	int endlessRunningSafe=0;

	private volatile int changeacaptCounter=0;
	Thread recalcThread;
	public int getInCommingWallCount(){

		return inCommingWall.Count;
	}
		
	public bool stop()
	{
		//changeOccured = true;
		//runCalculation = false;
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) {
			
			active = false;
			recalcThread.Join();
		}
		//recalcThread2.Join();
		return true;
	}

	public vectorFHelper(vectorField host_, float wallRadius_, float vecHeight_, int overSampling_, int maxX_, int maxZ_)
	{
		maxX = maxX_;
		maxZ = maxZ_;
		host = host_;
		wallRadius = wallRadius_;
		vecHeight = vecHeight_;
		overSampling = overSampling_;
		//Debug.Log("max x is: " + maxX + " max z is: " + maxZ);
		//recalcThread2 = new Thread(new ThreadStart(calcVectorField));
		//arecalcThread2.Start();
	}public void start(){
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) {

			recalcThread = new Thread(new ThreadStart(calcVectorField));
			recalcThread.Start();
			active = true;
			Debug.Log("Started Thread now lets see, we have "+getInCommingWallCount()+"waiting to be calculated");

		} else {
			active = false;
		}

	}


	public void halt(){
		

		active = false; //the thread will close;
		Debug.Log("The queue should be empty: "+inCommingWall.Count);
	}
	public void addWall(Vector2 incomming)
		{
		if (active) {
			inCommingWall.Add(incomming);
			//runCalculation = true;
			changeOccured = true;
			changeacaptCounter = 0;
		}
	}

	bool validValues(int x, int z)
	{
		if ((x >= 0 && x <= maxX - 1) && (z >= 0 && z <= maxZ - 1)) {
			return true;
		} else {
			return false;
		}


	}

	public void calcVectorField()
	{
		

		System.Random rnd = new System.Random();
		//Debug.Log ("ThreadStart");
		/// OPTIMIZATION you could check if you find any changes in the cetor calculation and just not return new values... that way you do not need to interput the main thread
		List<Vector2> visitedPos = new List<Vector2>();
		List<Vector3> visitedVectors = new List<Vector3>();

		List<Vector2> newPos = new List<Vector2>();
		List<Vector3> newVectors = new List<Vector3>();

		List<Vector2> neighbours = new List<Vector2>();
		List<Vector2> relevantPoints = new List<Vector2>();
		int savedItterations = 0;
		bool saved = false;
		while (active) {
			Thread.Sleep(150);
			if (inCommingWall.Count == 0) {
				endlessRunningSafe++;
				if (endlessRunningSafe > 500) {
					Debug.Log("It does not look like there is something comming. Gonna Stop for now.");
					stop();

				}
			}
			Debug.Log("Just sleeped... gona check on new data");
			while (inCommingWall.Count > 0) {
				Debug.Log("Great! Got work todo! still open "+ inCommingWall.Count);
				//tile[,] tiles = (tile[,])host.getVectors().Clone();

				Vector2 wall = inCommingWall.RemoveAndGet(rnd.Next(inCommingWall.Count - 1));
				neighbours.Clear();
				neighbours = getRelevantVecs(new Vector3(wall.x, vecHeight, wall.y), wallRadius);
				//Debug.Log("For how many neighbours are we looking" + neighbours.Count);
				foreach (Vector2 vn in neighbours) {
					if (changeOccured) {
						saved = false;
						visitedPos.Clear();
						visitedVectors.Clear();
						changeOccured=false;
					//	Debug.Log("Supposedly we saved: "+savedItterations);
						savedItterations = 0;
					} 
				//	Debug.Log("Blub");
					if (!visitedPos.Contains(vn)) {//TODO I dont't know why this is the way it is  Old : // if (!host.wallMem.Contains(vn) && !visitedPos.Contains(vn)) {
						//Debug.Log("a new visiter");
						int counter = 0;
						relevantPoints.Clear();
						relevantPoints = getRelevantVecs(new Vector3(vn.x, vecHeight, vn.y), wallRadius);
						Vector3 newVec = Vector3.zero;
						foreach (Vector2 v in relevantPoints) {
							if (host.wallMem.Contains(v)) {	
								newVec += (((new Vector3(vn.x, vecHeight, vn.y) - new Vector3(v.x, vecHeight, v.y)).normalized) / Mathf.Pow((new Vector3(vn.x, vecHeight, vn.y) - new Vector3(v.x, vecHeight, v.y)).magnitude, 2));
								counter++;
								//Debug.Log(newVec);
							}
						}	
						//Debug.Log("We calculated for count:" + counter);
						if (counter > 0) {
							newVec /= counter;
							newVec /= overSampling;
							if (validValues((int)(vn.x), (int)(vn.y))) {
								newPos.Add(vn);
								newVectors.Add(newVec);

								visitedPos.Add(vn);
								visitedVectors.Add(newVec);

							}
						}
					} else if(visitedPos.Contains(vn)){
						//Debug.Log("visited already adding it to the list");
						if (validValues((int)(vn.x), (int)(vn.y))) {
							newPos.Add(vn);
							newVectors.Add(visitedVectors[visitedPos.IndexOf(vn)]);
							savedItterations++;
							//Debug.Log("Added exsisting");
						
						}
					}
					//Debug.Log("NextVisitor");
				}

				//Debug.Log("thats all for now dumping data");
				float mag = 0;
				foreach (Vector3 v in newVectors) {
					if (v.magnitude > mag) {
						mag = v.magnitude;
					}
				}
				if (mag != (1 / overSampling)) {
					for (int i = 0; i < newVectors.Count; i++) {
						newVectors[i] *= (1 / (mag * overSampling));
						//Debug.Log((1 / (mag * overSampling)));
					}
				}


				applyNewVectorsToPoints(newPos.ToArray(), newVectors.ToArray());
				newPos.Clear();
				newVectors.Clear();

				//Debug.Log ("OK! I am all done");
				if (!active) {
					break;
				}
			}
			if (!saved) {
				//saveNewVectors();
				saved = true;
			}

		

		}
	}
	bool changeWaitForSync(){
		if (changeacaptCounter == 2) {
			changeOccured = false;	
			return true;
		} else {
			return false;
		}

	}
	//void saveNewVectors(){
	//	host.saveVectors();

	//}
	void changeAccapted(){
		changeacaptCounter++;
	}
	void applyNewVectorsToHost(tile[,] tiles)
	{
		host.applyNewVectors(tiles);
	}

	void applyNewVectorsToPoints(Vector2[] newPos, Vector3[] newVectors)
	{

		host.applyNewVectorList(newPos, newVectors);
		//runCalculation = false;


	}

	public List<Vector2> getRelevantVecs(Vector3 pos, float radius)
	{// local space
		float inX = pos.x;
		float inZ = pos.z;
		List<Vector2> output = new List<Vector2>();
		Debug.Log (pos);

		for (float r = radius; r > 0; r -= 1.0f) {
			float step = (Mathf.PI / 2) / (r * 2.0f);
			for (float a = 0; a < (2 * Mathf.PI); a += step) {
				int x = Mathf.RoundToInt(inX + r * Mathf.Cos(a));
				int z = Mathf.RoundToInt(inZ + r * Mathf.Sin(a));
				Vector2 outVec = new Vector2(x, z);
				//Debug.Log ("x:" + x + "\tz:" +z+ "\tr:" + r + "\ta:" + a+ "\t dist"+(new Vector2 (inX, inZ) - outVec).magnitude);
				if ((new Vector2(inX, inZ) - outVec).magnitude <= radius && !output.Contains(outVec)) {
					//Debug.Log ("x:" + x + "\tz:" +z+ "\tr:" + r + "\ta:" + a+ "\t dist"+(new Vector2 (inX, inZ) - outVec).magnitude);

					if (validValues(x, z)) {
						output.Add(outVec);
					}
				}


			}

		}

		if (host.validValues(Mathf.RoundToInt(inX), Mathf.RoundToInt(inZ))) {
			output.Add(new Vector2(Mathf.Round(inX), Mathf.Round(inZ)));
		}
		//Debug.Log (output.Count);
		return output;

	}


}


public static class Extensions
{
	public static T RemoveAndGet<T>(this IList<T> list, int index)
	{
		lock (list) {
			T value = list[index];
			list.RemoveAt(index);
			return value;
		}
	}
}