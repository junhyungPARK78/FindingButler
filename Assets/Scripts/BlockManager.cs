using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class BlockManager : MonoBehaviour
{
    public int MaxBlock = 10; // 고양이가 점프할 수 있는 블럭 개수, 난이도에 맞춰 개수를 조절한다.

    public GameObject[] Blocks; // Blocks[0]은 땅 블럭 프리펩, Blocks[1]은 돌 블럭 프리펩을 설정한다.
    public GameObject LastBlock; // 마지막에 생성된 블럭을 가리킨다. (다음 위치에 블럭이 생성되게 하기 위함)
    public GameObject Score; // Canvas에 생성될 스코어 이펙트 프리펩

    public Text TotalScore, HighScore; // 현재 스코어와 하이스코어를 나타낼 Canvas내의 Text 객체
    public GameObject CatLandedBlock = null; // 고양이가 착지한 블럭을 가리킨다.
    public Canvas canvas; // UI용 Canvas 객체를 가리킨다.

    int MyScore = 0; // 나의 스코어
    public float LandingTimer = 0f; // 점수 계산을 위해 블럭에 착지한 순간의 시간을 기록한다.


    void GetScore() // CatMove에서 호출해준다.
    {
        HighScore.text = PlayerPrefs.GetString("HighScore", "0"); // 디바이스에 저장해둔 하이스코어를 가져온다.
    }

    void StartGame()
    {
        // 1개의 블럭은 미리 만들어져 있으므로,
        // 나머지 9개의 블럭을 순차적으로 생성하게 한다.

        for (int i = 1; i < MaxBlock; i++)
        {
            Invoke("CreateNewBlock", i * 0.1f);
        }
    }

	void CreateNewBlock()
	{
        Vector3 pos = Vector3.zero;

        while (true)
        {
            int rnd = Random.Range(0, 100);

            if (rnd < 50) // 50%의 확률로 블럭을 마지막 블럭의 앞쪽에 생성한다.
            {
                // 마지막 블럭의 좌표에서 Z축으로 +1만큼 위치를 가져온다.
                // 이때 Y축을 1f로 잡아 블럭보다 위쪽을 설정한다.
                // 그래야만 Raycast로 아래쪽을 체크할 수 있기 때문이다.
                // 유니티의 Raycast는 물체 내부에서 시작하면 체크되지 않는다.
                pos = new Vector3(LastBlock.transform.localPosition.x, 1f, LastBlock.transform.localPosition.z + 1f);

                // 설정한 좌표에서 아래쪽으로 1.5f만큼의 거리에 블럭이 있는지 체크한다.
                // 이때 Z축으로 +2만큼 위치도 함께 체크한다.
                // 생성하려는 방향에 최소 2칸의 공백이 있어야만 블럭들이 겹치지 않고 무한루프에 빠지지 않는다.
                if (!Physics.Raycast(pos, Vector3.down, 1.5f) && !Physics.Raycast(new Vector3(pos.x, pos.y, pos.z + 1f), Vector3.down, 1.5f)) break;
            }
            else if (rnd < 70) // 20%의 확률로 블럭을 마지막 블럭의 오른쪽에 생성한다.
            {
                pos = new Vector3(LastBlock.transform.localPosition.x + 1f, 1f, LastBlock.transform.localPosition.z);
                if (!Physics.Raycast(pos, Vector3.down, 1.5f) && !Physics.Raycast(new Vector3(pos.x + 1f, pos.y, pos.z), Vector3.down, 1.5f)) break;
            }
            else if (rnd < 90) // 20%의 확률로 블럭을 마지막 블럭의 왼쪽에 생성한다.
            {
                pos = new Vector3(LastBlock.transform.localPosition.x - 1f, 1f, LastBlock.transform.localPosition.z);
                if (!Physics.Raycast(pos, Vector3.down, 1.5f) && !Physics.Raycast(new Vector3(pos.x - 1f, pos.y, pos.z), Vector3.down, 1.5f)) break;
            }
            else // 10%의 확률로 블럭을 마지막 블럭의 뒤쪽에 생성한다.
            {
                pos = new Vector3(LastBlock.transform.localPosition.x, 1f, LastBlock.transform.localPosition.z - 1f);
                if (!Physics.Raycast(pos, Vector3.down, 1.5f) && !Physics.Raycast(new Vector3(pos.x, pos.y, pos.z - 1f), Vector3.down, 1.5f)) break;
            }
        }
        int num = Random.Range(0, 100) > 0 ? 0 : 1; // 1%의 확률로 돌 블럭이 생성되고 99%의 확률로 땅 블럭이 생성된다.
        GameObject temp = Instantiate(Blocks[num]) as GameObject;
        temp.transform.localPosition = new Vector3(pos.x, 0f, pos.z);
        temp.transform.parent = transform;
        temp.name = "block";
        LastBlock = temp; // LastBlock은 항상 마지막에 생성된 블럭을 가리킨다.
	}

    void CreateScore(int score) // Canvas에 점수를 표시한다.
    {
        GameObject temp = Instantiate(Score) as GameObject;
        temp.transform.parent = canvas.transform;
        temp.transform.localScale = new Vector3(1f, 1f, 1f);

        if (score > 0) // 스코어가 0보다 큰 값이면
        {
            temp.GetComponent<Text>().text = "+" + score.ToString(); // 얻은 점수를 프리펩에 전달
            MyScore += score; // 내 점수에 합산
            TotalScore.text = MyScore.ToString(); // Canvas에 현재 토탈 점수를 표시

            if (MyScore > int.Parse(HighScore.text)) // 현재 토탈 점수가 하이스코어를 넘어섰으면
            {
                HighScore.text = MyScore.ToString(); // 하이스코어 설정
                PlayerPrefs.SetString("HighScore", HighScore.text); // 디바이스에 하이스코어 저장
            }
        }
        else // 스코어가 0보다 작은 값이면 MISS가 뜨게 한다.
        {
            temp.GetComponent<Text>().text = "MISS";
            temp.GetComponent<Text>().color = Color.red;
        }

        // 생성한 Score 프리펩을 Canvas 내의 좌표로 변환시켜 위치를 설정해준다.
        RectTransform rect = temp.GetComponent<RectTransform>();
        rect.anchoredPosition = WorldToCanvas(CatLandedBlock.transform.position);
        // 스코어를 위로 이동시켜 올라가는 효과를 준다.
        temp.transform.DOLocalMoveY(temp.transform.localPosition.y + 200f, 1.0f).SetEase(Ease.OutBack);
        // 스코어를 좌우로 랜덤하게 이동시켜 여러개의 점수를 볼 수 있도록 연출한다.
        temp.transform.DOLocalMoveX(temp.transform.localPosition.x + Random.Range(-100f, 100f), 1.0f);
        // DOTween 플러그인에는 알파값을 조절하는 함수가 없어 이 부분에만 LeanTween을 사용했다.
        LeanTween.textAlpha(rect, 0f, 1.0f); // 1초 동안 알파값이 0f가 되도록 서서히 사라지게 한다.

        Destroy(temp, 1.0f); // 1초 후에 스코어를 삭제한다.
    }

    public void LeaveLandedBlock() // 고양이가 착지했던 블럭에서 떠난 경우
    {
        // 현재 시간에서 착지했던 시간을 빼면 최대 0.5f가 나온다.
        // 이 값에 1000을 곱하면 최대 500을 얻을 수 있다.
        // 그러니까 0.3초 이내에 블럭에서 탈출해야 +1 이상의 점수를 얻을 수 있게 된다.
        // 0.3초가 넘어가면 (-)값이 되어 MISS가 뜨게 된다.

        int score = (int)(300 - (Time.time - LandingTimer) * 1000);
        if (CatLandedBlock.GetComponent<Block>().FallDelay < 0) score = 500; // 돌 블럭에서 떠난 경우 무조건 500점을 준다.
        CreateScore(score);

        CreateNewBlock(); // 새로운 블럭을 생성한다.
        CatLandedBlock.SendMessage("FallBlock"); // 착지했었던 블럭을 바다 밑으로 가라앉게 한다.
        CatLandedBlock = null; // 착지했던 블럭을 가리키는 객체를 초기화한다.
    }

    public Vector2 WorldToCanvas(Vector3 world_position) // 화면상의 월드 좌표를 Canvas내의 좌표로 변환해준다.
    {
        var viewport_position = Camera.main.WorldToViewportPoint(world_position);
        var canvas_rect = canvas.GetComponent<RectTransform>();

        return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                           (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
    }
}
