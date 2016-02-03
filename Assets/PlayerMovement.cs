using UnityEngine;
using System.Collections;
using Prime31;


public class PlayerMovement : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;


	/*** MY CODE
	 * 
	 * 
	 */

	bool shoveling = false;
	bool onladder = false, climbing = false, onladdertop = false, onladdertoppiece = false;
	float gravitybackup;

	GameObject currentladder;

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "LadderTopPiece") {
			onladdertoppiece = true;
			Debug.Log ("On top");
			currentladder = other.gameObject;
		}

		if (other.tag == "Ladder") {
			onladder = true;
			currentladder = other.gameObject;
		} else if (other.tag == "Respawn") {
			Application.LoadLevel (0);
		} else if (other.tag == "MovingPlatform") {
			gameObject.transform.parent = other.gameObject.transform;
		} else if (other.tag == "LadderTop") {
			onladdertop = true;
			currentladder = other.gameObject;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.tag == "LadderTopPiece") {
			onladdertoppiece = false;
			Debug.Log ("Not on top");
		}

		if (other.tag == "Ladder") {
			if(climbing)
				_velocity.y = 0;
			onladder = false;
			climbing = false;
			gravity = gravitybackup;
			_animator.Play (Animator.StringToHash ("Idle"));
		} else if (other.tag == "MovingPlatform") {
			transform.parent = null;
		} else if (other.tag == "LadderTop") {
			onladdertop = false;
		}

		if(other.tag == "LadderTopPiece" && onladder == false){
			if(climbing)
				_velocity.y = 0;
			climbing = false;
			gravity = gravitybackup;
			_animator.Play (Animator.StringToHash ("Idle"));
		}
	}

	void Start(){
		gravitybackup = gravity;
	}

	/****
	*/


	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		//Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		//Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		if (_controller.isGrounded) {
			_velocity.y = 0;

			/*** MY CODE
			 * 
			 */
			shoveling = false;
			/****
			 */
		}

		if( Input.GetKey( KeyCode.RightArrow ) && !climbing)
		{
			normalizedHorizontalSpeed = 1;
			if( transform.localScale.x < 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

			if( _controller.isGrounded )
				_animator.Play( Animator.StringToHash( "Run" ) );
		}
		else if( Input.GetKey( KeyCode.LeftArrow ) && !climbing)
		{
			normalizedHorizontalSpeed = -1;
			if( transform.localScale.x > 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

			if( _controller.isGrounded )
				_animator.Play( Animator.StringToHash( "Run" ) );
		}
		else
		{
			normalizedHorizontalSpeed = 0;

			if (_controller.isGrounded) {
				_animator.Play (Animator.StringToHash ("Idle"));
				climbing = false;
				gravity = gravitybackup;
			}
		}


		// we can only jump whilst grounded
		if( _controller.isGrounded && Input.GetButtonDown("Jump") )
		{
			_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
			_animator.Play( Animator.StringToHash( "Jump" ) );
		}

		/*
		************** MY CODE
		 * 
		 * 
		 */

		if (climbing && transform.position.x != currentladder.transform.position.x) {
			transform.position = new Vector3 (currentladder.transform.position.x,
				transform.position.y);
		}

		if (climbing && Input.GetButtonDown ("Jump")) {
			climbing = false;
			gravity = gravitybackup;
			_animator.Play( Animator.StringToHash( "Jump2" ) );
			_animator.StartPlayback ();
		}

		if (_controller.isGrounded && onladdertop && Input.GetKeyDown (KeyCode.DownArrow)) {
			transform.position = new Vector3 (currentladder.transform.position.x,
				transform.position.y);
			_controller.ignoreOneWayPlatformsThisFrame = true;
			_controller.move (new Vector3 (0, -1));
			gravity = 0;
			climbing = true;
			_animator.Play( Animator.StringToHash( "Climbing" ) );
		}

		if (!_controller.isGrounded && _velocity.y < 0 && !shoveling && !climbing) {
				_animator.Play( Animator.StringToHash( "Jump2" ) );
			}

		if (Input.GetKeyDown (KeyCode.DownArrow) && !_controller.isGrounded && !climbing) {
				_animator.Play( Animator.StringToHash( "Shoveling" ) );
				shoveling = true;
			}

		if (Input.GetKey (KeyCode.UpArrow) && onladder && !climbing && !onladdertoppiece) {
				climbing = true;
				_velocity.y = Mathf.Sqrt( 2f * jumpHeight );
				_velocity.x = 0;
				_controller.move( new Vector3( currentladder.transform.position.x
					- transform.position.x, transform.position.y));
				gravity = 0;
				_animator.Play( Animator.StringToHash( "Climbing" ) );
		}else if (Input.GetKey (KeyCode.UpArrow) && onladder && !climbing && onladdertoppiece) {
			climbing = true;
			_velocity.x = 0;
			//_controller.move( new Vector3( currentladder.transform.position.x
			//	- transform.position.x, transform.position.y));
			transform.position = new Vector3 (currentladder.transform.position.x,
				transform.position.y);
			gravity = 0;
			_animator.Play( Animator.StringToHash( "Climbing" ) );
		}

		if (Input.GetKeyUp (KeyCode.UpArrow) && onladder && climbing) {
			_velocity.y = 0;
			_animator.StartPlayback ();
		}

		if (Input.GetKeyDown (KeyCode.UpArrow) && onladder && climbing) {
			_velocity.y = Mathf.Sqrt( 2f * jumpHeight );
			_animator.StopPlayback ();
		}

		if (Input.GetKeyUp (KeyCode.DownArrow) && onladder && climbing) {
			_velocity.y = 0;
			_animator.StartPlayback ();
		}

		if (Input.GetKey (KeyCode.DownArrow) && onladder && climbing) {
			_velocity.y = -1 * Mathf.Sqrt( 2f * jumpHeight );
			_animator.StopPlayback ();
		}

		/************
		*/


		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );

		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets uf jump down through one way platforms
		if( _controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) )
		{
			//_velocity.y *= 3f;
			//_controller.ignoreOneWayPlatformsThisFrame = true;
		}

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}

}

