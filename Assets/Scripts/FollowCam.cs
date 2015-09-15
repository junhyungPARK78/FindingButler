using UnityEngine;
using System.Collections;

public class FollowCam : MonoBehaviour
{
	public float followSpeed = 20;
    public Transform TargetPlayer = null;
    Vector3 pos;

    void Start()
    {
        // 고양이와 카메라 사이의 거리를 미리 구해둔다.
        if (TargetPlayer != null) pos = transform.position - TargetPlayer.position;
    }

	void Update()
    {
        if (TargetPlayer != null)
        {
            // 카메라가 계속 고양이를 따라가도록 설정한다.
            transform.position = Vector3.MoveTowards(transform.position, TargetPlayer.position + pos, followSpeed * Time.deltaTime);
        }

        // 고양이가 물속에 빠져도 카메라는 2.0f 이하로 더이상 내려가지 않도록 고정시킨다.
        if (transform.position.y < 2.0f)
        {
            transform.position = new Vector3(transform.position.x, 2.0f, transform.position.z);
        }
    }
}
