using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePanel : MonoBehaviour
{
    private RectTransform showPos;
    private Vector3 startPos;
    private bool isShowed;

    private void Awake()
    {
        showPos = transform.parent.Find("ShowPos").gameObject.GetComponent<RectTransform>();
        startPos = GetComponent<RectTransform>().anchoredPosition;
        isShowed = false;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    public void MoveToPos()
    {
        StartCoroutine(this.Move());
    }

    IEnumerator Move()
    {
        Debug.Log("Move Start");
        yield return null;
        Vector3 targetPos = Vector3.zero;
        if (!isShowed)
        {
            targetPos = showPos.anchoredPosition;
            isShowed = true;
        }
        else
        {
            targetPos = startPos;
            isShowed = false;
        }

        float dis = 0;
        while (true)
        {
            yield return null;
            Debug.Log("distance To Target");
            dis = Vector3.Distance(GetComponent<RectTransform>().anchoredPosition, targetPos);
            Debug.Log(dis);
            if (dis < 0.5)
            {
                break;
            }
            GetComponent<RectTransform>().anchoredPosition = Vector3.MoveTowards(GetComponent<RectTransform>().anchoredPosition, targetPos, 5.0f);
        }
    }



}
