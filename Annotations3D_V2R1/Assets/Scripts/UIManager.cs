using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
    public DepthRenderer m_DepthViewRenderer;

    public Button m_BtnSettings;
    public Text m_BtnSettingsText;
    private bool m_SettingsActive = false;

    public Image m_BackgroundBlur;
    public GameObject m_AllSettingsUIElements;
    public InputField m_LocationX, m_LocationY;
    public InputField m_ViewWidth, m_ViewHeight;

    public Toggle m_ToggleColor, m_ToggleDepth, m_ToggleFiltering, m_ToggleAveraging;
    public InputField m_AveragingFrames;
    public Slider m_DepthScale;

    private string KEY_LOCATIONX = "LOCATION_X", KEY_LOCATIONY = "LOCATION_Y",
                   KEY_VIEWWIDTH = "VIEW_WIDTH", KEY_VIEWHEIGHT = "VIEW_HEIGHT",
                   KEY_COLOR = "COLOR", KEY_DEPTH = "DEPTH", KEY_FILTERING = "FILTERING", KEY_AVERGING = "AVERAGING",
                   KEY_AVGFRAMES = "FRAMES", KEY_DEPTHSCALES = "DEPTH_SCALE";
        
    void Start () {
        m_BtnSettings.onClick.AddListener(() => { ToggleSettingsVisibility(); });
        m_BackgroundBlur.gameObject.SetActive(false);
        m_AllSettingsUIElements.SetActive(false);
        InitialiseDefaultSettings();
        ApplySettings();
    }
	
	void InitialiseDefaultSettings()
    {
        if (PlayerPrefs.HasKey(KEY_LOCATIONX))
        {
            m_LocationX.text = "" + PlayerPrefs.GetInt(KEY_LOCATIONX);
        } else
        {
            m_LocationX.text = "150";
            PlayerPrefs.SetInt(KEY_LOCATIONX, 150);
        }

        if (PlayerPrefs.HasKey(KEY_LOCATIONY))
        {
            m_LocationY.text = "" + PlayerPrefs.GetInt(KEY_LOCATIONY);
        }
        else
        {
            m_LocationY.text = "180";
            PlayerPrefs.SetInt(KEY_LOCATIONY, 180);
        }

        if (PlayerPrefs.HasKey(KEY_VIEWWIDTH))
        {
            m_ViewWidth.text = "" + PlayerPrefs.GetInt(KEY_VIEWWIDTH);
        }
        else
        {
            m_ViewWidth.text = "250";
            PlayerPrefs.SetInt(KEY_VIEWWIDTH, 250);
        }

        if (PlayerPrefs.HasKey(KEY_VIEWHEIGHT))
        {
            m_ViewHeight.text = "" + PlayerPrefs.GetInt(KEY_VIEWHEIGHT);
        }
        else
        {
            m_ViewHeight.text = "200";
            PlayerPrefs.SetInt(KEY_VIEWHEIGHT, 200);
        }

        if (PlayerPrefs.HasKey(KEY_COLOR))
        {
            if (PlayerPrefs.GetInt(KEY_COLOR) == 1)
            {
                m_ToggleColor.isOn = true;
            } else
            {
                m_ToggleColor.isOn = false;
            }
        }
        else
        {
            m_ToggleColor.isOn = true;
            PlayerPrefs.SetInt(KEY_COLOR, 1);
        }

        if (PlayerPrefs.HasKey(KEY_DEPTH))
        {
            if (PlayerPrefs.GetInt(KEY_DEPTH) == 1)
            {
                m_ToggleDepth.isOn = true;
            }
            else
            {
                m_ToggleDepth.isOn = false;
            }
        }
        else
        {
            m_ToggleDepth.isOn = true;
            PlayerPrefs.SetInt(KEY_DEPTH, 1);
        }

        if (PlayerPrefs.HasKey(KEY_FILTERING))
        {
            if (PlayerPrefs.GetInt(KEY_FILTERING) == 1)
            {
                m_ToggleFiltering.isOn = true;
            }
            else
            {
                m_ToggleFiltering.isOn = false;
            }
        }
        else
        {
            m_ToggleFiltering.isOn = true;
            PlayerPrefs.SetInt(KEY_FILTERING, 1);
        }

        if (PlayerPrefs.HasKey(KEY_AVERGING))
        {
            if (PlayerPrefs.GetInt(KEY_AVERGING) == 1)
            {
                m_ToggleAveraging.isOn = true;
            }
            else
            {
                m_ToggleAveraging.isOn = false;
            }
        }
        else
        {
            m_ToggleAveraging.isOn = true;
            PlayerPrefs.SetInt(KEY_AVERGING, 1);
        }

        if (PlayerPrefs.HasKey(KEY_AVGFRAMES))
        {
            m_AveragingFrames.text = "" + PlayerPrefs.GetInt(KEY_AVGFRAMES);
        }
        else
        {
            m_AveragingFrames.text = "30";
            PlayerPrefs.SetInt(KEY_AVGFRAMES, 30);
        }

        if (PlayerPrefs.HasKey(KEY_DEPTHSCALES))
        {
            m_DepthScale.value = PlayerPrefs.GetFloat(KEY_DEPTHSCALES);
        } else
        {
            m_DepthScale.value = 0.2f;
            PlayerPrefs.SetFloat(KEY_DEPTHSCALES, 0.2f);
        }

    }

    public void SaveSettings()
    {
        int setVal = 1;
        PlayerPrefs.SetInt(KEY_LOCATIONX, Convert.ToInt32(m_LocationX.text));
        PlayerPrefs.SetInt(KEY_LOCATIONY, Convert.ToInt32(m_LocationY.text));
        PlayerPrefs.SetInt(KEY_VIEWWIDTH, Convert.ToInt32(m_ViewWidth.text));
        PlayerPrefs.SetInt(KEY_VIEWHEIGHT, Convert.ToInt32(m_ViewHeight.text));
        setVal = (m_ToggleColor.isOn) ? 1 : 0;
        PlayerPrefs.SetInt(KEY_COLOR, setVal);
        setVal = (m_ToggleDepth.isOn) ? 1 : 0;
        PlayerPrefs.SetInt(KEY_DEPTH, setVal);
        setVal = (m_ToggleFiltering.isOn) ? 1 : 0;
        PlayerPrefs.SetInt(KEY_FILTERING, setVal);
        setVal = (m_ToggleAveraging.isOn) ? 1 : 0;
        PlayerPrefs.SetInt(KEY_AVERGING, setVal);
        PlayerPrefs.SetInt(KEY_AVGFRAMES, Convert.ToInt32(m_AveragingFrames.text));
        PlayerPrefs.SetFloat(KEY_DEPTHSCALES, m_DepthScale.value);
    }

    public void ApplySettings()
    {
        m_DepthViewRenderer.m_enableColor = m_ToggleColor.isOn;
        m_DepthViewRenderer.m_enableDepth = m_ToggleDepth.isOn;
        m_DepthViewRenderer.m_enableFiltering = m_ToggleFiltering.isOn;
        m_DepthViewRenderer.m_enableMovingAverage = m_ToggleAveraging.isOn;

        m_DepthViewRenderer.m_DepthScaleFactor = m_DepthScale.value;
        m_DepthViewRenderer.m_ViewStartX = Convert.ToInt32(m_LocationX.text);
        m_DepthViewRenderer.m_ViewStartY = Convert.ToInt32(m_LocationY.text);
        m_DepthViewRenderer.m_ViewWidth = Convert.ToInt32(m_ViewWidth.text);
        m_DepthViewRenderer.m_ViewHeight = Convert.ToInt32(m_ViewHeight.text);
        m_DepthViewRenderer.m_NumAvgFrames = Convert.ToInt32(m_AveragingFrames.text);
        m_DepthViewRenderer.InitialiseMemoryAndMesh();
    }

	void Update () {
	
	}

    public void ToggleSettingsVisibility()
    {
        m_SettingsActive = !m_SettingsActive;
        
        if (m_SettingsActive)
        {
            m_BtnSettingsText.text = "Save and Exit";
            m_BackgroundBlur.gameObject.SetActive(true);
            m_AllSettingsUIElements.SetActive(true);
        } else
        {
            SaveSettings();
            ApplySettings();

            m_BtnSettingsText.text = "Settings";
            m_BackgroundBlur.gameObject.SetActive(false);
            m_AllSettingsUIElements.SetActive(false);
        }

    }
}
