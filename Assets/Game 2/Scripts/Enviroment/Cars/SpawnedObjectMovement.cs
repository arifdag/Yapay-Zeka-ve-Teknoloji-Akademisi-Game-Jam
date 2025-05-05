using UnityEngine;

public class SpawnedObjectMovement : MonoBehaviour
{
    private float _moveDuration;
    private float _speed;
    private float _elapsedTime = 0f;
    private bool _isInitialized = false;


    private Vector3 _initialPosition;

    public void Initialize(float duration, float speed)
    {
        _moveDuration = duration;
        _speed = speed;
        _elapsedTime = 0f;
        _isInitialized = true;

        _initialPosition = transform.position;
    }

    void Update()
    {
        if (!_isInitialized) return;

        if (_elapsedTime < _moveDuration)
        {
            //    Use Vector3.back (0, 0, -1) for negative world Z movement.
            //    Use Vector3.forward (0, 0, 1) for positive world Z movement.
            Vector3 worldZMovementDirection = Vector3.back; // Moving along negative World Z

            Vector3 movementThisFrame = worldZMovementDirection * _speed * Time.deltaTime;

            transform.Translate(movementThisFrame, Space.World);

            _elapsedTime += Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
            _isInitialized = false; // Prevent Update running after destroy called
        }
    }
}