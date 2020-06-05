using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public GameObject pathPrefab;
    public GameObject followedLightPrefab;
    public float speed;
    private int count;
    // public Text countText;
    private bool isGround;
    private float jumpSpeed = 100f;

    public Vector3 playerPosition;

    private float hitPoints;
    private float damage = 0.01f;
    private float generateDelay = 0.5f;
    private int frameCount = 0;
    private float lifetime = 5.0f;
    public float timer = 0;
    public Vector3 playerVector;

    [Command(channel = 1)]
    void CmdGeneratePath(Vector3 playerPosition, Vector3 playerVector)
    {
        GameObject path = (GameObject)Instantiate(
            pathPrefab,
            //transform.position,
            playerPosition - playerVector * 1.15f,
            Quaternion.identity
            );
        Destroy(path, lifetime);

        NetworkServer.Spawn(path);
    }

    [Command]
    void CmdCastPropID(NetworkInstanceId netId) {
        ClientDeleteProp(netId);
    }

    [Client]
    void ClientDeleteProp(NetworkInstanceId netId) {
        Destroy(ClientScene.FindLocalObject(netId), 0.0f);
    }

    void Startpickup(GameObject obj, float proptime)
    {
        StartCoroutine(Destroy(proptime));
        CmdCastPropID(obj.GetComponent<NetworkIdentity>().netId);
        Debug.Log("您获得了道具加成");
    }

    IEnumerator Destroy(float waittime)
    {
        yield return new WaitForSeconds(waittime);
        speed = speed / 4;
        damage = 0.01f;
        lifetime = 5.0f;
        Debug.Log("道具效果结束");
    }

    [Command]
    void CmdGenerateProps() {
        timer += Time.deltaTime;
        if(timer>=2)
        {
            timer = 0;
            //找到预制体
            GameObject propPrefab = Resources.Load<GameObject>("prop"+UnityEngine.Random.Range(0,3));

            Vector3 point = new Vector3(
                UnityEngine.Random.Range(-100, 100),
                1,
                UnityEngine.Random.Range(-100, 100)
                );

            // 生成预制体
            GameObject prop = Instantiate(
                propPrefab,
                point,
                propPrefab.transform.rotation) as GameObject;

            NetworkServer.Spawn(prop);
            //一分钟没人拾取销毁
            Destroy(prop,60f);
        }
    }


    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
        Camera.main.GetComponent<CameraController>().target = transform; //Fix camera on "me"
        GameObject followedLight = (GameObject)Instantiate(
            followedLightPrefab,
            //transform.position,
            playerPosition + new Vector3(0, 6, 0),
            Quaternion.identity
            );
        followedLight.GetComponent<Transform>().eulerAngles = new Vector3(90f, 0f, 0f);
        followedLight.GetComponent<Light>().intensity = 5f;
        followedLight.GetComponent<LightController>().target = transform;
    }

    // Start is called before the first frame 
    void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        count = 0;
        isGround = true;
        playerPosition = transform.position;
        hitPoints = 100.0f;
        // setCountText();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        frameCount += 1;
        //playerPosition = transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGround == true)
            {
                isGround = false;
                GetComponent<Rigidbody>().velocity += new Vector3(0, 5, 0);
                GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSpeed);
            }
        }
        if (
            System.Math.Abs(GetComponent<Rigidbody>().velocity.x) >= 0 ||
            System.Math.Abs(GetComponent<Rigidbody>().velocity.y) >= 0 ||
            System.Math.Abs(GetComponent<Rigidbody>().velocity.z) >= 0
            )
        {
            if (frameCount == 20) {
                CmdGeneratePath(playerPosition, playerVector);
                frameCount = 0;
            }
            playerPosition = transform.position;
            playerVector = GetComponent<Rigidbody>().velocity.normalized;
            CmdGenerateProps();
        }
        
    }

    public void EndGame()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        // UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        GetComponent<Rigidbody>().AddForce(movement * speed * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (other.gameObject.tag == "PickUp")
        {
            other.gameObject.SetActive(false);
            count++;
            // setCountText();
        }
        if (other.gameObject.tag == "GameOver")
        {
            EndGame();
        }
        if (other.gameObject.tag == "DoDamage")
        {
            hitPoints -= damage;
            Debug.Log(hitPoints);
        }
        if(other.gameObject.tag == "SpeedUp") {
            other.gameObject.SetActive(false);
            speed = 4 * speed; 
            Startpickup(other.gameObject, 5.0f);
        }
        if(other.gameObject.tag == "BeUnharmed") {
            other.gameObject.SetActive(false);
            damage = 0f;
            Startpickup(other.gameObject,3.0f);
        }
        if(other.gameObject.tag == "LongShadow") {
            other.gameObject.SetActive(false);
            lifetime = 10f;
            Startpickup(other.gameObject,20.0f);
        }
        if(other.gameObject.tag == "Meds") {
            other.gameObject.SetActive(false);
            hitPoints += 20.0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (collision.gameObject.tag == "Jumpable"
            && isGround == false)
        {
            isGround = true;
        }
    }

}
