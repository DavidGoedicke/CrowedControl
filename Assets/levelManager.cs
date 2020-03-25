using UnityEngine;
using System.Collections;
using System.IO;

public class levelManager : MonoBehaviour {
	public levelStorage[] levels;
	public int levelID=0;
	public bool loadNewLevel = false;
	// Use this for initialization
	void Start () {
		//Debug.Log(levels[0].GetPixel(0, 0)); 



	
	}
	// Update is called once per frame
	void Update () {
		if (loadNewLevel) {
			loadNewLevel = false;
			if (levelID <= levels.Length) {
				//Debug.Log(levels[levelID].myVectorField.wallMem.Count);
				levels[levelID].onLoadDo();

				//applyTexture(levels[levelID]);

			}
		}
		//levels[levelID].myVectorField.Update(floor);
	
	}

	/*void applyTexture(Texture2D tex){
		 
		for (int x = 0; x < tex.width; x++) {
			for (int y = 0; y < tex.height; y++) {
				//Debug.Log(tex.GetPixel(x, y));
				if (tex.GetPixel(x, y) == new Color(0,0,0)) {
					floor.transform.GetComponent<vectorField>().addWIC(x, y);
				}
			}
		}
	}*/

	public void nextLevel(){



	}

	public void prevLevel(){

	}
	public void storeVectorField(tile[,] tiles){

		/*
		using(var writer = new BinaryWriter(File.OpenWrite(@"/Volumes/HDD/tiles")))
		{
			writer.Write(tiles.ToString());
			writer.Close();
		}
	*/

	}


}
