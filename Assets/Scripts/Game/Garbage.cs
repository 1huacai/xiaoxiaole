using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage
{
    public int ID = 0;
    public Dictionary<int, int> EliminateCnt = new Dictionary<int, int>();
    public int ChainCnt = 0;
    public List<int> ComboShowed = new List<int>();
    public List<int> ChainShowed = new List<int>();
    public Dictionary<int, Transform> Parent = new Dictionary<int, Transform>();

    public void Reset()
    {
        EliminateCnt.Clear();
        ChainCnt = 0;
        ComboShowed.Clear();
        ChainShowed.Clear();
        Parent.Clear();
    }
}