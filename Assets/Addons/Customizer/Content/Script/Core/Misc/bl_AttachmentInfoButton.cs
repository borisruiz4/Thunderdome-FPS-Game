﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bl_AttachmentInfoButton : MonoBehaviour {

    [SerializeField]private Text m_Text;
    [SerializeField] private RawImage CamoImg;
    private CanvasGroup Alpha;
    [SerializeField] private AudioClip InitAudio;

    private int ID;
    private bl_AttachType m_Type;
    
    public void Init(CustomizerModelInfo info,bl_AttachType typ, float d, bool selected)
    {
        Alpha = GetComponent<CanvasGroup>();
        Alpha.alpha = 0;
        m_Text.text = info.Name;
        ID = info.ID;
        m_Type = typ;
        StartCoroutine(Fade(d));
        GetComponent<Button>().interactable = !selected;
    }

    public void InitCamo(CamoInfo info, float d, bool selected)
    {
        Alpha = GetComponent<CanvasGroup>();
        Alpha.alpha = 0;
        CamoImg.texture = info.Preview;
        ID = info.ID;
        StartCoroutine(Fade(d));
        GetComponentInChildren<Button>().interactable = !selected;
    }

    public void OnSelect()
    {
        bl_CustomizerManager c = FindObjectOfType<bl_CustomizerManager>();
        c.OnSelectAttachment(m_Type, ID);
        Button[] bt = transform.parent.GetComponentsInChildren<Button>();
        for (int i = 0; i < bt.Length; i++)
        {
            bt[i].interactable = true;
            bt[i].OnDeselect(null);
        }
        GetComponent<Button>().interactable = false;
    }

    public void OnSelectCamo()
    {
        bl_CustomizerManager c = FindObjectOfType<bl_CustomizerManager>();
        c.OnSelectCamo(ID);
        Button[] bt = transform.parent.GetComponentsInChildren<Button>();
        for (int i = 0; i < bt.Length; i++)
        {
            bt[i].interactable = true;
            bt[i].OnDeselect(null);
        }
        GetComponentInChildren<Button>().interactable = false;
    }

    IEnumerator Fade(float delay)
    {
        yield return new WaitForSeconds(delay);
        float d = 0;
        AudioSource.PlayClipAtPoint(InitAudio, Camera.main.transform.position);
        while (d < 1)
        {
            d += Time.deltaTime * 2;
            Alpha.alpha = d;
            yield return null;
        }
    }
}