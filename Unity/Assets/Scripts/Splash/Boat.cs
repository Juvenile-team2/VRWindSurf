using UnityEngine;

public class Boat : MonoBehaviour
{
    public Waves waterMesh;              // 波のスクリプトへの参照
    public ParticleSystem splashEffect;  // 水しぶきのパーティクルシステム

    private float previousHeight;        // 前フレームでの高さ
    private float currentHeight;         // 現在の高さ
    private bool wasDecreasing = false;  // 高さが減少していたかどうか

    [Header("Splash Settings")]
    public float minVelocityForSplash = 0.1f;  // スプラッシュが発生する最小速度
    public float splashCooldown = 0.5f;        // スプラッシュの最小間隔（秒）
    private float lastSplashTime;              // 最後にスプラッシュが発生した時間

    void Start()
    {
        if (waterMesh == null)
        {
            waterMesh = FindObjectOfType<Waves>();
        }

        // 初期高さを取得
        previousHeight = GetWaterHeight();
        currentHeight = previousHeight;
    }

    void Update()
    {
        previousHeight = currentHeight;
        currentHeight = GetWaterHeight();

        // ボートの位置を波の高さに合わせる
        Vector3 position = transform.position;
        position.y = currentHeight;
        transform.position = position;

        // 波の傾きに合わせてボートを回転
        UpdateBoatRotation();

        // 極小値の検出とスプラッシュの生成
        CheckForSplash();
    }

    private float GetWaterHeight()
    {
        return waterMesh.GetHeight(transform.position);
    }

    private void UpdateBoatRotation()
    {
        // 波の法線に基づいてボートを回転させる
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // 前後の傾き
        float frontHeight = waterMesh.GetHeight(transform.position + forward);
        float backHeight = waterMesh.GetHeight(transform.position - forward);
        float pitchAngle = Mathf.Atan2(frontHeight - backHeight, 2f) * Mathf.Rad2Deg;

        // 左右の傾き
        float rightHeight = waterMesh.GetHeight(transform.position + right);
        float leftHeight = waterMesh.GetHeight(transform.position - right);
        float rollAngle = Mathf.Atan2(rightHeight - leftHeight, 2f) * Mathf.Rad2Deg;

        // 回転を適用
        transform.rotation = Quaternion.Euler(-pitchAngle, transform.eulerAngles.y, -rollAngle);
    }

    private void CheckForSplash()
    {
        float velocity = (currentHeight - previousHeight) / Time.deltaTime;
        bool isDecreasing = velocity < 0;

        // 極小値の検出（下降から上昇に変わる瞬間）
        if (wasDecreasing && !isDecreasing)
        {
            // 速度チェックとクールダウンチェック
            if (Mathf.Abs(velocity) > minVelocityForSplash &&
                Time.time - lastSplashTime > splashCooldown)
            {
                CreateSplash();
                lastSplashTime = Time.time;
            }
        }

        wasDecreasing = isDecreasing;
    }

    private void CreateSplash()
    {
        if (splashEffect != null)
        {
            // パーティクルの位置をボートの底面に合わせる
            Vector3 splashPosition = transform.position;
            splashPosition.y = currentHeight;

            // パーティクルを再生
            splashEffect.transform.position = splashPosition;
            splashEffect.Play();
        }
    }
}