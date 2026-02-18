using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Block : MonoBehaviour
{
    [SerializeField]
    private Color[] blockColors;
    [SerializeField]
    private Image imageBlock;
    [SerializeField]
    private TextMeshProUGUI textBlockNumeric;
    private int numeric; //블록이 가지는 숫자 (2, 4, 8...)
    private bool combined = false;

    public Node Target { get; private set; }    //이동 또는 병합하기 위해 이동하는 목표 Node

    //모든 블록의 이동 병합이 완료된 이후 한꺼번에 삭제하기 위해 바로 삭제하지 않고 삭제할 블록은 NeedDestroy = true로 설정
    public bool NeedDestroy { get; private set; } = false;

    public int Numeric
    {
        set
        {
            numeric = value;    //실제 숫자 값 변경
            textBlockNumeric.text = value.ToString();   //블록에 출력되는 숫자 설정
            imageBlock.color = blockColors[(int)Mathf.Log(value, 2) - 1];   //블록 색상 설정
        }
        get => numeric;
    }

    public void Setup()
    {
        //0~99까지 숫자 중 90 미만이 나오면 2, 90이상이면 4
        Numeric = Random.Range(0, 100) < 90 ? 2 : 4;

        StartCoroutine(OnScaleAnimation(Vector3.one * 0.5f, Vector3.one, 0.15f));
    }

    public void MoveToNode(Node to)
    {
        Target = to;
        combined = false;
    }

    public void CombineToNode(Node to)
    {
        Target = to;
        combined = true; 
    }

    public void StartMove()
    {
        float moveTime = 0.1f;
        StartCoroutine(OnLocalMoveAnimation(Target.localPosition, moveTime, OnAfterMove));
    }

    private void OnAfterMove()
    {
        if (Target != null)
        {
            if (combined) //해당 블록이 다른 블록에 병합되는 블록이면
            {
                Target.placedBlock.Numeric *= 2;
                Target.placedBlock.StartPunchScale(Vector3.one * 0.25f, 0.15f, OnAfterPunchScale);
                gameObject.SetActive(false);
            }
            else //해당 블록이 이동하는 블록이면
            {
                Target = null;  //목표 위치까지 이동을 완료했기 때문에 목표 해제
            }
        }
    }

    public void StartPunchScale(Vector3 punch, float time, UnityAction action = null)
    {
        StartCoroutine(OnPunchScale(punch, time, action));
    }

    private void OnAfterPunchScale()
    {
        Target = null;
        NeedDestroy = true;
    }

    IEnumerator OnPunchScale(Vector3 punch, float time, UnityAction action)
    {
        Vector3 current = Vector3.one;

        yield return StartCoroutine(OnScaleAnimation(current, current + punch, time * 0.5f));

        yield return StartCoroutine(OnScaleAnimation(current + punch, current, time * 0.5f));

        if (action != null) action.Invoke();
    }

    IEnumerator OnScaleAnimation(Vector3 start, Vector3 end, float time)
    {
        float current = 0;
        float percent = 0;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            transform.localScale = Vector3.Lerp(start, end, percent);

            yield return null;
        }
    }

    IEnumerator OnLocalMoveAnimation(Vector3 end, float time, UnityAction action)
    {
        float percent = 0;
        float current = 0;
        Vector3 start = GetComponent<RectTransform>().localPosition;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            transform.localPosition = Vector3.Lerp(start, end, percent);
            yield return null;
        }
        if (action != null) action.Invoke();
    }

}
