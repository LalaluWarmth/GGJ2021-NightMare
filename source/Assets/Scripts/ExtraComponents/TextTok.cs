using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TextTok
{
    public char seg = ',';
    private List<int> _laneData = new List<int>();

    public List<int> TokText(string str)
    {
        string[] substring = str.Split(seg);
        foreach (var item in substring)
        {
            _laneData.Add(int.Parse(item));
        }

        return _laneData;
    }
}