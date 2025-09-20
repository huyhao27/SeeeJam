using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource[] audios;

    /// <summary>
    /// Phát SFX theo index trong mảng audios
    /// </summary>
    public void PlaySfx(int index)
    {
        if (audios == null || audios.Length == 0)
        {
            Debug.LogWarning("SoundManager: Chưa gán AudioSource nào!");
            return;
        }

        if (index < 0 || index >= audios.Length)
        {
            Debug.LogWarning($"SoundManager: Index {index} không hợp lệ!");
            return;
        }

        AudioSource sfx = audios[index];
        if (sfx != null)
        {
            sfx.Play();
        }
    }

    /// <summary>
    /// Phát SFX một lần mà không cần lo bị cắt (PlayOneShot)
    /// </summary>
    public void PlaySfxOneShot(int index)
    {
        if (audios == null || audios.Length == 0) return;
        if (index < 0 || index >= audios.Length) return;

        AudioSource sfx = audios[index];
        if (sfx != null && sfx.clip != null)
        {
            sfx.PlayOneShot(sfx.clip);
        }
    }
}