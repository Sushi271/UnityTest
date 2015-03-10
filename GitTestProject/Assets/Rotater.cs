using UnityEngine;
using System.Collections;

public class Rotater : MonoBehaviour {

    public float startY;
	// Use this for initialization
	void Start () {
        startY = this.transform.localPosition.y;
        StartCoroutine(rotateStart());
	}

    IEnumerator rotateStart () {
        while(true){
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, startY + Mathf.Sin(Time.fixedTime) * 0.2F, this.transform.localPosition.z);
            print("yeld");
            this.transform.Rotate(new Vector3(Random.Range(-2, 5), Random.Range(-2, 5), Random.Range(-5,3)));
            yield return new WaitForSeconds(0.03f);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
