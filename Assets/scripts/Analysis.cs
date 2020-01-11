using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class Analysis
{
    public long id;
    public int TargetHeight;
    public int MaxHeight = -111;
    public int MinHeight = 10000;
    public int[] Heights;
    
    public int TargetWidth;
    public int MaxWidth;
    public int MinWidth;
    public int[] Widths;

    public int NumberOfBlocks;
    public float TotalFreeSpace;
    public float InsideFreeSpace;

    public int NumberOfPlacesForSmallPigs;
    public int NumberOfPlacesForBigPigs;

    public int NumberOfBirdsRequiredToClearScreen;
    public int LevelDifficulty;


    public void CreateCsv()
    {
        var str = new StringBuilder();
        var titles = new StringBuilder();
        titles.Append("targetWidth,").Append("targetheight,").Append("maxWidth,").Append("minWidth,").Append("maxHeight,")
            .Append("minHeight");
        str.Append(TargetWidth+",").Append(TargetHeight+",").Append(MaxWidth+",").Append(MinWidth+",").Append(MaxHeight+",")
            .Append(MinHeight+",");

        for (var i = 0; i < TargetHeight; i++)
        {
            titles.Append("w" + i+",");
            str.Append(Widths[i]+",");
        }
        
        for (var i = 0; i < TargetWidth; i++)
        {
            titles.Append("h" + i+",");
            str.Append(Heights[i]+",");
        }
        
        var path = "Assets/Resources/"+id+"_analysis.csv";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(titles.ToString());
        writer.WriteLine(str.ToString());
        writer.Close();   
    }

    public void CreateJson()
    {
        var path = "Assets/Resources/"+id+"_analysis.json";
        var json = JsonUtility.ToJson(this);
        var writer = new StreamWriter(path,false);
        writer.WriteLine(json);
        writer.Close();
    }
}