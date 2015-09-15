using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Block : MonoBehaviour
{
    bool isCat = false;
    Vector3 resetPos;

    public float FallDelay = 0.5f;

    void Start()
    {
        // 돌 블럭은 FallDelay 값이 -1이므로 움직이지 않게 한다.
        // 땅 블럭은 FallDelay 값이 보통 0.5이므로 상하로 둥둥 떠있는 느낌이 들도록 반복 이동시킨다.
        if (FallDelay >= 0f) transform.DOLocalMoveY(-0.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "MyCat") // 블럭 위에 고양이가 착지한 경우
        {
            if (!isCat) // 충돌이 여려번 일어날 수 있기 때문에 bool 값으로 착지시 1회만 체크한다.
            {
                if (FallDelay >= 0f) // 땅 블럭인 경우에만
                {
                    Invoke("FallBlock", FallDelay); // FallDelay초 이후에 땅 블럭을 바다로 가라앉힌다.
                }
                isCat = true;
            }
        }
    }

    void FallBlock()
    {
        CancelInvoke("FallBlock"); // Invoke를 정지시킨다.
        transform.DOLocalMoveY(-3f, 0.5f); // DOTween 플러그인을 이용해 -3f까지 0.5초간 땅 블럭을 가라앉게 한다.
        Destroy(gameObject, 0.5f); // 0.5초 후에 땅 블럭을 삭제한다.
    }
}
