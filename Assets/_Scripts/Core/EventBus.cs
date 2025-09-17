using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Enum, Action<object>> subs = new();

    public static void On(Enum eventId, Action<object> cb)
    {
        if (subs.TryGetValue(eventId, out var d))
            subs[eventId] = d + cb;
        else
            subs[eventId] = cb;
    }

    public static void Off(Enum eventId, Action<object> cb)
    {
        if (subs.TryGetValue(eventId, out var d))
        {
            d -= cb;
            if (d == null) subs.Remove(eventId);
            else subs[eventId] = d;
        }
    }

    public static void Off(Enum eventId, object context)
    {
        
    }

    public static void Emit(Enum eventId, object data = null)
    {
        if (subs.TryGetValue(eventId, out var d))
            d.Invoke(data);
    }

    public static void Clear() => subs.Clear();
}