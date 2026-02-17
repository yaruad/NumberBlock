using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Block : MonoBehaviour
{
    [SerializeField]
    private Color[] blockColors;
    [SerializeField]
    private Image imageBlock;
    [SerializeField]
    private TextMeshProUGUI textBlockNumeric;
    private int numeric; //블록이 가지는 숫자 (2, 4, 8...)

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

}
