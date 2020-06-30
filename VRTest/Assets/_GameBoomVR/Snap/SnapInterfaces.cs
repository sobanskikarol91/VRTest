using BNG;

public interface ISnapCondition
{
    bool ShouldSnap(Grabbable subject);
}

public interface IUnSnapCondition
{
    bool ShouldUnsnap(Grabbable subject);
}

public interface ISnapAreaExit
{
    void SnapExit(Grabbable subject);
}

public interface ISnapAreaEnter
{
    void SnapEnter(Grabbable subject);
}

public interface ISnapOnRelease
{
    void Snap(Grabbable subject);
}

public interface IUnsnap
{
    void OnUnsnap(GrabbableEventArgs subject);
}

public interface ISnapNotUsed
{
    void SnapNotUsed(GrabbableEventArgs subject);
}