using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public const float SpeedIncrement= 0.15f;
    public const float InitialEnemySpeed = 4f;
    public const float InitialSpawnTime = 4f;
    public const int IntervalBetweenSpeedUps = 15;
    public const int InitialPointsForDestroyingEnemy = 10;
    public const int PointsIncrement = 2;
    public const float PowerUpsSize = 1.5f;

    private List<Enemy> _enemies;
    [SerializeField] private GameObject _enemyContainer;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _enemySpeed = InitialEnemySpeed;

    private bool _gameContinue = true;
    private GameObject[] _powerUpsPrefabs;
    [SerializeField] private GameObject _shieldPowerUpPrefab;
    [SerializeField] private float _spawnTime = InitialSpawnTime;
    [SerializeField] private GameObject _speedPowerUpPrefab;

    [SerializeField] private float _timeToSpeedUp = float.MaxValue;
    [SerializeField] private GameObject _tripleShotPowerUpPrefab;
    private UI_Manager _uiManager;

    public int pointsForDestroyingEnemy { get; private set; }

    private void Start()
    {
        _uiManager = FindObjectOfType<UI_Manager>();
        _powerUpsPrefabs = new[] {_shieldPowerUpPrefab, _speedPowerUpPrefab, _tripleShotPowerUpPrefab};
        _enemies = new List<Enemy>();
        pointsForDestroyingEnemy = InitialPointsForDestroyingEnemy;
    }

    public void Update()
    {
        if (Time.time < _timeToSpeedUp)
            return;

        _spawnTime -= _spawnTime < 1.5f ? 0f : 0.1f;
        _timeToSpeedUp = Time.time + IntervalBetweenSpeedUps;
        _enemySpeed += SpeedIncrement;
        _uiManager.UpdateFromSpawnManger(pointsForDestroyingEnemy, _enemySpeed, _spawnTime);
    }

    public void IncreasePointsForDestroyingEnemy()
    {
        pointsForDestroyingEnemy += PointsIncrement;
        _uiManager.UpdateFromSpawnManger(pointsForDestroyingEnemy, _enemySpeed, _spawnTime);
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnEnemy());
        StartCoroutine(SpawnPowerUps());
        _timeToSpeedUp = Time.time + IntervalBetweenSpeedUps;
    }

    public void PlayerDeath()
    {
        _gameContinue = false;
    }

    public void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }

    private IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(2.5f);
        while (_gameContinue)
        {
            if (_enemies.Count < 5)
            {
                var spawnPosition = new Vector3(Random.Range(Player.LeftBoundary, Player.RightBoundary),
                    Player.UpperBoundary + Enemy.EnemySize);
                var newEnemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity).GetComponent<Enemy>();
                newEnemy.SetInitialSpeed(_enemySpeed);
                newEnemy.transform.parent = _enemyContainer.transform;
                _enemies.Add(newEnemy);
            }

            yield return new WaitForSeconds(_spawnTime);
        }
    }

    private IEnumerator SpawnPowerUps()
    {
        yield return new WaitForSeconds(2.5f);
        while (_gameContinue)
        {
            var prefab = _powerUpsPrefabs[Random.Range(0, 3)];
            var position = new Vector3(Random.Range(Player.LeftBoundary, Player.RightBoundary),
                Player.UpperBoundary + PowerUpsSize);

            var powerUp = Instantiate(prefab, position, Quaternion.identity).GetComponent<PowerUp>();
            if (powerUp != null)
                powerUp.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(Random.Range(_spawnTime, 2 * _spawnTime));
        }
    }
}