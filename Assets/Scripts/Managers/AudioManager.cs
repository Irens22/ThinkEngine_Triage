using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource backgroundMusic;

    public void ToggleMusic()
    {
        if(backgroundMusic != null)
        {
            backgroundMusic.mute = !backgroundMusic.mute;
        }
    }
}