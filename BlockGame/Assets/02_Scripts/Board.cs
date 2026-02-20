using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { Wait = 0, Processing, End }
public class Board : MonoBehaviour
{
    [SerializeField]
    private NodeSpawner nodeSpawner;
    [SerializeField]
    private TouchController touchController;
    [SerializeField]
    private UIController uiController;
    [SerializeField]
    private GameObject blockPrefab;
    [SerializeField]
    private Transform blockRect;

    public List<Node> NodeList {  get; private set; }
    public Vector2Int BlockCount { get; private set; }

    private List<Block> blockList;

    private State state = State.Wait;   //현재 상태 (대기, 이동 병합, 후처리)

    private int currentScore;
    private int highScore;
    private float blockSize;    //블록 크기(맵 크기에 따라 블록 크기 설정)

    private void Awake()
    {
        //BlockCount = new Vector2Int(4, 4);
        int count = PlayerPrefs.GetInt("BlockCount");
        BlockCount = new Vector2Int(count, count);

        //블록 크기 설정 = (블록이 배치되는 보드 크기 - Padding - Spacing * (블록 개수 -1)) / 블록개수
        blockSize = (1080 - 85 - 25 * (BlockCount.x - 1)) / BlockCount.x;

        NodeList = nodeSpawner.SpawnNodes(this,BlockCount, blockSize);  //노드 블록 판 생성, 모든 노드의 정보를 NodeList에 저장

        blockList = new List<Block>();

        //점수 생성
        currentScore = 0;
        uiController.UpdateCurrentScore(currentScore);

        highScore = PlayerPrefs.GetInt("HighScore");
        uiController.UpdateHighScore(highScore);

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
        if (state == State.Wait)
        {
            Direction direction = touchController.UpdateTouch();
            if (direction != Direction.None)
            {
                AllBlockProcess(direction);
            }
        }
        else
        {
            UpdateState();
        }
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
            if (IsGameOver())
            {
                OnGameOver();
            }
        }
    }

    private void SpawnBlock(int x, int y)
    {
        if (NodeList[y * BlockCount.x + x].placedBlock != null) return;     //해당 위치에 블록이 배치되어 있으면 return

        GameObject clone = Instantiate(blockPrefab, blockRect);
        Block block = clone.GetComponent<Block>();
        Node node = NodeList[y* BlockCount.x + x];


        clone.GetComponent<RectTransform>().sizeDelta = new Vector2(blockSize, blockSize);
        clone.GetComponent<RectTransform>().localPosition = node.localPosition;     //생성한 블록의 위치를 노드의 위치와 동일하게 설정
        block.Setup();
        node.placedBlock = block;       //방금생성한 블록을 노드에 등록

        blockList.Add(block);   //리스트에 블록 정보 저장
    }

    private void AllBlockProcess(Direction direction)
    {
        if (direction == Direction.Right)
        {
            for (int y = 0; y < BlockCount.y; ++y)
            {
                for (int x = (BlockCount.x - 2); x >= 0 ; --x)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }
        }
        else if (direction == Direction.Left)
        {
            for(int y = 0; y < BlockCount.y; ++y)
            {
                for (int x = 1; x < BlockCount.x; ++x)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }
        }
        else if (direction == Direction.Down)
        {
            for (int y =(BlockCount.y - 2); y >= 0 ; --y)
            {
                for (int x = 0; x < BlockCount.x; ++x)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }
        }
        else if (direction == Direction.Up)
        {
            for (int y = 1; y < BlockCount.y; ++y)
            {
                for(int x = 0; x < BlockCount.x; ++x)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }   
        }

        //blockList에 있는 모든 블록을 검사해 Target이 있는 블록은
        //StartMove()로 Target까지 이동하도록 설정
        foreach(Block block in blockList)
        {
            if (block.Target != null)
            {
                state = State.Processing;
                block.StartMove();
            }
        }

        if (IsGameOver())
        {
            OnGameOver();
        }
    }

    private void BlockProcess(Node node, Direction direction)
    {
        //현재 노드에 블록이 없으면 종료
        if (node.placedBlock == null) return;

        //direction방행으로 이동 병합할 수 있는지 검사하기 위해 해당 방향에 있는 노드 검사
        Node neighborNode = node.FindTarget(node, direction);

        if (neighborNode != null)
        {
            //현재 노드와 이웃노드에 블록이 있고, 두 블록의 값이 같으면 Combine
            if (node.placedBlock != null && neighborNode.placedBlock != null)
            {
                if (node.placedBlock.Numeric == neighborNode.placedBlock.Numeric)
                {
                    Combine(node, neighborNode);
                }
            }

            //이동하려는 방향에 노드는 있지만 블록은 없으면 노드가 비어 있기 때문에 이동
            else if(neighborNode.placedBlock == null && neighborNode != null)
            {
                Move(node, neighborNode);
            }
        }
    }

    private void Move(Node from, Node to)
    {
        //from노드에 있는 블록의 목표 노드를 to노드로 설정
        from.placedBlock.MoveToNode(to);

        if (from.placedBlock != null)
        {
            //from노드에 있었던 블록을 to 노드에 소속된 것으로 설정
            to.placedBlock = from.placedBlock;
            //from노드의 블록정보를 비워줌
            from.placedBlock = null;
        }
    }


    private void Combine(Node from, Node to)
    {
        //from노드에 있는 블록이 to노드에 있는 블록에 병합되도록 설정
        from.placedBlock.CombineToNode(to);
        //from노드의 정보를 비움
        from.placedBlock = null;
        //to 노드의 combine = true로 설정해 병합되는 노드 설정
        to.combined =true;
    }
    private void UpdateState()
    {
        bool targetAllNull = true;

        //blockList에 있는 모든 블록을 검사해 Target이 null이 아닌 블록이 있으면 targetAllNull = false
        foreach (Block block in blockList)
        {
            if (block.Target != null)
            {
                targetAllNull = false;  
                break;
            }
        }

        //targetAllNull이 true이고 상태가 Processing일 때는 모든 블록의 이동 병합 처리가 완료된 직후
        if (targetAllNull && state == State.Processing)
        {
            //모든 블록을 탐색해 block.NeedDestroy가 true인 블록을 removeBlocks에 저장
            List<Block> removeBlocks = new List<Block>();
            foreach (Block block in blockList)
            {
                if (block.NeedDestroy)
                {
                    removeBlocks.Add(block);
                }
            }

            //removeBlocks의 모든 블록을 blockList에서 제외하고 블록 삭제
            removeBlocks.ForEach(x =>
            {
                currentScore += x.Numeric * 2;  //병합으로 삭제되는 블록의 숫자 X2
                blockList.Remove(x);
                Destroy(x.gameObject);
            });
            state = State.End;
        }

        if (state == State.End)
        {
            state = State.Wait;

            SpawnBlockToRandomNode();
            //모든 블록의 이동 병합이 종료되면 모든 노드의 combined를 false로
            NodeList.ForEach(x => x.combined = false);

            uiController.UpdateCurrentScore(currentScore);
        }
    }

    private bool IsGameOver()
    {
        foreach (Node node in NodeList)
        {
            //현재 노드에 블록이 없으면 게임 진행 가능
            if (node.placedBlock == null) return false;

            //각 노드의 이웃 노드 개수만큼 반복
            for (int i = 0; i < node.NeighborNodes.Length; ++i)
            {
                //이웃 노드가 없으면 건너뜀(바깥쪽 라인)
                if (node.NeighborNodes[i] == null ) continue;

                //이웃 노드 정보를 가져옴
                Vector2Int point = node.NeighborNodes[i].Value;
                Node neighborNode = NodeList[point.y * BlockCount.x + point.x];

                //현재 노드와 이웃 노드에 블록이 있고
                if (node.placedBlock != null && neighborNode.placedBlock != null)
                {
                    //두 노드에 있는 블록의 숫자가 같으면 게임 진행 가능
                    if (node.placedBlock.Numeric == neighborNode.placedBlock.Numeric)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void OnGameOver()
    {
        uiController.OnGameOver();

        if (currentScore >= highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
        }
    }
}
