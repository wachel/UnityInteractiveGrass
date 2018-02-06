using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public sealed class ProxyPublishData
{
    public string m_EventName;
    public string m_Group;
    public object[] m_Args;
}
public sealed class MessageSystem
{
    private class ReceiptInfo
    {
        public string name_;
        public Delegate delegate_;
        public ReceiptInfo() { }
        public ReceiptInfo(string n, Delegate d)
        {
            name_ = n;
            delegate_ = d;
        }
    }

    //public static object Subscribe(string ev_name, string group, MyAction subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0>(string ev_name, string group, MyAction<T0> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1>(string ev_name, string group, MyAction<T0, T1> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2>(string ev_name, string group, MyAction<T0, T1, T2> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3>(string ev_name, string group, MyAction<T0, T1, T2, T3> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    //public static object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public static void Unsubscribe(object receipt)
    {
        ReceiptInfo r = receipt as ReceiptInfo;
        Delegate d;
        if (null != r && subscribers_.TryGetValue(r.name_, out d)) {
            Delegate left = Delegate.Remove(d, r.delegate_);
            if (null == left) {
                subscribers_.Remove(r.name_);
            } else {
                subscribers_[r.name_] = left;
            }
        }
    }

    public static void Publish(string ev_name, params object[] parameters)
    {
        try {
            //LogSystem.Info("Publish {0} {1}", ev_name, group);

            Delegate d;
            string key = ev_name;
            if (subscribers_.TryGetValue(key, out d)) {
                if (null == d) {
                    //LogSystem.Error("Publish {0} {1}, Subscriber is null, Remove it", ev_name, group);
                    subscribers_.Remove(key);
                } else {
                    d.DynamicInvoke(parameters);
                }
            }
        } catch (Exception ex) {
            if (null != ex.InnerException) {
                ex = ex.InnerException;
            }
            //LogSystem.Error("PublishSubscribe.Publish({0},{1}) exception:{2}\n{3}", ev_name, group, ex.Message, ex.StackTrace);
        }
    }

    public static object AddSubscriber(string ev_name, Delegate d)
    {
        Delegate source;
        string key = ev_name;
        if (subscribers_.TryGetValue(key, out source)) {
            if (null != source)
                source = Delegate.Combine(source, d);
            else
                source = d;
        } else {
            source = d;
        }
        subscribers_[key] = source;
        return new ReceiptInfo(key, d);
    }

    private static Dictionary<string, Delegate> subscribers_ = new Dictionary<string, Delegate>();
}


public delegate void MyAction();
public delegate void MyAction<T1>(T1 t1);
public delegate void MyAction<T1, T2>(T1 t1, T2 t2);
public delegate void MyAction<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
public delegate void MyAction<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
public delegate void MyAction<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
public delegate void MyAction<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15);
public delegate void MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16);


public class Subscriber:IDisposable
{
    List<object> list = new List<object>();
    public void Add(string ev_name, MyAction subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0>(string ev_name, MyAction<T0> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1>(string ev_name, MyAction<T0, T1> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2>(string ev_name, MyAction<T0, T1, T2> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3>(string ev_name, MyAction<T0, T1, T2, T3> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4>(string ev_name, MyAction<T0, T1, T2, T3, T4> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> subscriber) { AddSubscriber(ev_name, subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> subscriber) { AddSubscriber(ev_name,  subscriber); }
    public void Add<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string ev_name, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> subscriber) { AddSubscriber(ev_name,  subscriber); }

    public void Dispose()
    {
        for(int i = 0; i<list.Count; i++) {
            MessageSystem.Unsubscribe(list[i]);
        }
        list.Clear();
    }

    void AddSubscriber(string ev_name, Delegate d)
    {
        list.Add(MessageSystem.AddSubscriber(ev_name, d));
    }
}