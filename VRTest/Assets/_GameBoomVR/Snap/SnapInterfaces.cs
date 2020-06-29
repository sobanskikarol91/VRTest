using BNG;
using System;

public interface ISnapCondition
{
    bool ShouldSnap(Grabbable subject);
}

public interface IUnSnapCondition
{
    bool ShouldUnsnap(Grabbable subject);
}

public interface ISnapEffect
{
    void SnapEffect(Grabbable subject);
    event Action OnCompletedAnimation;
}

public interface ISnapAreaExit
{
    void SnapExit(Grabbable subject);
}

public interface ISnapAreaEnter
{
    void SnapEnter(Grabbable subject);
}
