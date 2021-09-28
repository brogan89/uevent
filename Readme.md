# UMessage

A pub/sub event messaging system for Unity with MLAPI support

## Why use this and not just use RPCs or NetworkVariables from MLAPI?

The main benefit from using this system is that you don't need to apply a `NetworkBehaviour` and `NetworkObject` all the time which gives you more flexibility. However, use with caution as there is extra overhead on the client side when using this system, for messages that require higher usage it would be best to stick to NetworkVariables or RPCs provided by MLAPI.


# Examples

```csharp
public class TestSubscriber : MonoBehaviour, ISubscriber<TestMessage>
{
    // subscribe this MonoBehaviour as a message receiver

    private void Start()
    {
        // Binding will work for all ISubscriber<T>'s only need to do once at Start() or Awake()
        UMessage.Bind(this);
    }

    // or you can bind manually incase its not a MonoBehaviour class

    private void OnEnabled()
    {
        UMessage.Sub(this);
    }

    private void OnDisabled()
    {
        UMessage.UnSub(this);
    }
    
    // callbacks

    // Direct interface ISubscriber<T> callback method
    void ISubscriber<TestMessage>.OnPublished(TestMessage message)
    {
        Debug.Log($"ISubscriber<TestMessage> message received: {message}", this);
    }

    // Attribute callbacks via GenericMessage class
    // Method name matches the event name
    [MessageCallback]
    private void GenericMessage(string message)
    {
        Debug.Log($"GenericMessage message received: {message}", this);
    }

    // Method name doesn't match event name but event name is provided
    [MessageCallback("GenericMessage")]
    private void UnrelatedMethodName(string message)
    {
        Debug.Log($"UnrelatedMethodName message received: {message}", this);
    }
}

...

// IMessage event
UMessage.Publish(new TestMessage());

// Generic Message event
UMessage.Publish("GenericMessage", "Hello, World!");
```

# Limitations

- Uses UnityEngine.JsonUtilities for json serialisation
- Uses Reflection and LINQ when using networking to find ISubscriber{T} and method attributes