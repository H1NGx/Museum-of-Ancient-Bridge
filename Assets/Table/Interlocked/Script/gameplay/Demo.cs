using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Demo : MonoBehaviour
{
    Color mouseOverColor = Color.blue;
    Color originalColor;

    [SerializeField] private float maxDragSpeed = 18f;
    [SerializeField] private float moveStepThreshold = 0.01f;

    private Rigidbody _rigidbody;
    private Camera _camera;
    private float _dragDepth;
    private Vector3 _dragOffset;
    private Vector3 _dragStartPosition;
    private bool _dragging;
    private bool _stepCounted;

    //音源AudioSource相当于播放器，而音效AudioClip相当于磁带
    public AudioSource music;
    public AudioClip drag;//这里给块添加拖动的音效

    void Start()
    {
       // originalColor = GetComponent<Renderer>().sharedMaterial.color;
    }

    void OnMouseOver()
    {
       // GetComponent<Renderer>().material.color = mouseOverColor;
    }

    void OnMouseExit()
    {
       // GetComponent<Renderer>().material.color = originalColor;
    }

    private void OnMouseUp()
    {
        FinishDrag(true);
    }

    private void FinishDrag(bool countStep)
    {
        bool wasDragging = _dragging;
        _dragging = false;

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;
        }

        if (music != null && music.isPlaying)
        {
            music.Stop();
        }

        if (countStep && wasDragging && (transform.position - _dragStartPosition).sqrMagnitude > moveStepThreshold * moveStepThreshold)
        {
            RecordStep();
         //   //print(gamemanager1.text.text);
        //    //print("set");
        }
    }

    IEnumerator OnMouseDown()
    {
        if (_rigidbody == null || Camera.main == null)
        {
            yield break;
        }

        _camera = Camera.main;
        _dragging = true;
        _stepCounted = false;
        _dragStartPosition = transform.position;
        _dragDepth = _camera.WorldToScreenPoint(transform.position).z;
        _dragOffset = transform.position - GetMouseWorldPoint();

        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        while (Input.GetMouseButton(0))
        {
            Vector3 target = GetMouseWorldPoint() + _dragOffset;
            Vector3 desiredVelocity = (target - _rigidbody.position) / Time.fixedDeltaTime;
            _rigidbody.velocity = Vector3.ClampMagnitude(desiredVelocity, maxDragSpeed);

            if (!_stepCounted && (target - _dragStartPosition).sqrMagnitude > moveStepThreshold * moveStepThreshold)
            {
                RecordStep();
            }

            if (music != null && drag != null && !music.isPlaying)
            {
                music.clip = drag;
                music.volume = 1.25f;
                music.Play();
            }
            //   //print("force");
            //transform.position = curPosition;
            yield return new WaitForFixedUpdate();
        }

        if (_dragging)
        {
            OnMouseUp();
        }
    }
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody != null)
        {
            _rigidbody.useGravity = false;
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        music = gameObject.AddComponent<AudioSource>();
        //设置不一开始就播放音效
        music.playOnAwake = false;
        //加载音效文件，我把跳跃的音频文件命名为jump
        drag = Resources.Load<AudioClip>("music/drag");
    }

    private void OnDisable()
    {
        FinishDrag(false);
    }

    private Vector3 GetMouseWorldPoint()
    {
        Vector3 mousePosOnScreen = Input.mousePosition;
        mousePosOnScreen.z = _dragDepth;
        return _camera.ScreenToWorldPoint(mousePosOnScreen);
    }

    private void RecordStep()
    {
        if (_stepCounted)
        {
            return;
        }

        _stepCounted = true;
        gamemanager1.steps = gamemanager1.steps + 1;
        gamemanager1.Setsteps();
    }

}
