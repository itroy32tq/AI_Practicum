using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zenject;

namespace AI.Units
{
    public class PlayerComponent : BaseUnit
    {
        private PlayerControls _controls;
		[SerializeField, Tooltip("Скорость перемещения камеры")]
		private float _moveSpeed = 10f;
		[SerializeField, Tooltip("Скорость зума камеры")]
		private float _zoomSpeed = 1f;
		[SerializeField, Tooltip("Скорость вращения камеры")]
		private float _speedRotate = 5f;
		private Camera _camera;

		private void Awake()
		{
			_controls = new PlayerControls();
		}

		private void Update()
		{
			OnMove(_controls.UI.Navigate.ReadValue<Vector2>().ConvertToMoveVector3());
			OnZoom(_controls.UI.ScrollWheel.ReadValue<Vector2>().ConvertToMoveVector3());
			OnRotation(_controls.PlayerCamera.Looking.ReadValue<Vector2>());
		}

		protected override void OnMove(Vector3 movement)
		{

			//Если : персонаж стоит на месте - ничего не вычисляем
			if (movement == Vector3.zero) return;

			//Движение персонажа
			transform.position += transform.TransformDirection(movement * Time.deltaTime* _moveSpeed);
		}
		protected void OnZoom(Vector3 zoom)
		{
            transform.Translate(zoom * Time.deltaTime * _zoomSpeed);
		}

		protected void OnRotation(Vector2 looking)
		{
			
			var angle = new Vector2(-looking.y, looking.x) * _speedRotate * Time.deltaTime;

			transform.localRotation = transform.rotation * Quaternion.Euler(angle);

			transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
		}

		protected override void OnValidate()
		{
			_camera = Camera.main;
			_camera.transform.LookAt(transform);
		}

		private void OnEnable()
		{
			_controls.UI.Enable();
			_controls.PlayerCamera.Enable();
		}

		private void OnDisable()
		{
			_controls.UI.Disable();
			_controls.PlayerCamera.Disable();
		}

		private void OnDestroy()
		{
			_controls.Dispose();
		}
	}
}