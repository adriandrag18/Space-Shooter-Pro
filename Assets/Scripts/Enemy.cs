using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public const float EnemySize = 2f;

    private Animator _animator;
    private AudioSource _audioSource;

    [SerializeField] private bool _hitPlayer;
    [SerializeField] private bool _isDestroyed;

    private Player _player;
    private SpawnManager _spawnManager;

    [SerializeField] private float _speed;

    private float _initialSpeed;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.Log("Audio Source not found");

        _player = FindObjectOfType<Player>();
        if (_player == null)
            Debug.Log("Player not found");

        _spawnManager = FindObjectOfType<SpawnManager>();
        if (_spawnManager == null)
            Debug.Log("Spawn Manager not found");

        _animator = GetComponent<Animator>();
        if (_animator == null)
            Debug.Log("Animator not found");
    }

    private void Update()
    {
        Movement();
    }

    public void SetInitialSpeed(float speed)
    {
        _initialSpeed = speed;
        _speed = speed;
    }

    private void Movement()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < Player.LowerBoundary - EnemySize && !_isDestroyed)
            transform.position = new Vector3(Random.Range(Player.LeftBoundary, Player.RightBoundary),
                Player.UpperBoundary + EnemySize);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitPlayer)
            return;

        var laser = other.GetComponent<Laser>();
        if (laser != null)
        {
            _player.AddToScore(_spawnManager.pointsForDestroyingEnemy);
            Destroy(other.gameObject);
            _spawnManager.RemoveEnemy(this);
            _hitPlayer = true;
            Die();
        }

        if (_isDestroyed)
            return;

        var player = other.GetComponent<Player>();
        if (player == null)
            return;
        player.TakeDamage();
        _spawnManager.RemoveEnemy(this);
        Die();
    }

    private void Die()
    {
        _isDestroyed = true;
        StartCoroutine(SpeedDropDown());
        _animator.SetTrigger("OnEnemyDeath");
        _audioSource.Play();
        Destroy(GetComponent<BoxCollider2D>(), 0.5f);
        Destroy(gameObject, 2.4f);
    }

    private IEnumerator SpeedDropDown()
    {
        while (_speed > _initialSpeed / 4)
        {
            _speed *= 0.9f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}