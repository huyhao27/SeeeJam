using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<string, Action<object>> sceneSubs = new();       // event chỉ sống trong scene
    private static readonly Dictionary<string, Action<object>> persistentSubs = new();  // event sống xuyên scene

    // ----- Đăng ký -----
    public static void On(string eventId, Action<object> cb)
    {
        if (sceneSubs.TryGetValue(eventId, out var d))
            sceneSubs[eventId] = d + cb;
        else
            sceneSubs[eventId] = cb;
    }

    public static void OnPersistent(string eventId, Action<object> cb)
    {
        if (persistentSubs.TryGetValue(eventId, out var d))
            persistentSubs[eventId] = d + cb;
        else
            persistentSubs[eventId] = cb;
    }

    // ----- Hủy đăng ký -----
    public static void Off(string eventId, Action<object> cb)
    {
        if (sceneSubs.TryGetValue(eventId, out var d))
        {
            d -= cb;
            if (d == null) sceneSubs.Remove(eventId);
            else sceneSubs[eventId] = d;
        }

        if (persistentSubs.TryGetValue(eventId, out var d2))
        {
            d2 -= cb;
            if (d2 == null) persistentSubs.Remove(eventId);
            else persistentSubs[eventId] = d2;
        }
    }

    // ----- Emit -----
    public static void Emit(string eventId, object data = null)
    {
        if (sceneSubs.TryGetValue(eventId, out var d)) d.Invoke(data);
        if (persistentSubs.TryGetValue(eventId, out var d2)) d2.Invoke(data);
    }

    // ----- Overload cho Enum -----
    public static void On(Enum eventId, Action<object> cb) => On(eventId.ToString(), cb);
    public static void OnPersistent(Enum eventId, Action<object> cb) => OnPersistent(eventId.ToString(), cb);
    public static void Off(Enum eventId, Action<object> cb) => Off(eventId.ToString(), cb);
    public static void Emit(Enum eventId, object data = null) => Emit(eventId.ToString(), data);

    // ----- Clear -----
    public static void ClearSceneEvents() => sceneSubs.Clear(); // gọi khi chuyển scene
    public static void ClearAll()
    {
        sceneSubs.Clear();
        persistentSubs.Clear();
    }
}