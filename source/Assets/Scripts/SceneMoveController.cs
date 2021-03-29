using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SceneMoveController : MonoBehaviour
{
    public RectTransform BGs;
    public Transform edge;
    public Transform leftedge;
    public Transform endPoint;
    public Transform checkPoint;

    public Transform[] switchPoint;
    public SpriteRenderer man;

    public float walkSpeed;
    public float runSpeed;

    public bool audioOver=false;
    void Start()
    {
    }

    
    void Update()
    {
        if (endPoint.position.x <= edge.position.x)
        {
            //LoadWinScene
            Application.Quit();
        }

        if (audioOver)
        {
            if (checkPoint.position.x <= edge.position.x)
            {
                //loadfailscene
                Application.Quit();
            }
            else
            {
                //loadwinscene
                Application.Quit();
            }
        }

        if (ComboRecordAndStatusChange.CCInstance._status == -1)
        {
            Vector3 pos = BGs.position;
            pos.x -= 0 * Time.deltaTime;
            BGs.position = pos;
        }

        if (ComboRecordAndStatusChange.CCInstance._status == 0)
        {
            Vector3 pos = BGs.position;
            pos.x -= walkSpeed * Time.deltaTime;
            BGs.position = pos;
        }
        if (ComboRecordAndStatusChange.CCInstance._status == 1)
        {
            Vector3 pos = BGs.position;
            pos.x -= runSpeed * Time.deltaTime;
            BGs.position = pos;
        }
        
        Change();
    }

    public Color[] colors;

    private void Change()
    {
        int indexMax = 0;
        int curIndex = 0;
        bool iiii = false;
        foreach (var item in switchPoint)
        {
            if (item.position.x <= leftedge.position.x)
            {
                indexMax = curIndex;
                iiii = true;
            }

            curIndex++;
        }

        if (iiii)
        {
            man.DOColor(colors[indexMax], 0.5f);
        }
        

    }
    
}
