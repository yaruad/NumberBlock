using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject nodePrefab;
    [SerializeField]
    private RectTransform nodeRect; //생성한 노드들의 부모

    public List<Node> SpawnNodes(Board board, Vector2Int blockCount)
    {
        List<Node> nodeList = new List<Node>(blockCount.x * blockCount.y);

        for (int y = 0; y < blockCount.y; ++y)
        {
            for (int x = 0; x < blockCount.x; ++x)
            {
                GameObject clone = Instantiate(nodePrefab, nodeRect.transform);

                Vector2Int point = new Vector2Int(x, y);

                //인접 노드 정보 저장 (인접 노드가 없으면 Null)
                Vector2Int?[] neighborNodes = new Vector2Int?[4];

                Vector2Int right = point + Vector2Int.right;
                Vector2Int down = point + Vector2Int.up;
                Vector2Int left = point + Vector2Int.left;
                Vector2Int up = point + Vector2Int.down;

                if (IsVaild(right, blockCount)) neighborNodes[0] = right;
                if (IsVaild(down, blockCount)) neighborNodes[1] = down;
                if (IsVaild(left, blockCount)) neighborNodes[2] = left;
                if (IsVaild(up, blockCount)) neighborNodes[3] = up;

                Node node = clone.GetComponent<Node>();
                node.SetUp(board, neighborNodes, point);

                clone.name = $"[{node.Point.y}, {node.Point.x}]";

                nodeList.Add(node);
            }
        }

        return nodeList;
    }

    private bool IsVaild(Vector2Int point, Vector2Int blockCount)
    {
        if (point.x == -1 || point.x == blockCount.x || point.y == blockCount.y || point.y == -1)
        {
            return false; 
        }
        return true;
    }
}
