using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboRecordAndStatusChange : MonoBehaviour
{
    private static ComboRecordAndStatusChange _instance;

    public static ComboRecordAndStatusChange CCInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(ComboRecordAndStatusChange)) as ComboRecordAndStatusChange;
            }

            if (_instance == null)
            {
                GameObject newGO = new GameObject("ComboAndStatus");
                _instance = newGO.AddComponent<ComboRecordAndStatusChange>();
            }

            return _instance;
        }
    }

    public int combo;
    public int fail;
    public int _status;
    public Animator charactorAnimator;

    void Start()
    {
        _status = 0;
        combo = 0;
        fail = 0;
        charactorAnimator.SetInteger("Status", _status);
    }


    void Update()
    {
    }

    public void AddCombo()
    {
        combo++;
        fail = 0;
        CheckIfChangeStatus();
    }

    public void AddFail()
    {
        fail++;
        combo = 0;
        CheckIfChangeStatus();
    }

    private void CheckIfChangeStatus()
    {
        // Debug.Log("Combo:" + combo + "//Fail:" + fail);
        if (_status == -1)
        {
            if (combo == 3)
            {
                _status++;
                combo = 0;
                charactorAnimator.SetInteger("Status", _status);
            }
        }

        if (_status == 0)
        {
            if (combo == 5)
            {
                _status++;
                combo = 0;
                charactorAnimator.SetInteger("Status", _status);
            }

            if (fail == 2)
            {
                _status--;
                fail = 0;
                charactorAnimator.SetInteger("Status", _status);
            }
        }

        if (_status == 1)
        {
            if (fail == 2)
            {
                _status--;
                fail = 0;
                charactorAnimator.SetInteger("Status", _status);
            }
        }
    }
}