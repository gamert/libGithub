using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height * 3 / 8, Screen.width / 7, Screen.height / 8), "测试"))
        {
            MonoTool.ReplaceMissScript();
        }
    }
}
