using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace TanksMP
{
    public class PlayerMovement : MonoBehaviourPunCallbacks,IPunObservable
    {
        private PhotonView PV;
        private CharacterController myCC;

        /// <summary>
        /// Current turret rotation and shooting direction.
        /// </summary>
        [HideInInspector]
        public short turretRotation;

        /// <summary>
        /// Turret to rotate with look direction.
        /// </summary>
        public Transform turret;

        /// <summary>
        /// Position to spawn new bullets at.
        /// </summary>
        public Transform shotPos;

        /// <summary>
        /// Array of available bullets for shooting.
        /// </summary>
        public GameObject[] bullets;


        /// <summary>
        /// Delay between shots.
        /// </summary>
        public float fireRate = 0.75f;

        /// <summary>
        /// Reference to the camera following component.
        /// </summary>
        [HideInInspector]
        public FollowTarget camFollow;

        //timestamp when next shot should happen
        private float nextFire;

        //reference to this rigidbody
       // #pragma warning disable 0649
        private Rigidbody rb;
        //#pragma warning restore 0649

        public float moveSpeed;
        //public float rotationSpeed;

        // Start is called before the first frame update
        void Start()
        {
            PV = GetComponent<PhotonView>();
            myCC = GetComponent<CharacterController>();


            if (PV.IsMine)
            {
                //get components and set camera target
                rb = GetComponent<Rigidbody>();
                camFollow = Camera.main.GetComponent<FollowTarget>();
                camFollow.target = turret;
            }
        }

      
        void Update()
        {
            //if (PV.IsMine)
            //{
            //    Move();
            //    BasicRotation();
            //}

           

            if (PV.IsMine)
            {
                //movement variables
                Vector2 moveDir;
                Vector2 turnDir;

                //reset moving input when no arrow keys are pressed down
                if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
                {
                    moveDir.x = 0;
                    moveDir.y = 0;
                }
                else
                {
                    //read out moving directions and calculate force
                    moveDir.x = Input.GetAxis("Horizontal");
                    moveDir.y = Input.GetAxis("Vertical");
                    Move(moveDir);
                   // Move();
                }

                //cast a ray on a plane at the mouse position for detecting where to shoot 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane plane = new Plane(Vector3.up, Vector3.up);
                float distance = 0f;
                Vector3 hitPos = Vector3.zero;
                //the hit position determines the mouse position in the scene
                if (plane.Raycast(ray, out distance))
                {
                    hitPos = ray.GetPoint(distance) - transform.position;
                }

                //we've converted the mouse position to a direction
                turnDir = new Vector2(hitPos.x, hitPos.z);

                //rotate turret to look at the mouse direction
                RotateTurret(new Vector2(hitPos.x, hitPos.z));

                //shoot bullet on left mouse click
                // if (Input.GetButton("Fire1"))
                //    Shoot();
            }

            //if (!PV.IsMine)
            //{
            //    //keep turret rotation updated for all clients
            //    OnTurretRotation();
            //    return;
            //}

        }


        //this method gets called multiple times per second, at least 10 times or more
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //here we send the turret rotation angle to other clients
                stream.SendNext(turretRotation);
            }
            else
            {
                //here we receive the turret rotation angle from others and apply it
                this.turretRotation = (short)stream.ReceiveNext();
                OnTurretRotation();
            }
        }


        void OnTurretRotation()
        {
            //we don't need to check for local ownership when setting the turretRotation,
            //because OnPhotonSerializeView PhotonStream.isWriting == true only applies to the owner
            turret.rotation = Quaternion.Euler(0, turretRotation, 0);
        }



        //rotates turret to the direction passed in
        void RotateTurret(Vector2 direction = default(Vector2))
        {
            //don't rotate without values
            if (direction == Vector2.zero)
                return;

            //get rotation value as angle out of the direction we received
            turretRotation = (short)Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)).eulerAngles.y;
            OnTurretRotation();
        }


        //moves rigidbody in the direction passed in
        void Move(Vector2 direction = default(Vector2))
        {
            //if direction is not zero, rotate player in the moving direction relative to camera
            if (direction != Vector2.zero)
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y))
                 * Quaternion.Euler(0, camFollow.camTransform.eulerAngles.y, 0);



            //create movement vector based on current rotation and speed
            Vector3 movementDir = transform.forward * moveSpeed * Time.deltaTime;
            //apply vector to rigidbody position
            rb.MovePosition(rb.position + movementDir);
        }



        void Move()
        {
            if (Input.GetKey(KeyCode.W))
            {
                myCC.Move(transform.forward * Time.deltaTime * moveSpeed);
            }
            if (Input.GetKey(KeyCode.S))
            {
                myCC.Move(-transform.forward * Time.deltaTime * moveSpeed);
            }
            if (Input.GetKey(KeyCode.A))
            {
                myCC.Move(-transform.right * Time.deltaTime * moveSpeed);
            }
            if (Input.GetKey(KeyCode.D))
            {
                myCC.Move(transform.right * Time.deltaTime * moveSpeed);
            }
        }


        void BasicRotation()
        {
            float rotationSpeed = 20;
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;
            transform.Rotate(new Vector3(0, mouseX, 0));
        }



    }
}