using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlipNote : MonoBehaviour
{
    [SerializeField] private RectTransform selfTransform;

    public NotesController gameController;
    private int ID;
    public Vector3 targetPosition;
    private Vector3 direction;
    public int noteTime;
    private int delayTime;
    public slideVector aimDirection;

    public Sprite[] filpNoteRenderSprites;


    public int hitOffset;

    public Text haha;
    public NoteTestSound noteTestSound;

    void Awake()
    {
        selfTransform = GetComponent<RectTransform>();
    }


    void Update()
    {
        UpdatePosition();
        ClearWhenOutOfLane();
    }


    public void Initialized(NotesController gameCont, int Id, Vector3 sPosition, Vector3 tPosition, Vector3 dir,
        int nTime, int dTime, slideVector aimDir) //初始化方法
    {
        gameController = gameCont;
        ID = Id;
        selfTransform.position = sPosition;
        targetPosition = tPosition;
        direction = dir;
        noteTime = nTime;
        delayTime = dTime;
        aimDirection = aimDir;
        if (aimDirection == slideVector.up)
        {
            GetComponent<Image>().sprite = filpNoteRenderSprites[0];
        }
        else if (aimDirection == slideVector.down)
        {
            GetComponent<Image>().sprite = filpNoteRenderSprites[1];
        }
    }


    private void UpdatePosition()
    {
        Vector3 pos = targetPosition;
        pos.x -= (gameController.DelayedSampleTime - noteTime) / (float) gameController.SampleRate *
                 gameController.noteSpeed;
        selfTransform.position = pos;
    }

    private void ResetNote() //重置Note对象
    {
        gameController = null;
    }

    private void ReturnToPool() //返回对象池
    {
        gameController.ReturnFlipNoteToPool(this);
        ResetNote();
    }

    public void Onhit() //击中音符对象
    {
        if (noteTime - gameController.DelayedSampleTime <= gameController.hitMissRangeInSamples)
        {
            if (Mathf.Abs(noteTime - gameController.DelayedSampleTime) <= gameController.hitGreatRangeInSamples)
            {
                if (Mathf.Abs(noteTime - gameController.DelayedSampleTime) <= gameController.hitPerfectRangeInSamples)
                {
                    haha.text = "Perfect";
                    noteTestSound.Yahoooo();
                    ReturnToPool();
                    ComboRecordAndStatusChange.CCInstance.AddCombo();
                }
                else
                {
                    haha.text = "Great";
                    noteTestSound.Yahoooo();
                    ReturnToPool();
                    ComboRecordAndStatusChange.CCInstance.AddCombo();
                }
            }
            else
            {
                haha.text = "Miss";
                ReturnToPool();
                ComboRecordAndStatusChange.CCInstance.AddFail();
            }
        }
    }

    private void ClearWhenOutOfLane()
    {
        if (gameController.DelayedSampleTime - noteTime >= gameController.hitMissRangeInSamples)
        {
            haha.text = "Miss";
            ReturnToPool();
            ComboRecordAndStatusChange.CCInstance.AddFail();
        }
    }

    public void WrongDirInput()
    {
        haha.text = "Miss";
        ReturnToPool();
        ComboRecordAndStatusChange.CCInstance.AddFail();
    }
}