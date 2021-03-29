using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum slideVector
{
    nullVector,
    up,
    down,
    left,
    right
};

public class InputController : MonoBehaviour
{
    private Vector2 _rawTouchPos;
    private Vector2 _touchPos;

    public RectTransform laneTargeTransform;

    private RaycastHit2D targetMovingHit;
    public RaycastHit2D targetHoldingHit;
    private bool _ifFlip = false;
    public bool _ifHold = false;

    void Start()
    {
    }


    void Update()
    {
        CheckInput();
    }

    private void CheckInput() // CheckInput
    {
        if (Input.touchCount > 0)
        {
            foreach (var eachTouch in Input.touches)
            {
                if (eachTouch.phase == TouchPhase.Began)
                {
                    _rawTouchPos = eachTouch.position;
                    _touchPos = Camera.main.ScreenToWorldPoint(_rawTouchPos);
                    Vector2 pos = _touchPos;
                    pos.x = laneTargeTransform.position.x;
                    RaycastHit2D hit2DRight = Physics2D.Raycast(pos, new Vector2(1, 0), 1000);
                    RaycastHit2D hit2DLeft = Physics2D.Raycast(pos, new Vector2(-1, 0), 1000);
                    float theNearest = float.MaxValue;
                    RaycastHit2D targetHit = new RaycastHit2D();
                    if (hit2DRight)
                    {
                        float distance = Mathf.Abs(hit2DRight.collider.GetComponent<RectTransform>().position.x -
                                                   laneTargeTransform.position.x);
                        if (distance < theNearest)
                        {
                            theNearest = distance;
                            targetHit = hit2DRight;
                        }
                    }

                    if (hit2DLeft)
                    {
                        float distance = Mathf.Abs(hit2DLeft.collider.GetComponent<RectTransform>().position.x -
                                                   laneTargeTransform.position.x);
                        if (distance < theNearest)
                        {
                            theNearest = distance;
                            targetHit = hit2DLeft;
                        }
                    }

                    if (targetHit) //如果有音符
                    {
                        if (targetHit.collider.gameObject.CompareTag("ClickNote")) //如果是单击音符
                        {
                            targetHit.collider.GetComponent<Note>().Onhit();
                        }

                        if (targetHit.collider.gameObject.CompareTag("FlipNote")) //如果是Flip音符
                        {
                            targetMovingHit = targetHit;
                            _ifFlip = true;
                        }

                        if (targetHit.collider.gameObject.CompareTag("HoldNote"))
                        {
                            targetHoldingHit = targetHit;
                            targetHit.collider.GetComponentInParent<HoldNote>().OnhitHead();
                        }
                    }

                    touchFirst = _rawTouchPos; //记录开始按下的位置
                    ifEnd = false;
                }

                if (_ifFlip)
                {
                    CheckFlip(eachTouch, targetMovingHit);
                }

                if (_ifHold)
                {
                    CheckHold(eachTouch, targetHoldingHit);
                }
            }
        }
    }


    private Vector2 touchFirst = Vector2.zero; //手指开始按下的位置
    private Vector2 touchSecond = Vector2.zero; //手指拖动的位置
    private slideVector currentVector = slideVector.nullVector; //当前滑动方向
    private float timer; //时间计数器  
    private bool ifEnd; //为单次滑动检测服务的flag
    public float offsetTime; //判断的时间间隔 
    public float SlidingDistance;

    private void CheckFlip(Touch eachTouch, RaycastHit2D targetHit)
    {
        if (eachTouch.phase == TouchPhase.Moved)
            //判断当前手指是拖动事件
        {
            touchSecond = eachTouch.position;

            timer += Time.deltaTime; //计时器

            if (timer > offsetTime)
            {
                touchSecond = eachTouch.position; //记录结束下的位置
                Vector2 slideDirection = touchFirst - touchSecond;
                float x = slideDirection.x;
                float y = slideDirection.y;

                // if (y + SlidingDistance < x && y > -x - SlidingDistance && !ifEnd)
                // {
                //     Debug.Log("left");
                //
                //     currentVector = slideVector.left;
                //     ifEnd = true;
                // }
                // else if (y > x + SlidingDistance && y < -x - SlidingDistance && !ifEnd)
                // {
                //     Debug.Log("right");
                //
                //     currentVector = slideVector.right;
                //     ifEnd = true;
                // }
                if (y > x + SlidingDistance && y - SlidingDistance > -x && !ifEnd)
                {
                    // Debug.Log("Down");

                    currentVector = slideVector.down;
                    ifEnd = true;
                }
                else if (y + SlidingDistance < x && y < -x - SlidingDistance && !ifEnd)
                {
                    // Debug.Log("Up");

                    currentVector = slideVector.up;
                    ifEnd = true;
                }


                timer = 0;
                touchFirst = touchSecond;
            }
        } // 滑动方法

        if (ifEnd || (eachTouch.phase == TouchPhase.Ended && ifEnd))
        {
            if (targetHit)
            {
                if (targetHit.collider.GetComponent<FlipNote>().aimDirection == currentVector)
                {
                    targetHit.collider.GetComponent<FlipNote>().Onhit();
                }
                else
                {
                    targetHit.collider.GetComponent<FlipNote>().WrongDirInput();
                }

                targetMovingHit = new RaycastHit2D();
                currentVector = slideVector.nullVector;
                ifEnd = false;
            }
        }
    }

    private bool _isHolding;
    private bool _ifHoverTail;

    private void CheckHold(Touch eachTouch, RaycastHit2D targetHit)
    {
        if (eachTouch.phase == TouchPhase.Stationary || eachTouch.phase == TouchPhase.Moved)
        {
            // Debug.Log("isholding!!!");
            _isHolding = true;
            _ifHoverTail = targetHit.collider.GetComponentInParent<HoldNote>().IfHoverTail();
            if (_isHolding)
            {
                targetHit.collider.GetComponent<Transform>().position =
                    targetHit.collider.GetComponentInParent<HoldNote>().targetPosition;
            }
            if (_ifHoverTail)
            {
                // Debug.Log("Done!!!!!!");
                targetHit.collider.GetComponentInParent<HoldNote>().Done();
                _isHolding = false;
                _ifHold = false;
                
            }
        }

        if (_isHolding && eachTouch.phase == TouchPhase.Ended)
        {
            // Debug.Log("FingerUpppppppp");
            _isHolding = false;
            _ifHoverTail = targetHit.collider.GetComponentInParent<HoldNote>().IfHoverTail();
            if (!_ifHoverTail)
            {
                targetHit.collider.GetComponentInParent<HoldNote>().Oooops();
            }

            _ifHold = false;
        }
    }
}