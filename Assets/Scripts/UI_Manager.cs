using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] private Text _gameOverText;
    [SerializeField] private Text _infoText;
    [SerializeField] private Image _livesDisplay;
    [SerializeField] private Sprite[] _liveSprites;
    private Player _player;
    [SerializeField] private Text _restartText;
    [SerializeField] private Text _scoreText;
    [SerializeField] private Image _shieldVisualization;
    [SerializeField] private Image _speedBoostVisualization;
    [SerializeField] private Image _tripleShotVisualization;
    [SerializeField] private float initialLength;
    private GameInfo _info;
    private bool _infoShown;

    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _livesDisplay.sprite = _liveSprites[Player.MaxNumberOfLives];
        _gameOverText.gameObject.SetActive(false);
        _restartText.gameObject.SetActive(false);

        initialLength = _speedBoostVisualization.transform.localScale.x;
        _shieldVisualization.gameObject.SetActive(false);
        _speedBoostVisualization.gameObject.SetActive(false);
        _tripleShotVisualization.gameObject.SetActive(false);

        _infoText.text = "";
        _info = new GameInfo
        {
            spawnTime = SpawnManager.InitialSpawnTime,
            pointsForDestroyingEnemy = SpawnManager.InitialPointsForDestroyingEnemy,
            durationPowerUp = Player.InitialDurationPowerUps,
            fireRate = Player.InitialFireRate,
            speedLaser = 10f,
            speedEnemy = 3f,
            speedPlayer = Player.InitialSpeed
        };
        _infoText.gameObject.SetActive(false);
        _infoShown = false;
    }

    private void Update()
    {
        _scoreText.text = "Score: " + _player?.Score;
        CheckPowerUps();
        UpdateInfoText();
        UpdatePowerUpsVisualization();

        if (!Input.GetKeyDown(KeyCode.P))
            return;
        _infoShown = !_infoShown;
        _infoText.gameObject.SetActive(_infoShown);
    }

    public void UpdateLives(int currentLives)
    {
        _livesDisplay.sprite = _liveSprites[currentLives];
    }


    public void UpdateShield(bool shieldActive)
    {
        if (!shieldActive)
        {
            _info.timeDeactivationShield = Time.time - 1;
            return;
        }

        _info.playerShieldActive = true;
        _info.timeDeactivationShield = Time.time + 2 * _info.durationPowerUp;
    }

    public void UpdateSpeedBoost()
    {
        _info.playerSpeedBoostActive = true;
        _info.timeDeactivationSpeedBoost = Time.time + _info.durationPowerUp;
    }

    public void UpdateTripleShot()
    {
        _info.playerTripleShotActive = true;
        _info.timeDeactivationTripleShot = Time.time + _info.durationPowerUp;
    }

    public void UpdateFromPlayer(float durationPowerUp, float fireRate, float speed)
    {
        _info.durationPowerUp = durationPowerUp;
        _info.speedPlayer = _info.playerSpeedBoostActive ? speed / 2 : speed;
        _info.fireRate = fireRate;
    }

    public void UpdateFromSpawnManger(int pointsForDestroyingEnemy, float enemySpeed, float spawnTime)
    {
        _info.pointsForDestroyingEnemy = pointsForDestroyingEnemy;
        _info.speedEnemy = enemySpeed;
        _info.spawnTime = spawnTime;
    }

    private void CheckPowerUps()
    {

        if (_info.timeDeactivationShield < Time.time)
        {
            _info.playerShieldActive = false;
            _shieldVisualization.gameObject.SetActive(false);
        }

        if (_info.timeDeactivationSpeedBoost < Time.time)
        {
            _info.playerSpeedBoostActive = false;
            _speedBoostVisualization.gameObject.SetActive(false);
        }

        if (!(_info.timeDeactivationTripleShot < Time.time)) 
            return;
        
        _info.playerTripleShotActive = false;
        _tripleShotVisualization.gameObject.SetActive(false);
    }

    private void UpdatePowerUpsVisualization()
    {
        if (_info.playerShieldActive)
        {
            _shieldVisualization.gameObject.SetActive(true);
            _shieldVisualization.fillAmount =
                initialLength * (_info.timeDeactivationShield - Time.time) / (2 * _info.durationPowerUp);
        }
        
        if (_info.playerSpeedBoostActive)
        {
            _speedBoostVisualization.gameObject.SetActive(true);
            _speedBoostVisualization.fillAmount = initialLength * (_info.timeDeactivationSpeedBoost - Time.time) / _info.durationPowerUp;
        }

        if (!_info.playerTripleShotActive) 
            return;

        _tripleShotVisualization.gameObject.SetActive(true);
        _tripleShotVisualization.fillAmount = initialLength * (_info.timeDeactivationTripleShot - Time.time) / _info.durationPowerUp;
    }

    private void UpdateInfoText()
    {
        if (_player != null)
            _info.speedLaser = 10f + Mathf.Floor(_player.Score / 100f);

        _infoText.text = "";

        if (_info.playerShieldActive)
            _infoText.text += $"Shield: {(_info.timeDeactivationShield - Time.time):##0.#}\n";

        if (_info.playerSpeedBoostActive)
            _infoText.text += $"SpeedBoost: {(_info.timeDeactivationSpeedBoost - Time.time):##0.#}\n";

        if (_info.playerTripleShotActive)
            _infoText.text += $"TripleShot: {(_info.timeDeactivationTripleShot - Time.time):##0.#}\n";

        _infoText.text += $"\nPlayer Speed: {(_info.playerSpeedBoostActive ? 2 *_info.speedPlayer : _info.speedPlayer):##0.##}\n";
        _infoText.text += $"Enemy Speed: {_info.speedEnemy:##0.##}\n";
        _infoText.text += $"Laser Speed: {_info.speedLaser:##0.##}\n\n";
        _infoText.text += $"Fire Rate: {_info.fireRate:##0.##}\n";
        _infoText.text += $"Spawn Time: {_info.spawnTime:##0.##}\n";
        _infoText.text += $"Duration PowerUp: {_info.durationPowerUp:##0.##}\n";
        _infoText.text += $"Points for enemy: {_info.pointsForDestroyingEnemy:##0.##}\n";
    }

    public void GameOver()
    {
        _gameOverText.gameObject.SetActive(true);
        StartCoroutine(GameOverFlickerRoutine());
        _restartText.gameObject.SetActive(true);
#if UNITY_ANDROID
        _restartText.text = "Tap to restart the game";
#endif
    }

    private IEnumerator GameOverFlickerRoutine()
    {
        while (true)
        {
            _gameOverText.text = "Game Over";
            yield return new WaitForSeconds(0.5f);
            _gameOverText.text = "";
            yield return new WaitForSeconds(0.5f);
        }
    }
}

internal struct GameInfo
{
    public float durationPowerUp;
    public float fireRate;
    public bool playerShieldActive;
    public bool playerSpeedBoostActive;
    public bool playerTripleShotActive;
    public int pointsForDestroyingEnemy;
    public float spawnTime;
    public float speedEnemy;
    public float speedLaser;
    public float speedPlayer;
    public float timeDeactivationShield;
    public float timeDeactivationSpeedBoost;
    public float timeDeactivationTripleShot;
}