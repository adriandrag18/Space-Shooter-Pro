using UnityEngine;
#if UNITY_ANDROID
using UnityStandardAssets.CrossPlatformInput;
#endif

public class Player : MonoBehaviour
{
    public const float InitialSpeed = 5f;
    public const int MaxNumberOfLives = 3;
    public const float InitialDurationPowerUps = 5f;
    public const float SpeedIncrement = 0.5f;
    public const float InitialFireRate = 0.45f;

    public const float LowerBoundary = -4f;
    public const float UpperBoundary = 4f;
    public const float LeftBoundary = -8.3f;
    public const float RightBoundary = 8.3f;
    public const float playerWidth = 1f;

    [SerializeField] private float _fireRate = InitialFireRate;
    private AudioSource _audioSource;

    [SerializeField] private float _durationPowerUps = InitialDurationPowerUps;
    [SerializeField] private GameObject[] _fireFromDamage;
    private GameManager _gameManager;

    [SerializeField] private bool _isShieldActive;
    [SerializeField] private bool _isSpeedBoostActive;
    [SerializeField] private bool _isTripleShotActive;

    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private AudioClip _laserSound;
    [SerializeField] private int _lives = MaxNumberOfLives;

    private float _nextFire;
    [SerializeField] private AudioClip _powerUpsSound;
    [SerializeField] private GameObject _shield;
    private SpawnManager _spawnManager;
    [SerializeField] private float _speed = InitialSpeed;

    private int _targetScoreToUpTheSpeed = 100;
    private int _pointsBetweenTargets = 100;
    [SerializeField] private GameObject _tripleShotPrefab;
    private UI_Manager _uiManager;

    private float _timeToDeactivateShield;
    private float _timeToDeactivateSpeedBoost;
    private float _timeToDeactivateTripleShot;

    [SerializeField] public int Score { get; private set; }

    private void Start()
    {
        _shield.SetActive(false);
        transform.position = new Vector3(0, 0, 0);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.Log("Audio Source on Player not found");

        _spawnManager = FindObjectOfType<SpawnManager>().GetComponent<SpawnManager>();
        if (_spawnManager == null)
            Debug.Log("SpawnManager not found");

        _uiManager = FindObjectOfType<UI_Manager>();
        if (_uiManager == null)
            Debug.Log("UI Manager not found");
        _gameManager = FindObjectOfType<GameManager>();

        if (_uiManager == null)
            Debug.Log("Game Manager not found");
    }

    private void Update()
    {
        CalculateMovement();
        Firing();
        if (_isShieldActive && Time.time > _timeToDeactivateShield)
            DeactivateShield();

        if (_isSpeedBoostActive && Time.time > _timeToDeactivateSpeedBoost)
            DeactivateSpeedBoost();

        if (_isTripleShotActive && Time.time > _timeToDeactivateTripleShot)
            _isTripleShotActive = false;
    }

    private void CalculateMovement()
    {
#if UNITY_ANDROID
        var direction = new Vector3(CrossPlatformInputManager.GetAxis("Horizontal"),
            CrossPlatformInputManager.GetAxis("Vertical"));
#else
        var direction = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif
        transform.Translate(direction * Time.deltaTime * _speed);

        transform.position = new Vector3(transform.position.x,
            Mathf.Clamp(transform.position.y, LowerBoundary, UpperBoundary));

        if (transform.position.x > RightBoundary + playerWidth)
            transform.position = new Vector3(LeftBoundary - playerWidth, transform.position.y);
        else if (transform.position.x < LeftBoundary - playerWidth)
            transform.position = new Vector3(RightBoundary + playerWidth, transform.position.y);
    }

    private void Firing()
    {
#if UNITY_ANDROID
        // if (!CrossPlatformInputManager.GetButtonDown("FireButton") || Time.time < _nextFire)
        //     return;
        if (!Input.GetMouseButtonDown(0) || Time.time < _nextFire)
            return;
#else
        if (!Input.GetKeyDown(KeyCode.Space) || Time.time < _nextFire)
            return;
#endif
        _nextFire = Time.time + _fireRate;
        Instantiate(_isTripleShotActive ? _tripleShotPrefab : _laserPrefab, transform.position, Quaternion.identity);
        PlayLaserSound();
    }

    private void PlayLaserSound()
    {
        _audioSource.clip = _laserSound;
        _audioSource.Play();
    }

    public void TakeDamage()
    {
        if (_isShieldActive)
        {
            DeactivateShield();
            return;
        }

        _lives--;
        _uiManager.UpdateLives(_lives);

        if (_lives != 0)
        {
            DamageVisualization();
            return;
        }

        Die();
    }

    private void DamageVisualization()
    {
        var fire = _fireFromDamage[_lives == 2 ? 0 : 1];
        fire.SetActive(true);
        float x, y;
        switch (Random.Range(0, 6))
        {
            case 0:
                x = Random.Range(0.4f, 0.7f);
                y = Random.Range(-1f, 0.5f);
                break;
            case 1:
                x = Random.Range(-0.7f, -0.4f);
                y = Random.Range(-1f, 0.5f);
                break;
            case 2:
                x = Random.Range(0.7f, 1.5f);
                y = -2.5f;
                break;
            case 3:
                x = Random.Range(-1.5f, -0.7f);
                y = -2.5f;
                break;
            default:
                x = Random.Range(-0.7f, 0.7f);
                y = Random.Range(-2.2f, -1f);
                break;
        }

        fire.transform.localPosition += new Vector3(x, y);
    }

    public void AddToScore(int x)
    {
        Score += x;
        if (Score < _targetScoreToUpTheSpeed) 
            return;
        SpeedUpGame();
    }

    private void SpeedUpGame()
    {
        _pointsBetweenTargets += 50;
        _targetScoreToUpTheSpeed += _pointsBetweenTargets;
        _speed += (_isSpeedBoostActive ? 2: 1) * SpeedIncrement;
        _fireRate -= _fireRate <= 0.2f ? 0f : 0.01f;
        _durationPowerUps += 2 * SpeedIncrement;
        _spawnManager.IncreasePointsForDestroyingEnemy();
        _uiManager.UpdateFromPlayer(_durationPowerUps, _fireRate, _speed);
    }

    public void ActivateShield()
    {
        _timeToDeactivateShield = Time.time + 2 * _durationPowerUps;
        _isShieldActive = true;
        _shield.SetActive(true);
        PlayPowerUpsSound();
        _uiManager.UpdateShield(true);
    }

    private void DeactivateShield()
    {
        _isShieldActive = false;
        _shield.SetActive(false);
        _uiManager.UpdateShield(false);
    }

    public void ActivateSpeedBoost()
    {
        _timeToDeactivateSpeedBoost = Time.time + _durationPowerUps;
        if (_isSpeedBoostActive)
            return;
        _isSpeedBoostActive = true;
        _speed *= 2;
        PlayPowerUpsSound();
        _uiManager.UpdateSpeedBoost();
    }

    private void DeactivateSpeedBoost()
    {
        _isSpeedBoostActive = false;
        _speed /= 2;
    }

    public void ActivateTripleShot()
    {
        _timeToDeactivateTripleShot = Time.time + _durationPowerUps;
        _isTripleShotActive = true;
        PlayPowerUpsSound();
        _uiManager.UpdateTripleShot();
    }

    private void PlayPowerUpsSound()
    {
        _audioSource.clip = _powerUpsSound;
        _audioSource.Play();
    }

    private void Die()
    {
        _gameManager.Explosion(transform.position);
        _spawnManager.PlayerDeath();
        _uiManager.GameOver();
        _gameManager.GameIsOver();
        Destroy(gameObject, 0.2f);
    }
}