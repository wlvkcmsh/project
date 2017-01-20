/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController : MonoBehaviour
{
    /// <summary>
    /// The rate acceleration during movement.
    /// </summary>
    public float Acceleration = 0.1f;

    /// <summary>
    /// The rate of damping on movement.
    /// </summary>
    public float Damping = 0.3f;

    /// <summary>
    /// The rate of additional damping when moving sideways or backwards.
    /// </summary>
    public float BackAndSideDampen = 0.5f;

    /// <summary>
    /// The force applied to the character when jumping.
    /// </summary>
    public float JumpForce = 0.3f;

    /// <summary>
    /// The rate of rotation when using a gamepad.
    /// </summary>
    public float RotationAmount = 1.5f;

    /// <summary>
    /// The rate of rotation when using the keyboard.
    /// </summary>
    public float RotationRatchet = 45.0f;

    /// <summary>
    /// If true, reset the initial yaw of the player controller when the Hmd pose is recentered.
    /// </summary>
    public bool HmdResetsY = true;

    /// <summary>
    /// If true, tracking data from a child OVRCameraRig will update the direction of movement.
    /// </summary>
    public bool HmdRotatesY = true;

    /// <summary>
    /// Modifies the strength of gravity.
    /// </summary>
    public float GravityModifier = 0.379f;

    /// <summary>
    /// If true, each OVRPlayerController will use the player's physical height.
    /// </summary>
    public bool useProfileData = true;

    protected CharacterController Controller = null;
    protected OVRCameraRig CameraRig = null;
    

    private float MoveScale = 1.0f;
    private Vector3 MoveThrottle = Vector3.zero;
    private float FallSpeed = 0.0f;
    private OVRPose? InitialPose;
    private float InitialYRotation = 0.0f;
    private float MoveScaleMultiplier = 1.0f;
    private float RotationScaleMultiplier = 1.0f;
    private bool SkipMouseRotation = false;
    private bool HaltUpdateMovement = false;
    private bool prevHatLeft = false;
    private bool prevHatRight = false;
    private float SimulationRate = 60f;



    //이벤트 씬인지 체크

    public GameObject bulletHolePrefab;
    private RaycastHit hit;
    private Ray ray;
    private float fireculTime = 0.0f;

    private int state = 0;      //Player 상황 3인칭이냐 1인칭이냐

    void Start()
    {
        // Add eye-depth as a camera offset from the player controller
        var p = CameraRig.transform.localPosition;
        p.z = OVRManager.profile.eyeDepth;
        CameraRig.transform.localPosition = p;
    }

    void Awake()
    {
        Controller = gameObject.GetComponent<CharacterController>();

        if (Controller == null)
            Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

        // We use OVRCameraRig to set rotations to cameras,
        // and to be influenced by rotation
        OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();

        if (CameraRigs.Length == 0)
            Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
        else if (CameraRigs.Length > 1)
            Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
        else
            CameraRig = CameraRigs[0];

        InitialYRotation = transform.rotation.eulerAngles.y;
    }

    void OnEnable()
    {

        OVRManager.display.RecenteredPose += ResetOrientation;

        if (CameraRig != null)
        {
            CameraRig.UpdatedAnchors += UpdateTransform;
        }

    }

    void OnDisable()
    {

        OVRManager.display.RecenteredPose -= ResetOrientation;

        if (CameraRig != null)
        {
            CameraRig.UpdatedAnchors -= UpdateTransform;
        }
    }

    protected virtual void Update()
    {
        if (useProfileData)
        {
            if (InitialPose == null)
            {
                // Save the initial pose so it can be recovered if useProfileData
                // is turned off later.
                InitialPose = new OVRPose()
                {
                    position = CameraRig.transform.localPosition,
                    orientation = CameraRig.transform.localRotation
                };
            }

            var p = CameraRig.transform.localPosition;
            p.y = OVRManager.profile.eyeHeight - 0.5f * Controller.height
                + Controller.center.y;
            CameraRig.transform.localPosition = p;
        }
        else if (InitialPose != null)
        {
            // Return to the initial pose if useProfileData was turned off at runtime
            CameraRig.transform.localPosition = InitialPose.Value.position;
            CameraRig.transform.localRotation = InitialPose.Value.orientation;
            InitialPose = null;
        }

        UpdateMovement();

        Vector3 moveDirection = Vector3.zero;

        float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

        MoveThrottle.x /= motorDamp;
        MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
        MoveThrottle.z /= motorDamp;

        moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

        // Gravity
        if (Controller.isGrounded && FallSpeed <= 0)
            FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
        else
            FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);

        moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;

        // Offset correction for uneven ground
        float bumpUpOffset = 0.0f;

        if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
        {
            bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
            moveDirection -= bumpUpOffset * Vector3.up;
        }

        Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

        // Move contoller
        Controller.Move(moveDirection);

        Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

        if (predictedXZ != actualXZ)
            MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
    }

    public virtual void UpdateMovement()
    {
        if (HaltUpdateMovement)
            return;

        bool moveForward;
        bool moveLeft;
        bool moveRight;
        bool moveBack;
        //test Q버튼 Player상황 변환
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (state == 1)
                state = 0;
            else
                state = 1;
        }
            

        ////////////////움직임 부분 시작////////////////////
        if (state == 1)
        {
            fire();
            HmdRotatesY = true;
            moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        }
        else
        {
            
            moveForward = false;
            moveLeft = false;
            moveRight = false;
            moveBack = false;
        }


        bool dpad_move = false;

        if (OVRInput.Get(OVRInput.Button.DpadUp))
        {
            moveForward = true;
            dpad_move = true;

        }

        if (OVRInput.Get(OVRInput.Button.DpadDown))
        {
            moveBack = true;
            dpad_move = true;
        }

        MoveScale = 1.0f;

        if ((moveForward && moveLeft) || (moveForward && moveRight) ||
             (moveBack && moveLeft) || (moveBack && moveRight))
            MoveScale = 0.70710678f;

        // No positional movement if we are in the air
        if (!Controller.isGrounded)
            MoveScale = 0.0f;

        MoveScale *= SimulationRate * Time.deltaTime;

        // Compute this for key movement
        float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

        // Run!
        if (dpad_move || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            moveInfluence *= 2.0f;

        ///////////////로테이션 부분 시작////////////////////////
        if (state == 1)
        {
            Quaternion ort = transform.rotation;
            Vector3 ortEuler = ort.eulerAngles;
            ortEuler.z = ortEuler.x = 0f;
            ort = Quaternion.Euler(ortEuler);

            if (moveForward)
                MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
            if (moveBack)
                MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
            if (moveLeft)
                MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
            if (moveRight)
                MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);

            Vector3 euler = transform.rotation.eulerAngles;

            bool curHatLeft = OVRInput.Get(OVRInput.Button.PrimaryShoulder);

            if (curHatLeft && !prevHatLeft)
                euler.y -= RotationRatchet;

            prevHatLeft = curHatLeft;

            bool curHatRight = OVRInput.Get(OVRInput.Button.SecondaryShoulder);

            if (curHatRight && !prevHatRight)
                euler.y += RotationRatchet;

            prevHatRight = curHatRight;

            //Use keys to ratchet rotation
            if (Input.GetKeyDown(KeyCode.Q))
                euler.y -= RotationRatchet;

            if (Input.GetKeyDown(KeyCode.E))
                euler.y += RotationRatchet;

            float rotateInfluence = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;

#if !UNITY_ANDROID || UNITY_EDITOR
            if (!SkipMouseRotation)
                euler.y += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
#endif

            moveInfluence = SimulationRate * Time.deltaTime * Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

#if !UNITY_ANDROID // LeftTrigger not avail on Android game pad
            moveInfluence *= 1.0f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
#endif

            Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

            if (primaryAxis.y > 0.0f)
                MoveThrottle += ort * (primaryAxis.y * transform.lossyScale.z * moveInfluence * Vector3.forward);

            if (primaryAxis.y < 0.0f)
                MoveThrottle += ort * (Mathf.Abs(primaryAxis.y) * transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);

            if (primaryAxis.x < 0.0f)
                MoveThrottle += ort * (Mathf.Abs(primaryAxis.x) * transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);

            if (primaryAxis.x > 0.0f)
                MoveThrottle += ort * (primaryAxis.x * transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);

            Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            euler.y += secondaryAxis.x * rotateInfluence;

            transform.rotation = Quaternion.Euler(euler);
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
                        Debug.Log("No enemy Script!!");
                }

            }


        }
    }
    /// <summary>
    /// Invoked by OVRCameraRig's UpdatedAnchors callback. Allows the Hmd rotation to update the facing direction of the player.
    /// </summary>
    public void UpdateTransform(OVRCameraRig rig)
    {
        if(state == 1)
        {
            Transform root = CameraRig.trackingSpace;
            Transform centerEye = CameraRig.centerEyeAnchor;

            if (HmdRotatesY)
            {
                Vector3 prevPos = root.position;
                Quaternion prevRot = root.rotation;

                transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

                root.position = prevPos;
                root.rotation = prevRot;
            }
        }
       
    }

    /// <summary>
    /// Jump! Must be enabled manually.
    /// </summary>
    public bool Jump()
    {
        if (!Controller.isGrounded)
            return false;

        MoveThrottle += new Vector3(0, transform.lossyScale.y * JumpForce, 0);

        return true;
    }

    /// <summary>
    /// Stop this instance.
    /// </summary>
    public void Stop()
    {
        Controller.Move(Vector3.zero);
        MoveThrottle = Vector3.zero;
        FallSpeed = 0.0f;
    }

    /// <summary>
    /// Gets the move scale multiplier.
    /// </summary>
    /// <param name="moveScaleMultiplier">Move scale multiplier.</param>
    public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
    {
        moveScaleMultiplier = MoveScaleMultiplier;
    }

    /// <summary>
    /// Sets the move scale multiplier.
    /// </summary>
    /// <param name="moveScaleMultiplier">Move scale multiplier.</param>
    public void SetMoveScaleMultiplier(float moveScaleMultiplier)
    {
        MoveScaleMultiplier = moveScaleMultiplier;
    }

    /// <summary>
    /// Gets the rotation scale multiplier.
    /// </summary>
    /// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
    public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
    {
        rotationScaleMultiplier = RotationScaleMultiplier;
    }

    /// <summary>
    /// Sets the rotation scale multiplier.
    /// </summary>
    /// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
    public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
    {
        RotationScaleMultiplier = rotationScaleMultiplier;
    }

    /// <summary>
    /// Gets the allow mouse rotation.
    /// </summary>
    /// <param name="skipMouseRotation">Allow mouse rotation.</param>
    public void GetSkipMouseRotation(ref bool skipMouseRotation)
    {
        skipMouseRotation = SkipMouseRotation;
    }

    /// <summary>
    /// Sets the allow mouse rotation.
    /// </summary>
    /// <param name="skipMouseRotation">If set to <c>true</c> allow mouse rotation.</param>
    public void SetSkipMouseRotation(bool skipMouseRotation)
    {
        SkipMouseRotation = skipMouseRotation;
    }

    /// <summary>
    /// Gets the halt update movement.
    /// </summary>
    /// <param name="haltUpdateMovement">Halt update movement.</param>
    public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
    {
        haltUpdateMovement = HaltUpdateMovement;
    }

    /// <summary>
    /// Sets the halt update movement.
    /// </summary>
    /// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
    public void SetHaltUpdateMovement(bool haltUpdateMovement)
    {
        HaltUpdateMovement = haltUpdateMovement;
    }

    /// <summary>
    /// Resets the player look rotation when the device orientation is reset.
    /// </summary>
    public void ResetOrientation()
    {
        if (HmdResetsY)
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.y = InitialYRotation;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}

