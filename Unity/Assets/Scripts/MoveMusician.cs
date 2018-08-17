using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMusician : MonoBehaviour
{
    [SerializeField] float _speed = 0.1f;

	void Update ()
    {
        transform.position += Vector3.up * _speed * Time.deltaTime;
	}
}
