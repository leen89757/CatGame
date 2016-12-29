using System;
using System.Threading;

namespace CardGame
{
    public class MessageEventAgrs : EventArgs
    {
        public MessageEventAgrs(int what, object obj)
        {
            this.What = what;
            this.Obj = obj;
        }

        public int What { get; set; }
        public object Obj { get; set; }
    }

    public static class EventArgExtensions
    {
        public static void Raise<TEventArgs>(this TEventArgs e,
            Object sender, ref EventHandler<TEventArgs> eventDelegate)
        where TEventArgs : EventArgs
        {
            //for threading security, use a temp field to store delegate reference
            var temp = Interlocked.CompareExchange(ref eventDelegate, null, null);

            //notify any method registered the event  
            temp?.Invoke(sender, e);
        }
    }

}