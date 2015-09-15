using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using DG.Tweening;

public class CatMove : MonoBehaviour
{
    Animator anim;

    // speed는 고양이의 이동 속도, height는 고양이의 점프 높이
    float speed = 0.3f, height = 0.5f;

    public GameObject Sea; // 바다 객체 (이 객체의 좌표를 고양이의 좌표로 설정해 바다가 고양이 위치에 고정되게 한다)
    public GameObject WaterEffect; // 물보라 파티클
    public BlockManager Manager; // 하이어라키의 Blocks의 컴포넌트인 BlockManager

    public GameObject OverPanel, JoypadPanel, TitlePanel; // Canvas내의 조이패드 패널과 타이틀 텍스트 패널
    public AudioClip CatDie; // 고양이가 죽을때의 사운드 클립

    bool isDead = false; // 고양이가 죽었음을 나타냄

    ArrayList KeyArray = new ArrayList(); // 키보드 입력을 순차적으로 저장할 리스트


    void Awake()
    {
        if (Advertisement.isSupported)
        {
            Advertisement.Initialize("73464");
        }
        else
        {
            Debug.Log("Platform not supported");
        }
    }

    void ShowAds()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show(null, new ShowOptions
            {
                resultCallback = result =>
                {
                    Debug.Log(result.ToString());
                }
            });
        }
        else
        {
            Debug.Log("Advertisement is not ready");
        }
    }

	void Start()
	{
        Global.KillCount = PlayerPrefs.GetInt("KillCount");

        if (Time.time - Global.StartTime >= 30f)
        {
            Global.StartTime = Time.time;
            ShowAds();
        }
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly); // DOTween 플러그인 초기화 (어디서든 1회만 해주면 된다)

        Manager.SendMessage("GetScore");
        anim = GetComponentInChildren<Animator>();
	}

    void GameOver()
    {
        Global.KillCount++;
        PlayerPrefs.SetInt("KillCount", Global.KillCount);

        OverPanel.SetActive(true);
        OverPanel.transform.GetChild(0).GetComponent<Text>().text = string.Format("냥군 {0}마리째 사망!!", Global.KillCount);

        if (Global.KillCount < 10)
        {
            OverPanel.transform.GetChild(1).GetComponent<Text>().text = "냥이들이 당신을 피하기 시작합니다.";
        }
        else if (Global.KillCount < 50)
        {
            OverPanel.transform.GetChild(1).GetComponent<Text>().text = "익사한 냥이들이 해변으로 밀려오기 시작합니다.";
        }
        else if (Global.KillCount < 100)
        {
            OverPanel.transform.GetChild(1).GetComponent<Text>().text = "당신은 냥이를 키울 수 없는 운명이군요!";
        }
        else if (Global.KillCount < 500)
        {
            OverPanel.transform.GetChild(1).GetComponent<Text>().text = "냥이 대학살자의 칭호를 얻었습니다.";
        }
        else if (Global.KillCount < 1000)
        {
            OverPanel.transform.GetChild(1).GetComponent<Text>().text = "이제 바다에 물고기보다 냥이가 더 많습니다.";
        }
        else if (Global.KillCount < 5000)
        {
            OverPanel.transform.GetChild(1).GetComponent<Text>().text = "육지에도 더이상 냥이가 없다고 합니다.";
        }
        else
        {
            OverPanel.transform.GetChild(1).GetComponent<Text>().text = "냥이들이 물속에서 살 수 있게 진화하고 있습니다.";
        }
    }

    void Update()
	{
        if (TitlePanel.activeSelf) // 타이틀 화면이 보이는 상태이면
        {
            if (Input.GetMouseButtonUp(0)) // 아무키나 눌렀을때
            {
                Manager.SendMessage("StartGame"); // BlockManager로 StartGame을 호출해 최초 블럭을 생성시킨다.
                TitlePanel.SetActive(false); // 타이틀 화면을 보이지 않게 한다.
                JoypadPanel.SetActive(true); // 조이패드를 나타나게 한다.
            }
        }
        else
        {
            // 바다를 고양이의 X,Z 위치로 고정시킨다. Y축은 제외
            Sea.transform.position = new Vector3(transform.position.x, Sea.transform.position.y, transform.position.z);

            if (transform.position.y < 0f) // 고양이가 0f 이하로 내려가는 경우 (물속에 빠짐)
            {
                if (!WaterEffect.activeSelf) // 물보라 이펙트가 안보이는 상태이면
                {
                    GameOver();
                    audio.clip = CatDie; // 고양이가 죽을때 소리
                    audio.Play();
                    LeanTween.rotateAroundLocal(gameObject, Vector3.left, 90f, 0.5f); // 고양이를 0.5초간 앞으로 90도 회전시켜 머리부터 빠지게 한다.
                    WaterEffect.SetActive(true); // 물보라 이펙트를 활성화시킨다.
                    WaterEffect.transform.position = new Vector3(transform.position.x, -0.5f, transform.position.z); // 물보라 이펙트를 고양이 위치로 옮긴다.
                    Invoke("Restart", 3.0f); // 3초후에 게임을 다시 로딩한다.
                    isDead = true; // 고양이가 죽었기 때문에 키입력 등을 금지한다.
                }
            }
            // 윈도우 버전을 위한 키보드 입력에 따른 호출 (Canvas의 조이패드에서도 각각 이 함수들을 호출한다)
            if (Input.GetKeyDown(KeyCode.UpArrow)) Front();
            if (Input.GetKeyDown(KeyCode.DownArrow)) Back();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Left();
            if (Input.GetKeyDown(KeyCode.RightArrow)) Right();

            if (!isDead && Manager.CatLandedBlock != null) // 고양이가 살아있고 어딘가에 착지한 상태이면
            {
                if (KeyArray.Count > 0) // 키 입력이 1개라도 있는 경우
                {
                    KeyCode key = (KeyCode)KeyArray[0]; // 가장 먼저 입력했던 키 값을 가져온다.

                    // 키 값에 따라 실제로 고양이를 이동시키는 부분
                    if (key == KeyCode.UpArrow) FrontMove();
                    if (key == KeyCode.DownArrow) BackMove();
                    if (key == KeyCode.LeftArrow) LeftMove();
                    if (key == KeyCode.RightArrow) RightMove();

                    KeyArray.RemoveAt(0); // 처리한 키 값을 리스트에서 삭제한다.
                }
            }
        }
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
    }

    void Front()
    {
        KeyArray.Add(KeyCode.UpArrow);
    }

    void Back()
    {
        KeyArray.Add(KeyCode.DownArrow);
    }

    void Left()
    {
        KeyArray.Add(KeyCode.LeftArrow);
    }

    void Right()
    {
        KeyArray.Add(KeyCode.RightArrow);
    }

    void FrontMove()
    {
        audio.Play();
        anim.Play("ready"); // 고양이의 애니메이션을 멈추게 한다.

        transform.rotation = Quaternion.Euler(0f, 180f, 0f); // 고양이를 앞쪽을 보도록 회전시킨다.
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1f); // 이동할 좌표 설정
        transform.DOJump(pos, height, 1, speed); // DOTween을 이용해 해당 좌표로 height 만큼 높이로 점프하면서 이동

        Manager.LeaveLandedBlock(); // 착지해 있던 블럭에서 떠났음을 알린다.
        anim.SetTrigger("jump"); // 점프 애니메이션을 호출한다.
    }

    void BackMove()
    {
        audio.Play();
        anim.Play("ready");

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f);
        transform.DOJump(pos, height, 1, speed);

        Manager.LeaveLandedBlock();
        anim.SetTrigger("jump");
    }

    void LeftMove()
    {
        audio.Play();
        anim.Play("ready");

        transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        Vector3 pos = new Vector3(transform.position.x - 1f, transform.position.y, transform.position.z);
        transform.DOJump(pos, height, 1, speed);

        Manager.LeaveLandedBlock();
        anim.SetTrigger("jump");
    }

    void RightMove()
    {
        audio.Play();
        anim.Play("ready");

        transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        Vector3 pos = new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z);
        transform.DOJump(pos, height, 1, speed);

        Manager.LeaveLandedBlock();
        anim.SetTrigger("jump");
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "block") // 블럭에 착지한 경우
        {
            Manager.LandingTimer = Time.time; // 착지한 시간을 기록한다 (점수 계산을 위함)
            Manager.CatLandedBlock = col.gameObject; // 착지한 블럭을 BlockManager의 객체에 저장시킨다.
        }
    }

    void Restart()
    {
        Application.LoadLevel(0);
    }
}
