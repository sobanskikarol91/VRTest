public class UnparentOnRelease : DependsOnGrababbleUnityEvents
{
    protected override void SubscribeEvents()
    {
        grabbableEvents.onRelease.AddListener(() => transform.SetParent(null));
    }
}
