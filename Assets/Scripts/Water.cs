using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour
{
	void Start()
	{
        Invoke("StopWater", 1f);
	}

    void StopWater()
    {
        // 고양이가 물에 빠질때 나오는 물보라 파티클을 1초 후에 멈추게 한다.
        GetComponent<ParticleEmitter>().emit = false;
    }
}
