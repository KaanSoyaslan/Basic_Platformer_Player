using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using TMPro;
public class BasicPlatformer : MonoBehaviour
{
    //hareketler
    public bool IsGrounded = false;
    public float MoveSpeed;
    public float JumpForce;
    public Rigidbody2D rb;
    public Animator anim;
    public Transform groundCheckPoint;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public LayerMask MovingPlatLAYER;
    public bool isTouchingGround;



    public int JumpCount; //0 normal  //1takla

    //Týrmanma
    public static bool Pmove;
    public static bool ClimbEdge;
    public static bool ClimbEdge2;
    bool ClimbTimer = false;
    public float climbUpAMOUNT = 0f;
    public float climbSÝDEAMOUNT = 0f;
    public float climbLadderAMOUNT = 0f;


    //Merdiven sonuna gelmiþmi
    public static bool OnLadder = false;
    public static bool LadderDone = false;
    public GameObject midCollider;

    public static bool PlayerOnTheMovingPlat = false;

//Fps
    public TextMeshProUGUI FPStxt;
    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    //Öldükten sonra tekrar dirilme (reklam izleme ile vs olabilir)
    public GameObject God;
    public int isPlayerDEAD = 0;//0 alive  1 dead  2untouchable
    bool PlayerTransParanty = false;
    public float PlayerAlphaColor;
    public SpriteRenderer[] PlayerBodyParts;
    bool isGodCome = false;
    public GameObject rEVÝVEbtn;


    void Start()
    {
        Physics2D.IgnoreLayerCollision(7, 8); //player ile ClimbP arasýnda
                                              //dönme = true;

    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        frameCount++;
        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FPStxt.text = frameRate.ToString();

            time -= pollingTime;
            frameCount = 0;
        }



        if (isPlayerDEAD != 1) //player ölmediði sürece
        {
            if (OnLadder)
            {
                rb.gravityScale = 0f;
            }
            else if (!OnLadder && !anim.GetBool("isClimbing"))
            {
                rb.gravityScale = 7f;
            }

            if ((!anim.GetBool("isClimbing") && (!OnLadder || (OnLadder && isTouchingGround))) && isPlayerDEAD != 1)
            {
                var move1 = new Vector3(Input.GetAxis("Horizontal"), 0); //buttonlar için
                var move2 = new Vector3(CrossPlatformInputManager.GetAxis("Horizontal"), 0); //buttonlar için
                transform.position += move1 * MoveSpeed * Time.deltaTime;
                transform.position += move2 * MoveSpeed * Time.deltaTime;
            }

            if (((CrossPlatformInputManager.GetAxis("Horizontal") < 0f || Input.GetAxis("Horizontal") < 0f) && !anim.GetBool("isClimbing")) && isPlayerDEAD != 1) //saða sola dönme
            {
                var rotationVector = new Vector2(0, 0);
                transform.rotation = Quaternion.Euler(rotationVector);
            }
            if (((CrossPlatformInputManager.GetAxis("Horizontal") > 0f || Input.GetAxis("Horizontal") > 0f) && !anim.GetBool("isClimbing")) && isPlayerDEAD != 1)
            {
                var rotationVector = new Vector2(0, 180);
                transform.rotation = Quaternion.Euler(rotationVector);
            }
            isTouchingGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

            PlayerOnTheMovingPlat = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, MovingPlatLAYER);


            if (PlayerOnTheMovingPlat) //moving plat ground layer olmadýðýndan 
            {
                isTouchingGround = true;
            }

            if ((((CrossPlatformInputManager.GetButtonDown("Jump") || Input.GetButtonDown("Jump")) && JumpCount < 1 && !anim.GetBool("isClimbing") && !OnLadder)) && isPlayerDEAD != 1) //jump in phone
            {
                JumpCount++;
                rb.velocity = new Vector2(0, 0);
                rb.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);

            }
            if (isTouchingGround || OnLadder)
            {
                JumpCount = 0;
            }

            if (((CrossPlatformInputManager.GetButtonDown("Jump") || Input.GetButtonDown("Jump")) && JumpCount < 1 && !anim.GetBool("isClimbing") && OnLadder) && isPlayerDEAD != 1) //jump from ladder
            {
                StartCoroutine(JumpOnLadder());
                //JumpCount++;
                rb.velocity = new Vector2(0, 0);


                float a = gameObject.GetComponent<Transform>().eulerAngles.y;
                if (a > 90)
                {
                    var rotationVector = new Vector2(0, 0);
                    // transform.rotation = Quaternion.Euler(rotationVector);

                    rb.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
                    rb.AddForce(new Vector2(JumpForce / 15, 0), ForceMode2D.Impulse);

                }
                else
                {
                    var rotationVector = new Vector2(0, 180);
                    //transform.rotation = Quaternion.Euler(rotationVector);

                    rb.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
                    rb.AddForce(new Vector2(-JumpForce / 15, 0), ForceMode2D.Impulse);
                }

            }
            else if (anim.GetBool("isLadderStop") && OnLadder)
            {
                rb.velocity = new Vector2(0f, 0f);
            }



            if ((((CrossPlatformInputManager.GetAxis("Horizontal") != 0 || Input.GetAxis("Horizontal") != 0) && anim.GetBool("isPushing") == false &&
                rb.velocity.y < 0.001f && rb.velocity.y > -0.001f && anim.GetBool("isMelee") == false && anim.GetBool("isDoubleJump") == false)) && isPlayerDEAD != 1) //koþma
            {
                anim.SetBool("isRunning", true);

            }
            else
            {
                anim.SetBool("isRunning", false);

            }


            if (rb.velocity.y < 0.01f && rb.velocity.y > -0.01f || OnLadder)  //idle
            {
                anim.SetBool("isJump", false);
                anim.SetBool("isfall", false);
                anim.SetBool("isDoubleJump", false);

            }
            if (rb.velocity.y > 0.01f && !OnLadder)  //jump
            {
                if (JumpCount == 1)
                {
                    anim.SetBool("isDoubleJump", true);
                    anim.SetBool("isJump", false);
                }
                else
                {
                    anim.SetBool("isJump", true);
                    anim.SetBool("isDoubleJump", false);
                }
                anim.SetBool("isfall", false);

            }
            else if (rb.velocity.y < -0.01f && !OnLadder && !anim.GetBool("isClimbing")) //düþme
            {
                anim.SetBool("isJump", false);
                anim.SetBool("isfall", true);
                anim.SetBool("isDoubleJump", false);
            }

            if (ClimbEdge && !ClimbEdge2)  //köþeden týrmanma
            {
                anim.SetBool("isClimbing", true);
                rb.velocity = new Vector2(0f, 0f);
                rb.gravityScale = 0f;


                var moveUP = new Vector3(0, 1); //buttonlar için
                transform.position += moveUP * climbUpAMOUNT * Time.deltaTime;
            }
            else if (!ClimbEdge && ClimbEdge2)
            {
                if (!ClimbTimer)
                {
                    StartCoroutine(ClimbTime());
                }

                float a = gameObject.GetComponent<Transform>().eulerAngles.y;
                var moveSIDE = new Vector3(1, 0); //buttonlar için
                Debug.Log(a);
                if (a > 90)
                {
                    transform.position += moveSIDE * climbSÝDEAMOUNT * Time.deltaTime;

                }
                else
                {
                    transform.position -= moveSIDE * climbSÝDEAMOUNT * Time.deltaTime;
                }

            }

            //merdivensel 
            if (((CrossPlatformInputManager.GetButton("Up") || Input.GetKey(KeyCode.W)) && OnLadder) && isPlayerDEAD != 1)
            {
                var moveUP = new Vector3(0, 1); //buttonlar için
                transform.position += moveUP * climbLadderAMOUNT * Time.deltaTime;
            }

            if (((CrossPlatformInputManager.GetButton("Down") || Input.GetKey(KeyCode.S)) && OnLadder && !isTouchingGround) && isPlayerDEAD != 1)
            {
                var moveUP = new Vector3(0, 1); //buttonlar için
                transform.position -= moveUP * climbLadderAMOUNT * Time.deltaTime;
            }

            if (((((!CrossPlatformInputManager.GetButton("Up") && CrossPlatformInputManager.GetButton("Down")) ||
                (!CrossPlatformInputManager.GetButton("Down") && CrossPlatformInputManager.GetButton("Up"))) ||
                (!Input.GetKey(KeyCode.S) && (Input.GetKey(KeyCode.W)) || (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)))) && isPlayerDEAD != 1)
                && OnLadder && !isTouchingGround && !anim.GetBool("isClimbing") && !LadderDone)
            {
                anim.SetBool("isLadderStop", false);
                anim.SetBool("isLadder", true);

            }
            else if (OnLadder && !isTouchingGround && !anim.GetBool("isClimbing") || (!isTouchingGround && LadderDone && OnLadder)) //merdivende yere deðmiyor tuþada basmýyor
            {
                anim.SetBool("isLadderStop", true);
                anim.SetBool("isLadder", false);
            }
            else if (OnLadder && isTouchingGround)
            {
                anim.SetBool("isLadderStop", false);
                anim.SetBool("isLadder", false);

            }
            else if (!OnLadder || anim.GetBool("isClimbing"))
            {
                anim.SetBool("isLadderStop", false);
                anim.SetBool("isLadder", false);
            }
        }





        if (isPlayerDEAD == 1 && Input.GetKey(KeyCode.R) && !isGodCome) //öldük ve reviwe bastýk
        {
            StartCoroutine(GodComing());
        }
        if (isPlayerDEAD == 2) //canlanma evresi
        {
            for (int i = 0; i < PlayerBodyParts.Length; i++)
            {
                PlayerBodyParts[i].color = new Color(1, 1, 1, PlayerAlphaColor);
            }

            if (!PlayerTransParanty)
            {
                StartCoroutine(PlayerTranpanScailing());
            }
        }
        if (isPlayerDEAD == 1 && !isGodCome)
        {
            rEVÝVEbtn.SetActive(true);
        }
        else
        {
            rEVÝVEbtn.SetActive(false);
        }

    }
    public void ReviveBTN() //pc deki R tuþu
    {
        if (isPlayerDEAD == 1 && !isGodCome)
        {
            rEVÝVEbtn.SetActive(false);
            StartCoroutine(GodComing());
        }
    }
    IEnumerator GodComing()
    {
        isGodCome = true;
        Instantiate(God, new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + 10), Quaternion.identity);
        yield return new WaitForSeconds(5f); //0.5
        Revive();
        isGodCome = false;
    }
    public void PlayerNormalColor()
    {
        PlayerAlphaColor = 1f;
        for (int i = 0; i < PlayerBodyParts.Length; i++)
        {
            PlayerBodyParts[i].color = new Color(1, 1, 1, PlayerAlphaColor);
        }
    }
    public void Revive()
    {
        isPlayerDEAD = 2;
        anim.SetInteger("isDead", 0);
    }
    IEnumerator PlayerTranpanScailing()
    {
        PlayerTransParanty = true;
        Physics2D.IgnoreLayerCollision(7, 11, true);
        for (int i = 0; i < 8; i++)
        {
            PlayerAlphaColor = 0.5f;
            yield return new WaitForSeconds(0.25f); //0.5
            PlayerAlphaColor = 0.8f;
            yield return new WaitForSeconds(0.25f);
        }
        PlayerNormalColor();
        Physics2D.IgnoreLayerCollision(7, 11, false);
        isPlayerDEAD = 0;
        PlayerTransParanty = false;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (PlayerOnTheMovingPlat && collision.gameObject.tag == "MovingPlatform")  //player moving plata deðdiðinde
        {
            //   Debug.Log("Player deðdi");
            transform.SetParent(collision.transform);
        }

        if (collision.gameObject.tag == "Enemy")
        {
            rb.velocity = new Vector2(0, 0);

            StartCoroutine(DeadAnimTime());
            // anim.SetInteger("isDead", 2);
            Debug.Log("U dedad");
        }


    }
    IEnumerator DeadAnimTime()
    {
        anim.SetInteger("isDead", 1);
        anim.SetBool("isLadderStop", false);
        anim.SetBool("isLadder", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isfall", false);
        anim.SetBool("isDoubleJump", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isPushing", false);
        isPlayerDEAD = 1;
        yield return new WaitForSeconds(0.25f);
        anim.SetInteger("isDead", 2);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "kutu")
        {
            if (((CrossPlatformInputManager.GetAxis("Horizontal") != 0 || Input.GetAxis("Horizontal") != 0)) && isPlayerDEAD != 1)
            {
                anim.SetBool("isPushing", true);

            }
            else
            {
                anim.SetBool("isPushing", false);
            }

        }

        if (PlayerOnTheMovingPlat && collision.gameObject.tag == "MovingPlatform") //player moving platta beklerken
        {
            //Debug.Log("Player deðdi");
            transform.SetParent(collision.transform);
        }

        if (((CrossPlatformInputManager.GetButton("Up") || Input.GetKey(KeyCode.W)) && collision.gameObject.tag == "Ladder" && isTouchingGround) && isPlayerDEAD != 1) //merdiven týrmanabilir
        {
            Debug.Log("ortalandý");
            rb.velocity = new Vector2(0, 0);
            gameObject.transform.position = new Vector2(collision.gameObject.transform.position.x, gameObject.transform.position.y);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "kutu")
        {

            anim.SetBool("isPushing", false);

            anim.SetBool("isRunning", true);

        }

        if (!PlayerOnTheMovingPlat && collision.gameObject.tag == "MovingPlatform") //moving plattan çýkarken
        {

            transform.SetParent(null);
        }


    }
    IEnumerator ClimbTime()
    {
        ClimbTimer = true;
        yield return new WaitForSeconds(0.25f); //0.5
        ClimbEdge2 = false;
        ClimbEdge = false;
        rb.velocity = new Vector2(0, 0);
        rb.gravityScale = 7f; //yerçekimi yine aktif
        anim.SetBool("isClimbing", false);
        ClimbTimer = false;
    }
    IEnumerator JumpOnLadder()
    {
        midCollider.GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(0.3f);
        midCollider.GetComponent<Collider2D>().enabled = true;
    }

}