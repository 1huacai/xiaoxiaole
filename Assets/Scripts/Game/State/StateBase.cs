using System;
using System.Collections.Generic;
using UnityEngine;

// base state class
public class StateBase : Misc.StateBase
{
    protected GameController _controller;
    public StateBase(GameController controller)
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