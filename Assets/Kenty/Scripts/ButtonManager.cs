using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation.Samples;
using TMPro;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]
    private GameObject blendShapeHints;
    [SerializeField]
    private Button hintbutton;
    private void Start()
    {
        bool isActive = false;
        hintbutton.onClick.AddListener(()=>
        {
            isActive = !isActive;
            blendShapeHints.SetActive(isActive);
        });
    }
    public void ToDescription()
    {
        // 読み込むシーンの名前を直接指定
        SceneManager.LoadScene("Description");
    }

    public void ToGameScene()
    {
        SceneManager.LoadScene("FaceBlendShapesGame");
    }

    public void ToTitle()
    {
        SceneManager.LoadScene("Title");
    }
    public void ShowHints()
    {
    }

}
