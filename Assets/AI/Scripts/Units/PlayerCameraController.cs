using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Units
{
	public class PlayerCameraController : MonoBehaviour
	{
		private float _currentDelay;
		private Coroutine _coroutine;

		private PlayerControls _controls;
		private Transform _playerTransform;
		private Vector3 _offsetPosition;
		private Quaternion _offsetRotation;
		private Camera _camera;

		[SerializeField, Tooltip("Задержка до возвращения камеры в стартовое положение")]
		private float _delayReturnOffset = 3f;
		[SerializeField, Tooltip("Скорость вращения камеры")]
		private float _speedRotate = 5f;
		[SerializeField, Tooltip("Скорость возвращения камеры при движении")]
		private float _speedReturnInMove = 1.5f;
		[SerializeField, Tooltip("Скорость возвращения камеры при стоянии")]
		private float _speedReturnInIdle = 0.75f;

		private void Start()
		{
			_offsetRotation = transform.rotation;
			_offsetPosition = transform.position - _playerTransform.position;
			//_currentDelay = _delayReturnOffset;
		}

		private void Update()
		{
			//Перемещение камеры за игроком
			transform.position = _playerTransform.position + _offsetPosition;

			var delta = _controls.PlayerCamera.Looking.ReadValue<Vector2>();

			//Если : камера не двигается, то сокращаем таймер до возврата
			if(delta == Vector2.zero)
			{
				_currentDelay -= Time.deltaTime;
				//Запуск корутины по истечению времени
				if (_currentDelay <= 0f && _coroutine == null) _coroutine = StartCoroutine(ReturnCamera());

				return;
			}

			//Останавливаем корутину, если камера возвращается
			if(_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
			//Обновляем задержку на возвращение
			_currentDelay = _delayReturnOffset;

			var angle = new Vector3(-delta.y, delta.x, 0f) * _speedRotate * Time.deltaTime;

			//Вращаем камеру
			UpdateRotateWithCorrection(transform.rotation * Quaternion.Euler(angle));
		}

		private IEnumerator ReturnCamera()
		{
			//Задержка для оптимизации дублирований корутин
			_currentDelay = _delayReturnOffset;
			var currentRotate = transform.rotation;
			var it = 0f;
			
			while(it < 1f)
			{
				UpdateRotateWithCorrection(Quaternion.Lerp(currentRotate, _offsetRotation, it));
				it += Time.deltaTime * _speedReturnInMove;
				yield return null;
			}
			_coroutine = null;
		}

		//Обновляет поворот камеры без уклона по оси OZ
		private void UpdateRotateWithCorrection(in Quaternion rotate)
		{
			transform.localRotation = rotate;

			var corrective = transform.eulerAngles;
			corrective.z = 0f;
			transform.eulerAngles = corrective;
		}

		private void OnValidate()
		{
			_camera = Camera.main;
			_camera.transform.LookAt(transform);
			//_playerTransform = FindObjectOfType<PlayerComponent>().transform;//todo пробросить
		}

		private void Awake()
		{
			_controls = new PlayerControls();
		}

		private void OnEnable()
		{
			_controls.PlayerCamera.Enable();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void OnDisable()
		{
			_controls.PlayerCamera.Disable();
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void OnDestroy()
		{
			_controls.Dispose();
		}
	}
}
