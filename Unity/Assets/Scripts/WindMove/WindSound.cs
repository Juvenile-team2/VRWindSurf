using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AdvancedSpeedBasedSoundSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardSpeed boardSpeedController;
    [SerializeField] private AudioClip soundForSpeedRange1;
    [SerializeField] private AudioClip soundForSpeedRange2;

    [Header("Settings")]
    [SerializeField] private float speedThreshold1 = 1f; // ���̒l�ȏ�� soundForSpeedRange1
    [SerializeField] private float speedThreshold2 = 10f; // ���̒l�ȏ�� soundForSpeedRange2
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
            Debug.LogError("BoardSpeedController���ݒ肳��Ă��܂���B");
            enabled = false;
            return;
        }

        // soundForSpeedRange1 �� soundForSpeedRange2 �� null �ł����e�i�����Đ����Ȃ��ꍇ�����邽�߁j

        if (speedThreshold1 >= speedThreshold2)
        {
            Debug.LogError("speedThreshold1 �� speedThreshold2 ��菬�����ݒ肵�Ă��������B");
            enabled = false;
            return;
        }

        // ������Ԃ͉����Đ����Ȃ�
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
                // ��Ԃ��ς�������A�܂��͍Đ����łȂ��ꍇ�ɍĐ�/�؂�ւ�
                if (audioSource.clip != clipToPlay || !audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.clip = clipToPlay;
                    audioSource.loop = loopSounds;
                    audioSource.Play();
                }
            }
            else // �Đ����ׂ��N���b�v���Ȃ��ꍇ
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
            Debug.LogWarning("speedThreshold1 �� speedThreshold2 ��菬�����ݒ肵�Ă��������B");
        }
    }
}