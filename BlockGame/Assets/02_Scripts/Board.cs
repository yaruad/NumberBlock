using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField]
    private NodeSpawner nodeSpawner;
    [SerializeField]
    private GameObject blockPrefab;
    [SerializeField]
    private Transform blockRect;

    public List<Node> NodeList {  get; private set; }
    public Vector2Int BlockCount { get; private set; }

    private void Awake()
    {
        BlockCount = new Vector2Int(4, 4);
        NodeList = nodeSpawner.SpawnNodes(BlockCount);  //노드 블록 판 생성, 모든 노드의 정보를 NodeList에 저장
    }

    private void Start()
    {
        //노드의 위치에 블록을 생성하기 위해 Rebuild로 노드들의 위치를 갱싱
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSpawner.GetComponent<RectTransform>());

        foreach (Node node in NodeList)
        {
            node.localPosition = node.GetComponent<RectTransform>().localPosition;
        }

        //숫자블록 2개 생성
        SpawnBlockToRandomNode();
        SpawnBlockToRandomNode();
    }

    private void Update()
    {
        if (Input.GetKeyDown("1")) SpawnBlockToRandomNode();
    }

    private void SpawnBlockToRandomNode()
    {
        List<Node> emptyNodes = NodeList.FindAll(x => x.placedBlock == null);

        if (emptyNodes.Count != 0)
        {
            int index = Random.Range(0, emptyNodes.Count);
            Vector2Int point = emptyNodes[index].Point;
            SpawnBlock(point.x, point.y);
        }
        else
        {
            //게임오버 조건을 검사하고 게임오버처리
        }
    }

    private void SpawnBlock(int x, int y)
    {
        if (NodeList[y * BlockCount.x + x].placedBlock != null) return;     //해당 위치에 블록이 배치되어 있으면 return

        GameObject clone = Instantiate(blockPrefab, blockRect);
        Block block = clone.GetComponent<Block>();
        Node node = NodeList[y* BlockCount.x + x];

        clone.GetComponent<RectTransform>().localPosition = node.localPosition;     //생성한 블록의 위치를 노드의 위치와 동일하게 설정
        block.Setup();
        node.placedBlock = block;       //방금생성한 블록을 노드에 등록
    }
}
