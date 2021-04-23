using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private const float InitialAngularVelocity = 75;

    [SerializeField] private readonly float _angularVelocity = InitialAngularVelocity;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private SpawnManager _spawnManager;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        if (_gameManager == null)
            Debug.Log("Game Manager not found");

        _spawnManager = FindObjectOfType<SpawnManager>();
        if (_spawnManager == null)
            Debug.Log("Spawn Manager not found");
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * _angularVelocity * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var laser = other.GetComponent<Laser>();
        if (laser == null)
            return;
        _spawnManager.StartSpawning();
        _gameManager.Explosion(transform.position);
        Destroy(other.gameObject);
        Destroy(gameObject, 0.2f);
    }
}