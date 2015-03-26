using UnityEngine;
using System.Collections;

public class SphereControl : MonoBehaviour {

	//物理法則制御用
	private Rigidbody rgbody;

	void Start () {

		//物理法則制御の準備
		rgbody = this.GetComponent<Rigidbody>();

	}
	
	void Update () {

		if(Input.GetMouseButtonDown(0)){
			rgbody.AddForce(
				Vector3.up * 1,
				ForceMode.VelocityChange
				);
		}
	}
}
