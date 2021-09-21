# UMessage

A pub/sub event messaging system for Unity with MLAPI support

# Examples

```csharp
public class TestSubscriber : MonoBehaviour, ISubscriber<TestMessage>
{
    public bool ReceivedMessage;
    
    void ISubscriber<TestMessage>.OnPublished(TestMessage message)
    {
        Debug.Log($"TestSubscriber::TestMethod Received: {message}", this);
        ReceivedMessage = true;
    }
}
```

# Limitations

- Uses UnityEngine.JsonUtilities for json serialisation
- Uses Reflection and LINQ when using networking to find ISubscriber{T}