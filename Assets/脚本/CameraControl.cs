using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    private Player _player;
    private Camera _camera;

    private Vector2 _viewMinpos = new Vector3(0,1);
    private Vector2 _viewCenterPos = new Vector3(0.5f,1);
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        _player.minX = _camera.ViewportToWorldPoint(_viewMinpos).x;
        float cneterX = _camera.ViewportToWorldPoint(_viewCenterPos).x;
        if (_player.transform.position.x > cneterX)
        {
            Vector3 pos = transform.position;
            pos.x = _player.transform.position.x;
            transform.position = pos;
        }
    }
}
