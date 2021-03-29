using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldNote : MonoBehaviour
{
    [SerializeField] private RectTransform selfTransform;
    public Transform h0;
    public Transform h1;

    public NotesController gameController;
    private int ID;
    public Vector3 targetPosition;
    private Vector3 direction;

    public int noteTime0;
    private int delayTime0;
    public int noteTime1;
    private int delayTime1;


    public int hitOffset;

    public Text haha;
    public NoteTestSound noteTestSound;

    public InputController inputController;

    void Awake()
    {
        selfTransform = GetComponent<RectTransform>();
        GetComponent<LineRenderer>().material = GetComponent<HoldNoteRenderer>().defaultMaterial;
    }


    void Update()
    {
        UpdatePosition();
        ClearWhenOutOfLane();
    }


    public void Initialized(NotesController gameCont, int Id, Vector3 sPosition, Vector3 tPosition, Vector3 dir,
        int nTime0, int dTime0, int nTime1, int dTime1) //初始化方法
    {
        gameController = gameCont;
        ID = Id;
        selfTransform.position = sPosition;
        targetPosition = tPosition;
        direction = dir;

        noteTime0 = nTime0;
        delayTime0 = dTime0;
        noteTime1 = nTime1;
        delayTime1 = dTime1;
    }


    private void UpdatePosition()
    {
        Vector3 pos0 = targetPosition;
        pos0.x -= (gameController.DelayedSampleTime - noteTime0) / (float) gameController.SampleRate *
                  gameController.noteSpeed;
        h0.position = pos0;

        Vector3 pos1 = targetPosition;
        pos1.x -= (gameController.DelayedSampleTime - noteTime1) / (float) gameController.SampleRate *
                  gameController.noteSpeed;
        h1.position = pos1;
    }

    private void ResetNote() //重置Note对象
    {
        gameController = null;
    }

    private void ReturnToPool() //返回对象池
    {
        gameController.ReturnHoldNoteToPool(this);
        ResetNote();
    }

    public void OnhitHead() //击中音符对象
    {
        if (noteTime0 - gameController.DelayedSampleTime <= gameController.hitMissRangeInSamples)
        {
            if (Mathf.Abs(noteTime0 - gameController.DelayedSampleTime) <= gameController.hitGreatRangeInSamples)
            {
                if (Mathf.Abs(noteTime0 - gameController.DelayedSampleTime) <= gameController.hitPerfectRangeInSamples)
                {
                    haha.text = "Perfect";
                    inputController._ifHold = true;
                    noteTestSound.Yahoooo();
                    ComboRecordAndStatusChange.CCInstance.AddCombo();
                }
                else
                {
                    haha.text = "Great";
                    inputController._ifHold = true;
                    noteTestSound.Yahoooo();
                    ComboRecordAndStatusChange.CCInstance.AddCombo();
                }
            }
            else
            {
                Oooops();
            }
        }
    }

    public bool IfHoverTail()
    {
        if (gameController.DelayedSampleTime >= noteTime1) return true;
        return false;
    }

    private void ClearWhenOutOfLane()
    {
        if (gameController.DelayedSampleTime - noteTime1 >= gameController.hitMissRangeInSamples)
        {
            ReturnToPool();
        }
    }

    public void Oooops()
    {
        h0.GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<LineRenderer>().material = GetComponent<HoldNoteRenderer>().interruptMaterial;
        inputController.targetHoldingHit=new RaycastHit2D();
        haha.text = "Miss";
        inputController._ifHold = false;
        ComboRecordAndStatusChange.CCInstance.AddFail();
    }

    public void Done()
    {
        haha.text = "Niiiiiiiiiice";
        ReturnToPool();
    }
}