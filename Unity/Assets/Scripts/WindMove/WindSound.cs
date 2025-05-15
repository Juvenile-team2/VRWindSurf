using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AdvancedSpeedBasedSoundSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardSpeed boardSpeedController;
    [SerializeField] private AudioClip soundForSpeedRange1;
    [SerializeField] private AudioClip soundForSpeedRange2;

    [Header("Settings")]
    [SerializeField] private float speedThreshold1 = 1f; // この値以上で soundForSpeedRange1
    [SerializeField] private float speedThreshold2 = 10f; // この値以上で soundForSpeedRange2
    [SerializeField] private bool loopSounds = true;

    private AudioSource audioSource;

    private enum CurrentPlayingState
    {
        None,
        Sound1,
        Sound2
    }
    private CurrentPlayingState currentState = CurrentPlayingState.None;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (boardSpeedController == null)
        {
            Debug.LogError("BoardSpeedControllerが設定されていません。");
            enabled = false;
            return;
        }

        // soundForSpeedRange1 と soundForSpeedRange2 は null でも許容（何も再生しない場合があるため）

        if (speedThreshold1 >= speedThreshold2)
        {
            Debug.LogError("speedThreshold1 は speedThreshold2 より小さく設定してください。");
            enabled = false;
            return;
        }

        // 初期状態は何も再生しない
        audioSource.Stop();
        currentState = CurrentPlayingState.None;
    }

    void Update()
    {
        if (boardSpeedController == null) return;

        float currentSpeed = boardSpeedController.GetBoardSpeed();
        CurrentPlayingState newState = DetermineTargetState(currentSpeed);

        if (newState != currentState)
        {
            currentState = newState;
            ApplyAudioState(currentState);
        }
    }

    private CurrentPlayingState DetermineTargetState(float speed)
    {
        if (speed >= speedThreshold2)
        {
            return CurrentPlayingState.Sound2;
        }
        else if (speed >= speedThreshold1)
        {
            return CurrentPlayingState.Sound1;
        }
        else
        {
            return CurrentPlayingState.None;
        }
    }

    private void ApplyAudioState(CurrentPlayingState targetState)
    {
        switch (targetState)
        {
            case CurrentPlayingState.None:
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                audioSource.clip = null;
                break;
            case CurrentPlayingState.Sound1:
                PlayClip(soundForSpeedRange1);
                break;
            case CurrentPlayingState.Sound2:
                PlayClip(soundForSpeedRange2);
                break;
        }
    }

    private void PlayClip(AudioClip clipToPlay)
    {
        if (audioSource != null)
        {
            if (clipToPlay != null)
            {
                // 状態が変わった時、または再生中でない場合に再生/切り替え
                if (audioSource.clip != clipToPlay || !audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.clip = clipToPlay;
                    audioSource.loop = loopSounds;
                    audioSource.Play();
                }
            }
            else // 再生すべきクリップがない場合
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                audioSource.clip = null;
            }
        }
    }

    void OnValidate()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (speedThreshold1 >= speedThreshold2)
        {
            Debug.LogWarning("speedThreshold1 は speedThreshold2 より小さく設定してください。");
        }
    }
}