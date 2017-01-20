using UnityEngine;
using System.Collections;

[System.Serializable] //inspector에 노출
public class Anim
{
    public AnimationClip idle;
    public AnimationClip runForward;
    public AnimationClip runBackward;
    public AnimationClip runRight;
    public AnimationClip runLeft;
}

public class PlayerCtrl : MonoBehaviour {

    private float h = 0.0f;
    private float v = 0.0f;

    //컴포넌트 변수에 할당
    private Transform tr;
    //이동속도 변수
    public float moveSpeed = 10.0f;

    //회전속도 변수
    public float rotSpeed = 100.0f;

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;
    //아래에 있는 3d모델의 애니메이션 컴포넌트 접근
    public Animation _animation;

    public int hp = 100; //플레이어 생명

    //델리게이트 및 이벤트 선언
    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;



    public GameObject bulletHolePrefab;
    private RaycastHit hit;
    private Ray ray;
    private float fireculTime = 0.0f;

    private int state = 0;      //Player 상황 3인칭이냐 1인칭이냐

                                // Use this for initialization
    void Start () {
        tr = GetComponent<Transform>();

        _animation = GetComponentInChildren<Animation>();

        _animation.clip = anim.idle;
        _animation.Play();
	}
	
	// Update is called once per frame
	void Update () {

        fire();
        //h = Input.GetAxis("Horizontal");
        //v = Input.GetAxis("Vertical");

        ////Debug.Log("H=" + h.ToString());
        ////Debug.Log("V=" + v.ToString());
        //Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        //tr.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);
        //tr.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X"));

        ////키보드 입력값에 따라 애니메이션 수행
        //if (v >= 0.1f) //전
        //{
        //    _animation.CrossFade(anim.runForward.name, 0.3f);
        //}
        //else if (v <= -0.1f) //후
        //{
        //    _animation.CrossFade(anim.runBackward.name, 0.3f);
        //}
        //else if (h >= 0.1f) //우
        //{
        //    _animation.CrossFade(anim.runRight.name, 0.3f);
        //}
        //else if (h <= -0.1f) //좌
        //{
        //    _animation.CrossFade(anim.runLeft.name, 0.3f);
        //}
        //else
        //{
        //    _animation.CrossFade(anim.idle.name, 0.3f);
        //}
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "PUNCH")
        {
            hp -= 10;
            Debug.Log("Player HP = " + hp.ToString());

            if (hp <= 0)
            {
                PlayerDie();
            }
        }
    }
    public void fire()
    {
        fireculTime += Time.deltaTime;

        if (Input.GetButton("Fire1") && fireculTime > 0.2f)
        {
            fireculTime = 0.0f;

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            // The method ScreenPointToRay needs to be called from a camera
            // Since we are using the MainCamera of our scene we can have access to it using the Camera.main
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out hit, 100))
            {

                // A collision was detected please deal with it

                // We need a variable to hold the position of the prefab
                // The point of contact with the model is given by the hit.point
                Vector3 bulletHolePosition = hit.point + hit.normal * 0.01f;

                // We need a variable to hold the rotation of the prefab
                // The new rotation will be a match between the quad vector forward axis and the hit normal
                Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);

                GameObject hole = (GameObject)GameObject.Instantiate(bulletHolePrefab, bulletHolePosition, bulletHoleRotation);
                if (hit.transform.tag.Equals("ENEMY"))
                {
                    enemy hitobjScript = hit.transform.GetComponent<enemy>();

                    if (null != hitobjScript)
                    {

                        hitobjScript.Hit(10);
                    }
                    else
                        Debug.Log("No enemy Script!!");
                }
                if (hit.transform.tag.Equals("TOWER"))
                {
                    towerAi hitobjScript = hit.transform.GetComponent<towerAi>();

                    if (null != hitobjScript)
                    {

                        hitobjScript.HitTower(10);
                    }
                    else
                        Debug.Log("No Tower Script!!");
                }
                if (hit.transform.tag.Equals("FUELTANK"))
                {
                    Fueltank hitobjScript = hit.transform.GetComponent<Fueltank>();

                    if (null != hitobjScript)
                    {

                        hitobjScript.HitFuel(10);
                    }
                    else
                        Debug.Log("No Fueltank Script!!");
                }

            }


        }
    }

    void PlayerDie()
    {
        Debug.Log("Player Die !!");

        //GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //foreach (GameObject monster in monsters)
        //{
        //    monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        //}

        OnPlayerDie();
    }
}
