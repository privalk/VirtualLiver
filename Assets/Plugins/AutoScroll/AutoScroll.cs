using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


//滚动方向
public enum ScrollDir
{
    BottomToTop = 1,
    TopToBottom = 2,
    LeftToRight = 3,
    RightToLeft = 4
}



[RequireComponent(typeof(ScrollRect))]
public class AutoScroll : MonoBehaviour
{
    [Header("自动滑动方向")]
    public ScrollDir AutoScrollDir = ScrollDir.BottomToTop;   //自动滑动方向
    [Header("滚动步长")]
    public float step = 0.01f;

    HorizontalOrVerticalLayoutGroup LayoutGroup;   //横竖分布处理
    GridLayoutGroup GridGroup;       //网格分布
    float Space = 0;   //间隔数据

    bool scroll = true;
    bool isScroll
    {

        set
        {
            scroll = value;
            if (scrollrect != null)
                scrollrect.enabled = !scroll;
        }

        get
        {
            return scroll;
        }
    }
    EventTrigger et;

    ScrollRect scrollrect = null;
    RectTransform scrolltran;
    float ItemWidth, ItemHeight;
    private void Start()
    {
        scrollrect = gameObject.GetComponent<ScrollRect>();
        scrolltran = scrollrect.GetComponent<RectTransform>();
        LayoutGroup = scrollrect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
        GridGroup = scrollrect.content.GetComponent<GridLayoutGroup>();

        et = gameObject.GetComponent<EventTrigger>();
        if (et == null)
            et = gameObject.AddComponent<EventTrigger>();

        //设置滚动间隔
        if (LayoutGroup != null)
            Space = LayoutGroup.spacing;
        else if (GridGroup != null)
        {
            switch (AutoScrollDir)
            {
                case ScrollDir.BottomToTop://由底至顶滚动  向上
                case ScrollDir.TopToBottom://由顶至底滚动  向下
                    Space = GridGroup.spacing.y;
                    break;
                case ScrollDir.LeftToRight://由左至右滚动 →
                case ScrollDir.RightToLeft://由右至左滚动 ←
                    Space = GridGroup.spacing.x;
                    break;
                default:
                    Space = 0;
                    break;
            }

        }

        //设置子节点高度和宽度
        if (LayoutGroup != null && scrollrect.content.childCount > 0)
        {
            ItemWidth = scrollrect.content.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            ItemHeight = scrollrect.content.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
        }
        else if (GridGroup != null)
        {
            ItemWidth = GridGroup.cellSize.x;
            ItemHeight = GridGroup.cellSize.y;
        }
        //AddETEvent(et, EventTriggerType.PointerEnter, OnPointerIn);
        //AddETEvent(et, EventTriggerType.PointerExit, OnPointerOut);
    }


    private void OnPointerIn(BaseEventData data)
    {
        if (IsInvoking("StartAutoScroll"))
            CancelInvoke("StartAutoScroll");
        isScroll = false;
    }


    private void OnPointerOut(BaseEventData data)
    {
        Invoke("StartAutoScroll", 1f);
    }


    //开始自动滚动
    void StartAutoScroll()
    {
        isScroll = true;
    }

    private void AddETEvent(EventTrigger et, EventTriggerType ei, UnityAction<BaseEventData> ua)
    {
        UnityAction<BaseEventData> action = ua;
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = ei;
        entry.callback.AddListener(action);
        et.triggers.Add(entry);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isScroll)
            DoAutoScroll();
    }

    //开始自动滑动
    void DoAutoScroll()
    {
        switch (AutoScrollDir)
        {
            //由底至顶滚动  向上
            case ScrollDir.BottomToTop:
                {
                    if (scrollrect.content.sizeDelta.y > scrolltran.sizeDelta.y + (ItemHeight + Space))
                    {
                        scrollrect.content.anchoredPosition3D += new Vector3(0, step, 0);
                        if (scrollrect.content.anchoredPosition3D.y >= (scrollrect.content.sizeDelta.y - scrolltran.sizeDelta.y) / 2)
                        {
                            if (GridGroup != null && GridGroup.constraintCount > 1)
                            {
                                for (int i = 0; i < GridGroup.constraintCount; i++)
                                    scrollrect.content.GetChild(0).transform.SetAsLastSibling();
                                scrollrect.content.anchoredPosition3D -= new Vector3(0, (ItemHeight + Space), 0);
                            }
                            else
                            {
                                scrollrect.content.GetChild(0).transform.SetAsLastSibling();
                                scrollrect.content.anchoredPosition3D -= new Vector3(0, (ItemHeight + Space), 0);
                            }
                        }
                    }
                }
                break;
            //由顶至底滚动  向下
            case ScrollDir.TopToBottom:
                {
                    if (scrollrect.content.sizeDelta.y > scrolltran.sizeDelta.y + (ItemHeight + Space))
                    {
                        scrollrect.content.anchoredPosition3D -= new Vector3(0, step, 0);
                        if (scrollrect.content.anchoredPosition3D.y <= (scrollrect.content.sizeDelta.y - scrolltran.sizeDelta.y) / 2)
                        {
                            if (GridGroup != null && GridGroup.constraintCount > 1)
                            {
                                for (int i = 0; i < GridGroup.constraintCount; i++)
                                    scrollrect.content.GetChild(scrollrect.content.childCount - 1).transform.SetAsFirstSibling();
                                scrollrect.content.anchoredPosition3D += new Vector3(0, (ItemHeight + Space), 0);
                            }
                            else
                            {
                                scrollrect.content.GetChild(scrollrect.content.childCount - 1).transform.SetAsFirstSibling();
                                scrollrect.content.anchoredPosition3D += new Vector3(0, (ItemHeight + Space), 0);
                            }
                        }
                    }
                }
                break;

            //由左至右滚动 →
            case ScrollDir.LeftToRight:
                {
                    if (scrollrect.content.sizeDelta.x > scrolltran.sizeDelta.x + (ItemWidth + Space))
                    {
                        scrollrect.content.anchoredPosition3D += new Vector3(step, 0, 0);
                        if (scrollrect.content.anchoredPosition3D.x >= -(scrollrect.content.sizeDelta.x - scrolltran.sizeDelta.x) / 2)
                        {
                            if (GridGroup != null && GridGroup.constraintCount > 1)
                            {
                                for (int i = 0; i < GridGroup.constraintCount; i++)
                                    scrollrect.content.GetChild(scrollrect.content.childCount - 1).transform.SetAsFirstSibling();
                                scrollrect.content.anchoredPosition3D -= new Vector3((ItemWidth + Space), 0, 0);
                            }
                            else
                            {
                                scrollrect.content.GetChild(scrollrect.content.childCount - 1).transform.SetAsFirstSibling();
                                scrollrect.content.anchoredPosition3D -= new Vector3((ItemWidth + Space), 0, 0);
                            }
                        }
                    }
                }
                break;
            //由右至左滚动 ←
            case ScrollDir.RightToLeft:
                {
                    if (scrollrect.content.sizeDelta.x > scrolltran.sizeDelta.x + (ItemWidth + Space))
                    {
                        scrollrect.content.anchoredPosition3D -= new Vector3(step, 0, 0);
                        if (scrollrect.content.anchoredPosition3D.x <= -(scrollrect.content.sizeDelta.x - scrolltran.sizeDelta.x) / 2)
                        {
                            if (GridGroup != null && GridGroup.constraintCount > 1)
                            {
                                for (int i = 0; i < GridGroup.constraintCount; i++)
                                    scrollrect.content.GetChild(0).transform.SetAsLastSibling();
                                scrollrect.content.anchoredPosition3D += new Vector3((ItemWidth + Space), 0, 0);
                            }
                            else
                            {
                                scrollrect.content.GetChild(0).transform.SetAsLastSibling();
                                scrollrect.content.anchoredPosition3D += new Vector3((ItemWidth + Space), 0, 0);
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }
    }
}
