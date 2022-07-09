using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
public class PlayerMovement : MonoBehaviour
{
	public bool isGrounded { get; private set; }

	private bool OnSlope()
	{
		return Physics.Raycast(base.transform.position, Vector3.down, out this.slopeHit, this.playerHeight / 2f * this.slopeForceRayLength) && this.slopeHit.normal != Vector3.up;
	}

	private bool OnSlope2()
	{
		return Physics.Raycast(base.transform.position, Vector3.down, out this.slopeHit, this.playerHeight / 2f * this.slopeForceRayLength) && this.slopeHit.normal != Vector3.up;
	}

	private void Start()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.rb.freezeRotation = true;
		this.currentHealth = 5;
		if (GameManager.NumberPlayers == 2)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.player2, new Vector3(this.orientation.position.x + 1f, this.orientation.position.y, this.orientation.position.z), this.orientation.rotation);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown("p"))
		{
			this.ToggleCharacter();
		}
		if (!this.minerModel)
		{
			this.bunnyGobj.transform.position = this.rb.transform.position;
			this.bunnyGobj.transform.rotation = this.rb.transform.rotation;
		}
		this.healthText.text = this.currentHealth.ToString();
		Ray ray = new Ray(base.transform.position, Vector3.down);
		RaycastHit raycastHit;
		this.isGrounded = Physics.Raycast(ray, out raycastHit, 2f);
		RaycastHit raycastHit2;
		if (Physics.Raycast(new Ray(base.transform.position, Vector3.down), out raycastHit2, 2f))
		{
			if (raycastHit2.transform.tag == "Slide")
			{
				this.onSlide = true;
			}
			else
			{
				this.onSlide = false;
			}
		}
		else
		{
			this.onSlide = false;
		}
		this.MyInput();
		this.ControlDrag();
		this.ControlSpeed();
		this.PlayerDead();
		this.Anime();
		if (Input.GetButtonDown("Jump") && this.isGrounded && this.isGrounded)
		{
			base.GetComponent<AudioSource>().PlayOneShot(this.jumpsound);
			this.canDoubleJump = true;
			this.rb.velocity = new Vector3(this.rb.velocity.x, 0f, this.rb.velocity.z);
			this.rb.AddForce(base.transform.up * this.jumpForce, ForceMode.Impulse);
		}
		if (Input.GetButtonDown("Jump") && !this.isGrounded && this.canDoubleJump)
		{
			base.GetComponent<AudioSource>().PlayOneShot(this.jumpsound);
			this.rb.velocity = new Vector3(this.rb.velocity.x, 0f, this.rb.velocity.z);
			this.rb.AddForce(base.transform.up * this.jumpForce, ForceMode.Impulse);
			this.canDoubleJump = false;
		}
	}

	private void MyInput()
	{
		this.horizontalMovement = Input.GetAxisRaw("Horizontal");
		this.verticalMovement = Input.GetAxisRaw("Vertical");
		this.moveDirection = this.orientation.forward * this.verticalMovement + this.orientation.right * this.horizontalMovement;
	}

	private void ControlSpeed()
	{
		this.moveSpeed = Mathf.Lerp(this.moveSpeed, this.walkSpeed, this.acceleration * Time.deltaTime);
	}

	private void ControlDrag()
	{
		if (this.isGrounded)
		{
			this.rb.drag = this.groundDrag;
			return;
		}
		this.rb.drag = this.airDrag;
	}

	private void FixedUpdate()
	{
		this.MovePlayer();
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(base.transform.position, Vector3.down), out raycastHit, 2f) && raycastHit.transform.tag == "Box")
		{
			this.isGrounded = true;
			this.rb.AddForce(this.moveDirection.normalized * this.moveSpeed * this.movementMultiplier, ForceMode.Force);
		}
		Transform transform = GameObject.FindWithTag("Water").transform;
		if (base.transform.position.y < transform.position.y)
		{
			this.currentHealth = 0;
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		if (this.canDamage && col.gameObject.tag == "Damage")
		{
			base.GetComponent<AudioSource>().PlayOneShot(this.damagesound);
			this.currentHealth--;
			this.canDamage = false;
			GameObject.Find("GunTip").GetComponent<ShotBullets>().tipotiro = 1;
			base.StartCoroutine(this.DamageDelay());
		}
		if (col.gameObject.tag == "Dead")
		{
			this.currentHealth = 0;
		}
	}

	private void OnTriggerEnter(Collider target)
	{
		if (target.tag == "Dead")
		{
			this.currentHealth = 0;
		}
	}

	private IEnumerator DamageDelay()
	{
		yield return new WaitForSeconds(1f);
		this.canDamage = true;
		yield break;
	}

	private void PlayerDead()
	{
		if (this.currentHealth <= 0)
		{
			this.dead = true;
			UnityEngine.Object.Destroy(base.gameObject);
			UnityEngine.Object.Instantiate<Rigidbody>(this.DeadRagDoll, this.orientation.position, this.orientation.rotation).velocity = base.transform.TransformDirection(new Vector3(0f, 100f, 100f));
			return;
		}
		this.dead = false;
	}

	private void Anime()
	{
		if (this.minerModel)
		{
			if (Input.GetAxisRaw("Vertical") == 0f && Input.GetAxisRaw("Horizontal") == 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("parado");
			}
			if (Input.GetAxisRaw("Vertical") > 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("frente");
			}
			if (Input.GetAxisRaw("Vertical") < 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("tras");
			}
			if (Input.GetAxisRaw("Vertical") == 0f && Input.GetAxisRaw("Horizontal") != 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("lateral");
			}
			if (!this.isGrounded && this.canDoubleJump && !this.onSlide && this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("jump");
			}
			if (!this.isGrounded && !this.canDoubleJump && this.rb.velocity.y > 0f && !this.onSlide && this.canDamage && GameObject.Find("GunPosition").GetComponent<GrapplingGun>().lr.positionCount == 0)
			{
				this.charAnime.GetComponent<Animation>().Play("jumpdouble1");
			}
			if (!this.isGrounded && !this.canDoubleJump && this.rb.velocity.y < 0f && !this.onSlide && this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("jumpdesce");
			}
			if (this.onSlide && this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("slide1");
			}
			if (!this.canDamage)
			{
				this.charAnime.GetComponent<Animation>().Play("dano");
				return;
			}
		}
		else
		{
			if (Input.GetAxisRaw("Vertical") == 0f && Input.GetAxisRaw("Horizontal") == 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("paradoP2");
			}
			if (Input.GetAxisRaw("Vertical") > 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("frenteP2");
			}
			if (Input.GetAxisRaw("Vertical") < 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("trasP2");
			}
			if (Input.GetAxisRaw("Vertical") == 0f && Input.GetAxisRaw("Horizontal") != 0f && this.isGrounded && !this.onSlide && this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("lateralP2");
			}
			if (!this.isGrounded && this.canDoubleJump && !this.onSlide && this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("jumpP2");
			}
			if (!this.isGrounded && !this.canDoubleJump && this.rb.velocity.y > 0f && !this.onSlide && this.canDamage && GameObject.Find("GunPosition").GetComponent<GrapplingGunP2>().lr.positionCount == 0)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("jumpdoubleP2");
			}
			if (!this.isGrounded && !this.canDoubleJump && this.rb.velocity.y < 0f && !this.onSlide && this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("jumpdesceP2");
			}
			if (this.onSlide && this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("slideP2");
			}
			if (!this.canDamage)
			{
				this.bunnyGobj.GetComponent<Animation>().Play("danoP2");
			}
		}
	}

	public PlayerMovement()
	{
		this.slopeForceRayLength = 1.5f;
	}

	[CompilerGenerated]
	private void MovePlayer()
	{
		if (this.isGrounded && !this.OnSlope() && !this.onSlide)
		{
			this.rb.AddForce(this.moveDirection.normalized * this.moveSpeed * this.movementMultiplier, ForceMode.Acceleration);
		}
		else if (this.isGrounded && this.OnSlope() && !this.onSlide)
		{
			this.rb.AddForce(this.slopeMoveDirection.normalized * this.moveSpeed * this.movementMultiplier, ForceMode.Acceleration);
			this.rb.AddForce(Vector3.up.normalized * this.moveSpeed * 3f, ForceMode.Acceleration);
		}
		else if (!this.isGrounded && !this.onSlide)
		{
			this.rb.AddForce(this.moveDirection.normalized * this.moveSpeed * this.movementMultiplier * this.airMultiplier / 2f, ForceMode.Acceleration);
		}
		if (this.onSlide)
		{
			this.rb.AddRelativeForce(Vector3.forward * this.moveSpeed, ForceMode.Force);
			this.rb.AddRelativeForce(Vector3.forward * this.moveSpeed * 3.3f, ForceMode.Acceleration);
			this.rb.AddForce(Vector3.down * this.moveSpeed * 13f, ForceMode.Acceleration);
			UnityEngine.Object.Instantiate<Rigidbody>(this.smokefoot, new Vector3(this.orientation.position.x, this.orientation.position.y - 1f, this.orientation.position.z), this.orientation.rotation);
		}
		new Ray(base.transform.position, Vector3.down);
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, (float)this.GroundObject) && !this.isGrounded && !this.onSlide)
		{
			this.rb.AddForce(this.moveDirection.normalized * 3.2f, ForceMode.Acceleration);
			this.rb.AddForce(Vector3.down * 2f, ForceMode.Acceleration);
		}
	}

	private void ToggleCharacter()
	{
		if (!this.minerModel)
		{
			this.charAnime.SetActive(true);
			this.minerModel = true;
			UnityEngine.Object.Destroy(this.bunnyGobj);
			return;
		}
		this.charAnime.SetActive(false);
		this.minerModel = false;
		this.bunnyGobj = UnityEngine.Object.Instantiate<GameObject>(this.player2.GetComponent<PlayerMovementP2>().charAnime, new Vector3(this.orientation.position.x + 1f, this.orientation.position.y, this.orientation.position.z), this.orientation.rotation);
	}

	private float playerHeight = 2f;

	public Transform orientation;

	[Header("Movement")]
	public float moveSpeed = 10f;

	public float slideSpeed = 100f;

	public float airMultiplier = 0.35f;

	public float movementMultiplier = 3f;

	[Header("Sprinting")]
	public float walkSpeed = 8.5f;

	[SerializeField]
	private float acceleration = 10f;

	[Header("Jumping")]
	public float jumpForce = 15f;

	public bool canDoubleJump = true;

	[Header("Keybinds")]
	[Header("Drag")]
	public float groundDrag = 1.2f;

	public float airDrag = 0.07f;

	private float horizontalMovement;

	private float verticalMovement;

	public bool onSlide;

	private Vector3 moveDirection;

	private Vector3 slopeMoveDirection;

	private Rigidbody rb;

	private RaycastHit slopeHit;

	public bool wallLeft;

	public bool wallRight;

	private RaycastHit leftWallHit;

	private RaycastHit rightWallHit;

	public int currentHealth;

	public Text healthText;

	public bool canDamage = true;

	public bool dead;

	public Rigidbody DeadRagDoll;

	public GameObject charAnime;

	public Rigidbody smokefoot;

	public GameObject player2;

	public LayerMask GroundObject;

	public float slopeForceRayLength;

	public AudioClip jumpsound;

	public AudioClip damagesound;

	public float inputDeadZone;

	private bool minerModel = true;

	private GameObject bunnyGobj;
}
