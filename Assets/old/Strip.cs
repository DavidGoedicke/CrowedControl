using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Strip : MonoBehaviour {
		int maximumLineLength = 50;
		List <Vector3> points;
		Color myColor;
		LineRenderer myLineRenderer;
	public bool done;
	public Vector3 getColsestWaypoint(){

		return points [0];


	}
	public Vector3 getNextWaypoint (Vector3 position){

		float distance = (points [0] - position).magnitude;
		int count = 0;
		int target = 0;
		foreach(Vector3 v in points){
			if ((v - position).magnitude < distance) {
				distance = (v - position).magnitude;
				target = count;
			}

			count++;
			}


		if (target == 0) {
			return points [1] - points [0];

		} else if (target >= 1 && target <= points.Count - 2) {
			return (points [target+1] - points [target-1])/2;


		} else if (target >= points.Count - 1) {

			return points [points.Count - 1] - points [points.Count - 2];
		} else {
			Debug.Log ("Should not happen!");
			return Vector3.zero;
		}


		/* // This is an old mthod where we returned the next possible position in this "list of waypoints"
		if (target == points.Count - 1) {

			Debug.Log ("option1");
			Vector3 direction = points [target] - points [target-1];
			return (position+direction);
		} else if (target < points.Count - 1) {
			Debug.Log ("option2");
			if (distance <= 2 || target == points.Count - 2) {
				return points [target++];
			} else {

				return points [target+=2];
			}

		} else {
			Debug.Log ("This should never happen");
			return Vector3.zero;

		}
*/

	}
	public void init (Color newCol, Vector3 firstPoint)
		{points = new List<Vector3> ();
			myColor = newCol;
			points.Add (firstPoint);
			myLineRenderer = transform.GetComponent<LineRenderer> ();
			//myLineRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			myLineRenderer.SetColors (newCol, newCol);
			myLineRenderer.SetWidth (0.2F, 0.3F);
			myLineRenderer.SetVertexCount (2);
		done = false;

		}

		public void addPoint (Vector3 nextPoint)
		{
		
			if (points.Count <= maximumLineLength) {
				points.Add (nextPoint);
				myLineRenderer.SetVertexCount (points.Count);
				myLineRenderer.SetPositions (points.ToArray ());

			} else {
				Debug.Log ("we run out of points here draw another line");
				stop ();
			}

		}

		public void stop ()
		{
		done = true;
			myLineRenderer.SetVertexCount (points.Count);
			myLineRenderer.SetPositions (points.ToArray ());
		Debug.Log ("We are done here:" + points.Count);
		}

		public void delete ()
		{

		}
	}
