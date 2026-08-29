using UnityEngine;
using System.Collections.Generic;

public class Sound_Manager
{
    public AudioSource[] _audioSources = new AudioSource[(int)Sound.Max];
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private GameObject _soundRoot = null;

    public float BGMValue, VFXValue;

    public void Init()
    {
        BGMValue = PlayerPrefs.GetFloat("BGM");
        VFXValue = PlayerPrefs.GetFloat("VFX");

        if (_soundRoot == null)
        {
            _soundRoot = GameObject.Find("@SoundRoot");
            if(_soundRoot == null)
            {
                _soundRoot = new GameObject { name = "@SoundRoot" };
                Object.DontDestroyOnLoad(_soundRoot);

                string[] soundTypeName = System.Enum.GetNames(typeof(Sound));
                for(int i =0; i< soundTypeName.Length - 1; i++)
                {
                    GameObject go = new GameObject { name = soundTypeName[i] };
                    _audioSources[i] = go.AddComponent<AudioSource>();
                    go.transform.parent = _soundRoot.transform;
                }

                _audioSources[(int)Sound.BGM].loop = true;
            }
        }
    }



    // Base_Manager.Sound.Play()
    public bool Play(Sound type, string path)
    {
        if(string.IsNullOrEmpty(path))
        {
            Debug.Log("경로에 오디오 파일이 존재하지 않습니다.");
            return false;
        }

        AudioSource audioSource = _audioSources[(int)type];

        if(path.Contains("Sound/") == false)
        {
            path = string.Format("Sound/{0}", path);
        }

        if(type == Sound.BGM)
        {
            AudioClip audioClip = Resources.Load<AudioClip>(path);

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.volume = BGMValue;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if(type == Sound.Effect)
        {
            AudioClip audioClip = GetAudioClip(path);
            if (audioClip == null)
                return false;

            audioSource.volume = VFXValue;
            audioSource.PlayOneShot(audioClip);
            return true;
        }
        return false;
    }

    private AudioClip GetAudioClip(string path)
    {
        AudioClip audioClip = null;

        if (_audioClips.TryGetValue(path, out audioClip))
            return audioClip;

        audioClip = Resources.Load<AudioClip>(path);

        _audioClips.Add(path, audioClip);
        return audioClip;
    }
}
