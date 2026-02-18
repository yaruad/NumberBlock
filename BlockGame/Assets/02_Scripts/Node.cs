using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { None = -1, Right = 0, Down, Left, Up}

public class Node : MonoBehaviour
{
    public Block placedBlock;   //횬재 노드에 배치되어 있는 블록 정보
    public Vector2 localPosition;   //현재 노드의 RectTransform 로컬좌표
    public bool combined =false;    //현재 노드에 있는 블록이 병합중인지 체크

    public Vector2Int Point {  get; private set; }  //현재 노드의 x, y 격자 좌표 정보(좌상단 0,0)
    public Vector2Int?[] NeighborNodes { get; private set; } //현재 노드에 인접한 노드의 격자 좌표(없으면 Null)

    private Board board;
    
    public void SetUp(Board board, Vector2Int?[] neighborNodes, Vector2Int point) //?:0이 아닌 비어 있는 값으로 null상태를 가질 수 있음
    {
        this.board = board;
        NeighborNodes = neighborNodes;
        Point = point;
    }

    public Node FindTarget(Node originalNode, Direction direction, Node farNode = null)
    {
        //direction 방향에 노드가 있으면
        if (NeighborNodes[(int)direction].HasValue == true)
        {
            //direction 방향에 있는 인접 노드 정보
            Vector2Int point = NeighborNodes[(int)direction].Value;
            Node neighborNode = board.NodeList[point.y * board.BlockCount.x + point.x];
            
            //인접 노드가 병합 상태면 this 반환
            if (neighborNode != null && neighborNode.combined)
            {
                return this;
            }

            //두 노드 모두 블록이 배치되어 있고
            if (neighborNode.placedBlock != null && originalNode.placedBlock != null)
            {
                if (neighborNode.placedBlock.Numeric == originalNode.placedBlock.Numeric)
                {
                    return neighborNode;
                }
                else
                {
                    return farNode;
                }
            }

            //인접노드에 블록이 없으면 제귀함수 호출 (여러 칸 이동 위해)
            return neighborNode.FindTarget(originalNode, direction, neighborNode);
        }
        return farNode;
    }
}
