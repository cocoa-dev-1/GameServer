using UnityEngine;
using System;

enum PlayerInputs
{
    W,
    A,
    S,
    D,
    Space
}

public class Player : MonoBehaviour
{
	public int id;
	public string username;
    public CharacterController controller;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;

    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int id, string username)
	{
		this.id = id;
		this.username = username;

		inputs = new bool[5];
	}

    private void FixedUpdate()
    {
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[(int)PlayerInputs.W])
        {
            _inputDirection.y += 1;
        }
        if (inputs[(int)PlayerInputs.A])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[(int)PlayerInputs.S])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[(int)PlayerInputs.D])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        //transform.position += moveDirection * moveSpeed;
        moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0;
            if (inputs[(int)PlayerInputs.Space])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        moveDirection.y = yVelocity;

        controller.Move(moveDirection);

        ClientSend.PlayerPosition(this);
        ClientSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
}

