using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[CreateAssetMenu(fileName = "Level000", menuName = "levelStorage", order = 1)]
[System.Serializable]
public class levelStorage : ScriptableObject {

	public int overSampling = 4;
	public float vecHeight = 1f;
	public float wallRadius = 8;

	public vectorField myVectorField;
	public Texture2D levelImage;
	public List<GameObject> gates;



	void OnEnable(){
	}

	public void initialize(int _overSampling,int _vecHeight,int _wallRadius,Texture2D _levelImage){
		overSampling = _overSampling;
		vecHeight = _vecHeight;
		wallRadius = _wallRadius;
		levelImage = _levelImage;
		Debug.Log("Starting to Initialize?");
		//first we create the actual vecot field class;
		myVectorField = new vectorField(_levelImage.width, _levelImage.height, _overSampling, _vecHeight, _wallRadius);
		//now we need to add the walls

		vectorFHelper newVFHelper= new vectorFHelper(myVectorField, _wallRadius, _vecHeight, _overSampling, levelImage.width*_overSampling, levelImage.height*_overSampling);

		for (int x = 0; x < levelImage.width; x++) {
			for (int z = 0; z < levelImage.height; z++) {
				if (levelImage.GetPixel(x, z) == Color.black) {
					myVectorField.addWIC(x, z, newVFHelper);

				}
			}
		}
		newVFHelper.start();
		while (newVFHelper.getInCommingWallCount() > 0) {
			;
		}
		myVectorField.storeValues();
		newVFHelper.stop();
		Debug.Log("During runtime wall Count"+myVectorField.wallMem.Count);
	}


	public void onLoadDo(){

		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = new Vector3(0, 0, 0);
		cube.transform.localScale = new Vector3(levelImage.height, 0.1f, levelImage.width);



		GameObject MainLevel = new GameObject();
		MainLevel.name = "LevelHost";
		levelHandler tempLevelHandler = MainLevel.AddComponent<levelHandler>()as levelHandler;

		cube.transform.parent = MainLevel.transform;

		tempLevelHandler.assignData(this);



		//TODO: add loading in the texture of the floor.

		//Load in the walls from the vectorfield and display them;



	}

}