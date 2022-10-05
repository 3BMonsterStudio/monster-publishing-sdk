#define EVENTROUTER_THROWEXCEPTIONS 
#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // Uncomment this if you want listeners to be required for sending events.
#endif

using System;
using System.Collections.Generic;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif
/// <summary>
/// GameEvents are used throughout the game for general game events (game started, game ended, life lost, etc.)
/// </summary>
/// 
namespace ImoSysSDK.Others
{
    public struct GameEvent
    {
        public string EventName;
        public GameEvent(string newName)
        {
            EventName = newName;
        }
    }

    public struct IapCallBack
    {
        public bool isSuccess;
        public string productId;
        public string response;
        public int instanceId;
#if UNITY_PURCHASING
public Product product;
#endif

        public bool isRestored;
        #if UNITY_PURCHASING
        public IapCallBack(bool _isSuccess, string _productId, string _response, int _instanceId, Product _product, bool _isRestored)
        {
            isSuccess = _isSuccess;
            productId = _productId;
            response = _response;
            instanceId = _instanceId;
            product = _product;
            isRestored = _isRestored;
        }
#endif
    }

    /// <summary>
    /// This class handles event management, and can be used to broadcast events throughout the game, to tell one class (or many) that something's happened.
    /// Events are structs, you can define any kind of events you want. This manager comes with MMGameEvents, which are 
    /// basically just made of a string, but you can work with more complex ones if you want.
    /// 
    /// To trigger a new event, from anywhere, just call EventManager.TriggerEvent(YOUR_EVENT);
    /// For example : EventManager.TriggerEvent(new GameEvent("GameStart")); will broadcast an GameEvent named GameStart to all listeners.
    ///
    /// To start listening to an event from any class, there are 3 things you must do : 
    ///
    /// 1 - tell that your class implements the EventListener interface for that kind of event.
    /// For example: public class GUIManager : Singleton<GUIManager>, EventListener<MMGameEvent>
    /// You can have more than one of these (one per event type).
    ///
    /// 2 - On Enable and Disable, respectively start and stop listening to the event :
    /// void OnEnable()
    /// {
    /// 	this.EventStartListening<GameEvent>();
    /// }
    /// void OnDisable()
    /// {
    /// 	this.EventStopListening<GameEvent>();
    /// }
    /// 
    /// 3 - Implement the EventListener interface for that event. For example :
    /// public void OnEvent(GameEvent gameEvent)
    /// {
    /// 	if (gameEvent.eventName == "GameOver")
    ///		{
    ///			// DO SOMETHING
    ///		}
    /// } 
    /// will catch all events of type GameEvent emitted from anywhere in the game, and do something if it's named GameOver
    /// </summary>
    public static class EventManager
    {

        private static Dictionary<Type, List<EventListenerBase>> _subscribersList;

        static EventManager()
        {
            _subscribersList = new Dictionary<Type, List<EventListenerBase>>();
        }

        /// <summary>
        /// Adds a new subscriber to a certain event.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="Event">The event type.</typeparam>
        public static void AddListener<Event>(EventListener<Event> listener) where Event : struct
        {
            Type eventType = typeof(Event);

            if (!_subscribersList.ContainsKey(eventType))
                _subscribersList[eventType] = new List<EventListenerBase>();

            if (!SubscriptionExists(eventType, listener))
                _subscribersList[eventType].Add(listener);
        }

        /// <summary>
        /// Removes a subscriber from a certain event.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="Event">The event type.</typeparam>
        public static void RemoveListener<Event>(EventListener<Event> listener) where Event : struct
        {
            Type eventType = typeof(Event);

            if (!_subscribersList.ContainsKey(eventType))
            {
#if EVENTROUTER_THROWEXCEPTIONS
                throw new ArgumentException(string.Format("Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString()));
#else
            return;
#endif
            }

            List<EventListenerBase> subscriberList = _subscribersList[eventType];
            bool listenerFound = false;

            foreach (EventListenerBase subscriber in subscriberList)
            {
                if (subscriber == listener)
                {
                    subscriberList.Remove(subscriber);
                    listenerFound = true;

                    if (subscriberList.Count == 0)
                        _subscribersList.Remove(eventType);

                    return;
                }
            }

#if EVENTROUTER_THROWEXCEPTIONS
            if (!listenerFound)
            {
                throw new ArgumentException(string.Format("Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString()));
            }
#endif
        }

        /// <summary>
        /// Triggers an event. All instances that are subscribed to it will receive it (and will potentially act on it).
        /// </summary>
        /// <param name="newEvent">The event to trigger.</param>
        /// <typeparam name="Event">The 1st type parameter.</typeparam>
        public static void TriggerEvent<Event>(Event newEvent) where Event : struct
        {
            List<EventListenerBase> list;
            if (!_subscribersList.TryGetValue(typeof(Event), out list))
#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
#else
                return;
#endif

            foreach (EventListenerBase b in list)
            {
                (b as EventListener<Event>).OnEvent(newEvent);
            }
        }

        /// <summary>
        /// Checks if there are subscribers for a certain type of events
        /// </summary>
        /// <returns><c>true</c>, if exists was subscriptioned, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="receiver">Receiver.</param>
        private static bool SubscriptionExists(Type type, EventListenerBase receiver)
        {
            List<EventListenerBase> receivers;

            if (!_subscribersList.TryGetValue(type, out receivers)) return false;

            bool exists = false;

            foreach (EventListenerBase subscription in receivers)
            {
                if (subscription == receiver)
                {
                    exists = true;
                    break;
                }
            }

            return exists;
        }
    }
    /// <summary>
    /// Static class that allows any class to start or stop listening to events
    /// </summary>
    public static class EventRegister
    {
        public delegate void Delegate<T>(T eventType);

        public static void EventStartListening<EventType>(this EventListener<EventType> caller) where EventType : struct
        {
            EventManager.AddListener<EventType>(caller);
        }

        public static void EventStopListening<EventType>(this EventListener<EventType> caller) where EventType : struct
        {
            EventManager.RemoveListener<EventType>(caller);
        }
    }

    /// <summary>
    /// Event listener basic interface
    /// </summary>
    public interface EventListenerBase { };

    /// <summary>
    /// A public interface you'll need to implement for each type of event you want to listen to.
    /// </summary>
    public interface EventListener<T> : EventListenerBase
    {
        void OnEvent(T eventType);
    }
}