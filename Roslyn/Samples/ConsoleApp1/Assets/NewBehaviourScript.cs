using MobaGo.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

    kScriptSpellProc m_kScriptSpellProc;
    // Use this for initialization
    void Start () {
        m_kScriptSpellProc = kScriptSpellProc.SSP_7;
        Debug.Log(m_kScriptSpellProc);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
