using System;
using System.Collections.Generic;

namespace J2534
{
    //The Sieve class is a filter used to validate incomming messages and distribute them to the appropiate
    //Calling thread.  The concept is a Sieve with multiple filter screens.  Each screen catches different
    //messages.  Whatever isn't caught in a screen is discarded.
    internal class Sieve
    {
        private object LOCK = new object();
        private List<FilterScreen> Screens = new List<FilterScreen>();

        public void AddScreen(int Priority, Predicate<J2534Message> Predicate)
        {
            lock (LOCK)
            {
                Screens.Add(new FilterScreen(Priority, Predicate));
                Screens.Sort((S1, S2) => { return S1.Priority - S2.Priority; });
            }
        }

        public void RemoveScreen(Predicate<J2534Message> ComparerAsKey)
        {
            lock (LOCK)
                Screens.Remove(Screens.Find(Screen => Screen.Comparer == ComparerAsKey));
        }

        public void ExtractFrom(List<J2534Message> Messages)
        {
            Messages.ForEach(Message =>
            {
                lock (LOCK)
                    foreach (FilterScreen Screen in Screens)
                    {
                        if (Screen.Comparer(Message))
                        {
                            Screen.Messages.Add(Message);
                            break;
                        }
                    }
            });
        }

        public int ScreenMessageCount(Predicate<J2534Message> ComparerAsKey)
        {
            lock (LOCK) //This will throw an exception if predicate is not found.  That is probably best.
                return Screens.Find(Screen => Screen.Comparer == ComparerAsKey).Messages.Count;
        }

        public List<J2534Message> EmptyScreen(Predicate<J2534Message> ComparerAsKey, bool Remove)
        {
            lock (LOCK)
            {
                FilterScreen Screen = Screens.Find(_Screen => _Screen.Comparer == ComparerAsKey);
                if (Remove)
                    Screens.Remove(Screen);
                else
                    Screen.Messages = new List<J2534Message>();
                return Screen.Messages;
            }
        }
    }

    internal class FilterScreen
    {
        public int Priority { get; set; }
        public List<J2534Message> Messages = new List<J2534Message>();
        public Predicate<J2534Message> Comparer;
        public FilterScreen(int Priority, Predicate<J2534Message> Comparer)
        {
            this.Priority = Priority;
            this.Comparer = Comparer;
        }
    }
}
