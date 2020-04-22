﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeMouseSensitivity : MonoBehaviour
{

	public GameObject cam;
	public SmarterCamControl scc;
	public Slider s;
    // Start is called before the first frame update
    void Start()
    {
        scc = cam.GetComponent<SmarterCamControl>();
        if (scc == null) Debug.Log("SmarterCamControl could not be found");
        if (PlayerPrefs.HasKey("MouseSensitivity"))
        {
            s.value = PlayerPrefs.GetFloat("MouseSensitivity");
            changeMouseSensitivity(s.value);
        }
    }

    public void changeMouseSensitivity(float f)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", s.value);
    	scc.changeMouseSensitivity(PlayerPrefs.GetFloat("MouseSensitivity"));
    }

    public void DebugStuff()
    {
        Debug.Log(s.value);
    }
}
