using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEvent
{
    public delegate void VoidDelegate();
    public delegate void DelegateWithEventData(PointerEventData eventData);
    public delegate void DelegateWithBaseEventData(BaseEventData eventData);

    private static T GetHandler<T>(GameObject go) where T : Component
    {
        T listener = go.GetComponent<T>();
        if (listener == null) listener = go.AddComponent<T>();
        return listener;
    }
    public static void BindClick(GameObject go, UIEvent.VoidDelegate fun)
    {
        GetHandler<EventTriggerClick>(go).onClick = fun;
    }
    public static void BindEventClick(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerClick>(go).onClickWithEvtent = fun;
    }
    public static void BindEnter(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerEnterExit>(go).onEnter = fun;
    }
    public static void BindExit(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerEnterExit>(go).onExit = fun;
    }
    public static void BindDown(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerDownUp>(go).onDown = fun;
    }
    public static void BindUp(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerDownUp>(go).onUp = fun;
    }
    public static void BindBeginDrag(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerDrag>(go).onBeginDrag = fun;
    }
    public static void BindDrag(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerDrag>(go).onDrag = fun;
    }
    public static void BindEndDrag(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerDrag>(go).onEndDrag = fun;
    }
    public static void BindInitializePotentialDrag(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerDrag>(go).onInitializePotentialDrag = fun;
    }
    public static void BindDrop(GameObject go, UIEvent.DelegateWithEventData fun)
    {
        GetHandler<EventTriggerDrag>(go).onDrop = fun;
    }
    public static void BindSelect(GameObject go, UIEvent.DelegateWithBaseEventData fun)
    {
        GetHandler<EventTriggerSelect>(go).onSelect = fun;
    }
    public static void BindUpdateSelect(GameObject go, UIEvent.DelegateWithBaseEventData fun)
    {
        GetHandler<EventTriggerSelect>(go).onUpdateSelect = fun;
    }
    public static void BindDeselect(GameObject go, UIEvent.DelegateWithBaseEventData fun)
    {
        GetHandler<EventTriggerSelect>(go).onDeselect = fun;
    }
    public static void AddDestroyListener(GameObject go,UIEvent.VoidDelegate fun)
    {
        GetHandler<EventDestroy>(go).onDestroy += fun;
    }
}

public class EventTriggerClick : MonoBehaviour, IPointerClickHandler
{
    public UIEvent.VoidDelegate onClick;
    public UIEvent.DelegateWithEventData onClickWithEvtent;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick();
        if (onClickWithEvtent != null) onClickWithEvtent(eventData);
    }
}

public class EventTriggerEnterExit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UIEvent.DelegateWithEventData onEnter;
    public UIEvent.DelegateWithEventData onExit;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(eventData);
    }
}

public class EventTriggerDownUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UIEvent.DelegateWithEventData onDown;
    public UIEvent.DelegateWithEventData onUp;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(eventData);
    }
}


public class EventTriggerDrag : MonoBehaviour, IBeginDragHandler, IInitializePotentialDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public UIEvent.DelegateWithEventData onBeginDrag;
    public UIEvent.DelegateWithEventData onInitializePotentialDrag;
    public UIEvent.DelegateWithEventData onDrag;
    public UIEvent.DelegateWithEventData onEndDrag;
    public UIEvent.DelegateWithEventData onDrop;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (onBeginDrag != null) onBeginDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null) onDrag(eventData);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (onEndDrag != null) onEndDrag(eventData);
    }
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (onInitializePotentialDrag != null) onInitializePotentialDrag(eventData);
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (onDrop != null) onDrop(eventData);
    }
}

public class EventTriggerSelect : MonoBehaviour, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler
{
    public UIEvent.DelegateWithBaseEventData onSelect;
    public UIEvent.DelegateWithBaseEventData onDeselect;
    public UIEvent.DelegateWithBaseEventData onUpdateSelect;
    public void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(eventData);
    }
    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(eventData);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (onDeselect != null) onDeselect(eventData);
    }
}


public class EventDestroy : MonoBehaviour
{
    public UIEvent.VoidDelegate onDestroy;

    public void OnDestroy()
    {
        if (onDestroy != null) {
            onDestroy();
        }
    }
}