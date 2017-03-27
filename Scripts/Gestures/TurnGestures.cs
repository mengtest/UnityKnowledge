﻿using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
public class TurnGestures : MonoBehaviour {
    public float angle = 0;
    [Header("放开之后是否继续转动")]
    public bool isGo=false;
    private float disAngle;//转动角度距离
    Vector2 targetPos;
    Vector2 currentDragPos;
    Vector2 previousDragPos;
    /// <summary>
    /// 0顺逆,1为顺时针 -1为逆时针
    /// </summary>
    [Header("旋转方向（0顺逆,1顺时针，-1逆时针）")]
    public int direction = 1;
    /// <summary>
    /// 0为沿着x轴旋转 1为沿着y轴旋转 2为沿着z轴旋转
    /// </summary>
    [Header("旋转轴（0x轴，1y轴，2z轴）")]
    public int shaft = 2;
    public Action onBeginDrag;
    public Action onEndDrag;
    public Action onDown;
    /// <summary>
    /// 定义角度委托变量
    /// </summary>
    public Action<float> onTurnAngle;
    public Tween t;
    void Awake()
    {
        //场景中需要一个Tag为MainCamera摄像机
        targetPos = Camera.main.WorldToScreenPoint(this.transform.position);
        EventTriggerListener.Get(gameObject).onBeginDrag = OnBeginDrag;
        EventTriggerListener.Get(gameObject).onDrag = OnDrag;
        EventTriggerListener.Get(gameObject).onEndDrag = OnEndDrag;
        EventTriggerListener.Get(gameObject).onDown = OnDown;
    }
    /// <summary>
    /// 按下
    /// </summary>
    /// <param name="evenData"></param>
    /// <param name="obj"></param>
    private void OnDown(UnityEngine.EventSystems.PointerEventData evenData, GameObject obj)
    {
        if (t != null) t.Kill();
        currentDragPos = previousDragPos = evenData.position;
        if (onDown != null) onDown();
    }
    /// <summary>
    /// 开始拖拽
    /// </summary>
    /// <param name="evenData"></param>
    /// <param name="obj"></param>
    private void OnBeginDrag(UnityEngine.EventSystems.PointerEventData evenData, GameObject obj)
    {
        if (onBeginDrag != null) onBeginDrag();
    }
    /// <summary>
    /// 拖拽中
    /// </summary>
    /// <param name="evenData"></param>
    /// <param name="obj"></param>
    public void OnDrag(UnityEngine.EventSystems.PointerEventData evenData, GameObject obj)
    {
        currentDragPos = evenData.position;
        disAngle=MathTool.GetAngle2(currentDragPos, targetPos)-MathTool.GetAngle2(previousDragPos, targetPos);
        if ((( disAngle < 0 && direction == 1) || (disAngle > 0 && direction == -1) || direction == 0)&&Math.Abs(disAngle)<100)
        {
            angle += disAngle;
            switch (shaft)
            {
                case 0:
                    this.transform.Rotate(new Vector3(disAngle, 0,0));
                    break;
                case 1:
                    this.transform.Rotate(new Vector3(0, disAngle, 0));
                    break;
                case 2:
                    this.transform.Rotate(new Vector3(0, 0, disAngle));
                    break;
                default:
                    break;
            }
            if (onTurnAngle != null) onTurnAngle(Math.Abs(angle));
        }
        previousDragPos = currentDragPos;
    }
    /// <summary>
    /// 结束拖拽
    /// </summary>
    /// <param name="evenData"></param>
    /// <param name="obj"></param>
    private void OnEndDrag(UnityEngine.EventSystems.PointerEventData evenData, GameObject obj)
    {
        if (onEndDrag != null) onEndDrag();
        if (isGo == false) return;
        currentDragPos = evenData.position;
        disAngle = MathTool.GetAngle2(currentDragPos, targetPos) - MathTool.GetAngle2(previousDragPos, targetPos);
        if (((disAngle < 0 && direction == 1) || (disAngle > 0 && direction == -1) || direction == 0) && Math.Abs(disAngle) < 100)
        {
            t = DOTween.To(() => disAngle, x => disAngle = x, 0f, Math.Abs(disAngle/40)).SetEase(Ease.Linear).OnUpdate(() => UpdateTween(disAngle));
        }
    }
    void UpdateTween(float num)
    {
        angle += num;
        switch (shaft)
        {
            case 0:
                this.transform.Rotate(new Vector3(num, 0, 0));
                break;
            case 1:
                this.transform.Rotate(new Vector3(0, num, 0));
                break;
            case 2:
                this.transform.Rotate(new Vector3(0, 0, num));
                break;
            default:
                break;
        }
        if (onTurnAngle != null) onTurnAngle(Math.Abs(angle));
    }
    /// <summary>
    /// 拖动方向
    /// </summary>
    /// <param name="currentDir"></param>
    /// <param name="previousDir"></param>
    /// <returns></returns>
    public int DragDirection()
    {
        if (MathTool.GetAngle2(currentDragPos, targetPos)<MathTool.GetAngle2(previousDragPos, targetPos))
        {
            //顺时针
            return 1;
        }
        else
        {
            //逆时针
            return -1;
        }
    }
    void OnEnable()
    {
        targetPos = Camera.main.WorldToScreenPoint(this.transform.position);
        EventTriggerListener etl = gameObject.GetComponent<EventTriggerListener>();
        if (etl != null) etl.enabled = true;
    }
    void OnDisable()
    {
        EventTriggerListener etl = gameObject.GetComponent<EventTriggerListener>();
        if (etl != null) etl.enabled = false;
    }
    void OnDestroy()
    {
        EventTriggerListener etl = gameObject.GetComponent<EventTriggerListener>();
        if (etl == null) return;
        Destroy(etl);
    }
}