# UEvent

A simple [pub sub event system](https://www.altexsoft.com/blog/event-driven-architecture-pub-sub/) for Unity.

# Examples

```csharp
public class TestMessage
{
    public string PlayerName { get; set; }
}

public class TestSubscriber : MonoBehaviour
{
    // subscribe this MonoBehaviour as a message receiver

    private void Start()
    {
        // Binding will work for all ISubscriber<T>'s only need to do once at Start() or Awake()
        this.Bind<TestMessage>(TestMessageEvent);
        this.Bind<string>(GenericMessage);
    }

    // or you can bind manually incase its not a MonoBehaviour class
    private void OnEnabled()
    {
        UEvent<TestMessage>.Event += TestMessageEvent;
        UEvent<string>.Event += GenericMessage;
    }

    private void OnDisabled()
    {
        UEvent<TestMessage>.Event -= TestMessageEvent;
        UEvent<string>.Event -= GenericMessage;
    }
    
    // callbacks

    private void TestMessageEvent(TestMessage message)
    {
        Debug.Log($"{message.PlayerName} has entered the game", this);
    }

    private void GenericMessage(string message)
    {
        Debug.Log($"GenericMessage message received: {message}", this);
    }
}

...

// IMessage event
UMessage.Publish(new TestMessage{ PlayerName = "Player One" });

// Generic Message event
UMessage.Publish("Public service announcement");
```