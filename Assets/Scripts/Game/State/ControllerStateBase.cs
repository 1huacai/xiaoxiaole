using System;
using System.Collections.Generic;
using UnityEngine;

// base state class
public class ControllerStateBase : Misc.StateBase
{
    protected GameController _controller;
    public ControllerStateBase(GameController controller)
    {
        _controller = controller;
    }
    virtual public void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        //Implemented in child class
    }
    virtual public void OnDestroy()
    {
        //Implemented in child class
        _controller = null;
    }
}