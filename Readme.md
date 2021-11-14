# UEvent

A simple [pub sub event system](https://www.altexsoft.com/blog/event-driven-architecture-pub-sub/) for Unity.

# Examples

```csharp
public class TestEvent
{
    public string PlayerName { get; set; }
}

public class TestSubscriber : MonoBehaviour
{
    // Bind this MonoBehaviour to a specific event
    private void Start()
    {
        this.Bind<TestEvent>(TestEventCallback);
        this.Bind<string>(GenericEventCallback);
    }

    // or you can assign/unassign manually, useful for non MonoBehaviour classes
    private void OnEnabled()
    {
        UEvent<TestEvent>.Event += TestEventCallback;
        UEvent<string>.Event += GenericEventCallback;
    }

    private void OnDisabled()
    {
        UEvent<TestEvent>.Event -= TestEventCallback;
        UEvent<string>.Event -= GenericEventCallback;
    }
    
    // callbacks

    private void TestEventCallback(TestEvent message)
    {
        Debug.Log($"{message.PlayerName} has entered the game", this);
    }

    private void GenericEventCallback(string message)
    {
        Debug.Log($"GenericMessage message received: {message}", this);
    }
}

...

// IMessage event
UEvent.Publish(new TestEvent{ PlayerName = "Player One" });

// Generic Message event
UEvent.Publish("Public service announcement");
```