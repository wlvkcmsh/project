using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))] //컴포넌트 삭제 방지
public class FireCtrl : MonoBehaviour
{
    public GameObject bullet; //총알 프리팹
    public Transform firePos; //총알 발사좌표

    public AudioClip fireSfx; //총알 발사 사운드
    private AudioSource source = null; //컴포넌트 저장

    //public MeshRenderer muzzleFlash; //muzzle flash 메쉬랜더 연결

    // Use this for initialization
    void Start()
    {
        source = GetComponent<AudioSource>();
        //muzzleFlash.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    void Fire()
    {
        CreateBullet();
        source.PlayOneShot(fireSfx, 0.9f); //사운드 발생
        //StartCoroutine(this.ShowMuzzleFlash());
    }

    void CreateBullet()
    {

        Instantiate(bullet, firePos.position, firePos.rotation);
    }

    //IEnumerator ShowMuzzleFlash()
    //{
    //   스케일 불규칙하게
    //   float scale = Random.Range(1.0f, 2.0f);
    //   muzzleFlash.transform.localScale = Vector3.one * scale;

    //   z축 기준으로 불규칙하게 회전시킴
    //   Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360));
    //   muzzleFlash.transform.localRotation = rot;

    //   muzzleFlash.enabled = true; //활성화

    //   yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));

    //   muzzleFlash.enabled = false; //비활성화
    //}
}
