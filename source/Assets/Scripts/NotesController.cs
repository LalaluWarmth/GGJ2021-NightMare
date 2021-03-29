using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using SonicBloom.Koreo;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotesController : MonoBehaviour
{
    [Tooltip("事件对应ID")] [EventID] public string[] eventID;

    [Tooltip("速度")] public float noteSpeed = 1;

    [Tooltip("Miss")] [Range(0f, 1000f)] public float hitMissRangeInMS;
    
    public float hitMissRangeInSamples
    {
        get { return SampleRate * (hitMissRangeInMS * 0.001f); }
    }

    [Tooltip("Great")] [Range(0f, 1000f)] public float hitGreatRangeInMS;

    public float hitGreatRangeInSamples
    {
        get { return SampleRate * (hitGreatRangeInMS * 0.001f); }
    }

    [Tooltip("Perfect")] [Range(0f, 1000f)]
    public float hitPerfectRangeInMS;

    public float hitPerfectRangeInSamples
    {
        get { return SampleRate * (hitPerfectRangeInMS * 0.001f); }
    }


    public int SampleRate //采样率
    {
        get { return playingKoreo.SampleRate; }
    }

    //-------------------------引用--------------------------
    public Koreography playingKoreo;
    public AudioSource audioCom;


    //-------------------------对象池--------------------------

    Stack<Note> notePool = new Stack<Note>();
    Stack<FlipNote> flipNotePool = new Stack<FlipNote>();
    Stack<HoldNote> holdNotePool = new Stack<HoldNote>();

    public Note sampleNote;

    public FlipNote sampleFlipNote;

    public HoldNote sampleHoldNote;
    // public Stack<GameObject> hitEffectObjectPool = new Stack<GameObject>(); //特效

    //-------------------------音符--------------------------
    public Note noteObject;

    public FlipNote flipNoteObject;

    public HoldNote holdNoteObject;
    // public GameObject hitEffectGo;

    //-------------------------引导时间--------------------------
    [Tooltip("开始播放音频之前提供的时间量")] public float leadInTime;
    [SerializeField] private float timeLeftToPlay; //音乐开始之前的倒计时器

    public int DelayedSampleTime
    {
        get
        {
            return playingKoreo.GetLatestSampleTime() -
                   (int) (SampleRate * timeLeftToPlay); //乐谱上的采样点时刻-提前量对应采样点个数=>延迟调用
        }
    }

    public RectTransform[] laneTargets;
    public RectTransform[] laneOrigins;


    private Queue<clickNoteInitData> initClickNotesDatas = new Queue<clickNoteInitData>();
    private Queue<filpNoteInitData> initFlipNotesDatas = new Queue<filpNoteInitData>();
    private Queue<holdNoteInitData> initHoldNotesDatas = new Queue<holdNoteInitData>();
    public Transform storyboard;


    void Start()
    {
        CheckDeviceType();
        InitializeLeadIn();
        playingKoreo = Koreographer.Instance.GetKoreographyAtIndex(0); //获取Koreography对象
        ReturnNoteToPool(sampleNote);
        InitClickNoteQueue();
        ReturnFlipNoteToPool(sampleFlipNote);
        InitFlipNoteQueue();
        ReturnHoldNoteToPool(sampleHoldNote);
        InitHoldNoteQueue();
    }


    void Update()
    {
        LeadinTimeCountDown();
        CheckSpawnNext(); //检测新Click音符的产生
        CheckSpawnNextFlipNote(); //检测新Flip音符的产生
        // Debug.Log(DelayedSampleTime);
        CheckSpawnNextHoldNote();
    }

    private void CheckDeviceType() // CheckDeviceType
    {
#if UNITY_EDITOR
        Debug.Log("Unity Editor");
#endif

#if UNITY_IOS
        Debug.Log("Iphone");
#endif

#if UNITY_STANDALONE_OSX
        Debug.Log("Stand Alone OSX");
#endif

#if UNITY_STANDALONE_WIN
        Debug.Log("Stand Alone Windows");
#endif
    }


    private void InitializeLeadIn() //初始化引导时间并控制播放
    {
        if (leadInTime > 0)
        {
            timeLeftToPlay = leadInTime;
        }
        else
        {
            audioCom.Play();
        }
    }

    private void LeadinTimeCountDown()
    {
        if (timeLeftToPlay > 0)
        {
            timeLeftToPlay = Mathf.Max(timeLeftToPlay - Time.unscaledDeltaTime, 0);
            if (timeLeftToPlay <= 0)
            {
                audioCom.Play();
                StartCoroutine(AudioPlayFinished(audioCom.clip.length));
            }
        }
    }

    private IEnumerator AudioPlayFinished(float time)
    {
        yield return new WaitForSeconds(time);
        AudioOver();
    }

    public SceneMoveController SceneMoveController;
    private void AudioOver()
    {
        SceneMoveController.audioOver = true;
    }
    
    private struct clickNoteInitData
    {
        public int ID;
        public Vector3 sPosition;
        public Vector3 tPosition;
        public Vector3 direction;
        public int nTime;
        public int dTime;
    }

    private void InitClickNoteQueue()
    {
        KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(eventID[0]); //获取时间轨迹
        List<KoreographyEvent> rawClickEvents = rhythmTrack.GetAllEvents(); //获取所有事件
        int clickID = 0;
        foreach (var item in rawClickEvents)
        {
            string rawText = item.GetTextValue();
            List<int> laneData = new TextTok().TokText(rawText);
            foreach (var rawData in laneData)
            {
                clickNoteInitData temp = new clickNoteInitData();
                temp.ID = clickID;
                clickID++;
                temp.sPosition = laneOrigins[rawData].position;
                temp.tPosition = laneTargets[rawData].position;
                temp.direction = new Vector3(-1, 0, 0);
                temp.nTime = item.StartSample;
                temp.dTime = item.StartSample - GetSpawnSampleOffset(temp.sPosition, laneTargets[rawData].position);
                initClickNotesDatas.Enqueue(temp);
            }
        }
    }

    private struct filpNoteInitData
    {
        public int ID;
        public Vector3 sPosition;
        public Vector3 tPosition;
        public Vector3 direction;
        public int nTime;
        public int dTime;
        public slideVector aimDir;
    }

    private void InitFlipNoteQueue()
    {
        KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(eventID[1]); //获取时间轨迹
        List<KoreographyEvent> rawClickEvents = rhythmTrack.GetAllEvents(); //获取所有事件
        int clickID = 0;
        foreach (var item in rawClickEvents)
        {
            string rawText = item.GetTextValue();
            List<int> laneData = new TextTok().TokText(rawText);
            int index = 0;
            while (index < laneData.Count - 1)
            {
                filpNoteInitData temp = new filpNoteInitData();
                temp.ID = clickID;
                clickID++;
                temp.sPosition = laneOrigins[laneData[index]].position;
                temp.tPosition = laneTargets[laneData[index]].position;
                temp.direction = new Vector3(-1, 0, 0);
                temp.nTime = item.StartSample;
                temp.dTime = item.StartSample -
                             GetSpawnSampleOffset(temp.sPosition, laneTargets[laneData[index]].position);
                switch (laneData[index + 1])
                {
                    case 1:
                        temp.aimDir = slideVector.up;
                        break;
                    case 2:
                        temp.aimDir = slideVector.down;
                        break;
                }

                index += 2;
                initFlipNotesDatas.Enqueue(temp);
            }
        }
    }

    private struct holdNoteInitData
    {
        public int ID;
        public Vector3 sPosition;
        public Vector3 tPosition;
        public Vector3 direction;
        public int nTime0;
        public int dTime0;
        public int nTime1;
        public int dTime1;
    }

    private void InitHoldNoteQueue()
    {
        KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(eventID[2]); //获取时间轨迹
        List<KoreographyEvent> rawHoldEvents = rhythmTrack.GetAllEvents(); //获取所有事件
        int clickID = 0;
        int index = 0;
        while (index < rawHoldEvents.Count - 1)
        {
            string rawText = rawHoldEvents[index].GetTextValue();
            List<int> laneData = new TextTok().TokText(rawText);
            holdNoteInitData temp = new holdNoteInitData();
            temp.ID = clickID;
            clickID++;
            temp.sPosition = laneOrigins[laneData[0]].position;
            temp.tPosition = laneTargets[laneData[0]].position;
            temp.direction = new Vector3(-1, 0, 0);
            temp.nTime0 = rawHoldEvents[index].StartSample;
            temp.dTime0 = rawHoldEvents[index].StartSample -
                          GetSpawnSampleOffset(temp.sPosition, laneTargets[laneData[0]].position);
            temp.nTime1 = rawHoldEvents[index + 1].StartSample;
            temp.dTime1 = rawHoldEvents[index + 1].StartSample -
                          GetSpawnSampleOffset(temp.sPosition, laneTargets[laneData[0]].position);
            initHoldNotesDatas.Enqueue(temp);
            index += 2;
        }
    }


    private int GetSpawnSampleOffset(Vector3 curClickNoteStartPosition,
        Vector3 curClickNoteTargetPosition) //确定在乐谱上产生音符的对应采样点的偏移量
    {
        float spawnDistToTarget =
            Vector3.Distance(curClickNoteStartPosition, curClickNoteTargetPosition); //计算生成为位置和结束位置的距离

        float spawnPosToTargetTime = spawnDistToTarget / noteSpeed; //计算出音符到达结束位置所需的时间
        return (int) (spawnPosToTargetTime * SampleRate); //时间*采样率=采样点偏移量
    }

    //-------------------------Click音符对象池方法--------------------------
    private void CheckSpawnNext() //不断检测是否生成下一个新音符
    {
        int currentTime = DelayedSampleTime;
        int curNum = 1;
        while (curNum <= initClickNotesDatas.Count && initClickNotesDatas.Peek().dTime <= currentTime)
        {
            clickNoteInitData tempData = initClickNotesDatas.Dequeue();
            // Debug.Log("TimeToGo!");
            Note newObj = GetFreshNote();
            newObj.Initialized(this, tempData.ID, tempData.sPosition, tempData.tPosition, tempData.direction,
                tempData.nTime, tempData.dTime);
            curNum++;
        }
    }

    public Note GetFreshNote() //从对象池中取对象的方法
    {
        Note retObj;
        if (notePool.Count > 0)
        {
            retObj = notePool.Pop();
        }
        else
        {
            retObj = Instantiate<Note>(noteObject, storyboard);
        }

        retObj.transform.position = Vector3.one * 200;
        retObj.gameObject.SetActive(true);
        retObj.enabled = true;
        return retObj;
    }

    public void ReturnNoteToPool(Note obj) //音符对象回到对象池
    {
        if (obj != null)
        {
            notePool.Push(obj);
            obj.enabled = false;
            obj.gameObject.SetActive(false);
        }
    }

    //-------------------------Flip音符对象池方法--------------------------
    private void CheckSpawnNextFlipNote() //不断检测是否生成下一个新音符
    {
        int currentTime = DelayedSampleTime;
        int curNum = 1;
        while (curNum <= initFlipNotesDatas.Count && initFlipNotesDatas.Peek().dTime <= currentTime)
        {
            filpNoteInitData tempData = initFlipNotesDatas.Dequeue();
            // Debug.Log("TimeToGo!");
            FlipNote newObj = GetFreshFlipNote();
            newObj.Initialized(this, tempData.ID, tempData.sPosition, tempData.tPosition, tempData.direction,
                tempData.nTime, tempData.dTime, tempData.aimDir);
            curNum++;
        }
    }

    public FlipNote GetFreshFlipNote() //从对象池中取对象的方法
    {
        FlipNote retObj;
        if (flipNotePool.Count > 0)
        {
            retObj = flipNotePool.Pop();
        }
        else
        {
            retObj = Instantiate<FlipNote>(flipNoteObject, storyboard);
        }

        retObj.transform.position = Vector3.one * 200;
        retObj.gameObject.SetActive(true);
        retObj.enabled = true;
        return retObj;
    }

    public void ReturnFlipNoteToPool(FlipNote obj) //音符对象回到对象池
    {
        if (obj != null)
        {
            flipNotePool.Push(obj);
            obj.enabled = false;
            obj.gameObject.SetActive(false);
        }
    }

    //-------------------------Hold音符对象池方法--------------------------
    private void CheckSpawnNextHoldNote() //不断检测是否生成下一个新音符
    {
        int currentTime = DelayedSampleTime;
        int curNum = 1;
        while (curNum <= initHoldNotesDatas.Count && initHoldNotesDatas.Peek().dTime0 <= currentTime)
        {
            holdNoteInitData tempData = initHoldNotesDatas.Dequeue();
            // Debug.Log("TimeToGo!");
            HoldNote newObj = GetFreshHoldNote();
            newObj.Initialized(this, tempData.ID, tempData.sPosition, tempData.tPosition, tempData.direction,
                tempData.nTime0, tempData.dTime0, tempData.nTime1, tempData.dTime1);
            curNum++;
        }
    }

    public HoldNote GetFreshHoldNote() //从对象池中取对象的方法
    {
        HoldNote retObj;
        if (holdNotePool.Count > 0)
        {
            retObj = holdNotePool.Pop();
        }
        else
        {
            retObj = Instantiate<HoldNote>(holdNoteObject, storyboard);
        }

        retObj.transform.position = Vector3.one * 200;
        retObj.gameObject.SetActive(true);
        retObj.enabled = true;
        return retObj;
    }

    public void ReturnHoldNoteToPool(HoldNote obj) //音符对象回到对象池
    {
        if (obj != null)
        {
            holdNotePool.Push(obj);
            obj.enabled = false;
            obj.gameObject.SetActive(false);
        }
    }
}