using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Block placedBlock;   //횬재 노드에 배치되어 있는 블록 정보
    public Vector2 localPosition;   //현재 노드의 RectTransform 로컬좌표
    public Vector2Int Point {  get; private set; }  //현재 노드의 x, y 격자 좌표 정보(좌상단 0,0)
    
    public void SetUp(Vector2Int point)
    {
        Point = point;
    }
}
