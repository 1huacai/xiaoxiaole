using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData 
{
    private int _row;
    private int _col;
    private BlockType _type;

    public int row
    {
        get { return _row; }
        set { _row = value; }
    }

    public int col
    {
        get { return _col; }
        set { _col = value; }
    }

    public BlockType type
    {
        get { return _type; }
        set { _type = value; }
    }
}
