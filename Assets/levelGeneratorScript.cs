using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class levelGeneratorScript : EditorWindow {
	//public levelStorage storageToBeAdjusted;

	private vectorFHelper myHelper;
	[MenuItem ("Window/levelGenerator")]
	static void  Init () 
	{
		EditorWindow.GetWindow (typeof(levelGeneratorScript));
	}
	void  OnEnable () {
		

	}
	// Use this for initialization
	void OnGUI () {
		//GUILayout.BeginHorizontal ();
		GUILayout.Label ("Level Data Generation Based on a selected Image", EditorStyles.boldLabel);
		if (GUILayout.Button("Run on Selected LevelStorageObject", GUILayout.ExpandWidth(false))) 
		{
			runCalc();
		}
		//GUILayout.EndHorizontal();
	}

	void runCalc(){
		if (Selection.activeObject.GetType() == typeof(Texture2D)) {


		
			Debug.Log("We are going to generate the level from the image: "+Selection.activeObject.name);

			Texture2D levelImage = Selection.GetFiltered(typeof(Texture2D), SelectionMode.TopLevel)[0] as Texture2D;

			//levelStorage newLevel = new levelStorage()
			//levelStorage myLevelStorage = Selection.GetFiltered(typeof(levelStorage), SelectionMode.TopLevel)[0] as levelStorage;



			levelStorage newLevel = ScriptableObject.CreateInstance<levelStorage>();
			//newLevel.levelImage = levelImage;

			newLevel.initialize(4,1,6,levelImage);

			AssetDatabase.CreateAsset(newLevel, "Assets/generatedLevels/"+Selection.activeObject.name+".asset");

			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = newLevel;





			//myLevelStorage.myVectorField = new vectorField(myLevelStorage.levelImage.width,myLevelStorage.levelImage.height,myLevelStorage.overSampling,myLevelStorage.vecHeight,myLevelStorage.wallRadius);
			//vectorFHelper myHelper = new vectorFHelper(myLevelStorage.myVectorField, myLevelStorage.wallRadius, myLevelStorage.vecHeight, myLevelStorage.overSampling, myLevelStorage.levelImage.width * myLevelStorage.overSampling, myLevelStorage.levelImage.height * myLevelStorage.overSampling);



			/*
			for (int x=0;x<myLevelStorage.levelImage.width;x++){
				for (int y = 0; y < myLevelStorage.levelImage.height; y++) {
				//Debug.Log(tex.GetPixel(x, y));
					if (myLevelStorage.levelImage.GetPixel(x, y) == new Color(0, 0, 0)) {
						myLevelStorage.myVectorField.addWIC(x, y, myHelper);
				}
			}
	

		}
			myHelper.start();
			Debug.Log(myLevelStorage.myVectorField.tiles.Length + "Length of the tiles");
			Debug.Log(myLevelStorage.myVectorField.wallMem.Count + "Count of the Walls");


			myHelper.halt();
			//myHelper.stop();/// we need to tell the thread to stop calulating
*/
		}


		}

}
