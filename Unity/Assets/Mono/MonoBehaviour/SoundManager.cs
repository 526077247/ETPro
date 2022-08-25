using UnityEngine;
using UnityEngine.Audio;

namespace ET
{
    public class SoundManager:MonoBehaviour
    {
        public static SoundManager Instance;
        [HideInInspector]
        public AudioSource m_bgm;
        [HideInInspector]
        public Transform _soundsRoot;
        [HideInInspector]
        public GameObject _soundsClipClone;


        public AudioMixer BGM;
        public AudioMixer Sound;
        public void Awake()
        {
            SoundManager.Instance = this;
            GameObject go = GameObject.Find("SoundsRoot");
            if (go == null)
            {
                go = new GameObject("SoundsRoot");
            }
            _soundsRoot = go.transform;
            
            m_bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>();
            if (m_bgm == null)
            {
                m_bgm = new GameObject("BGMManager", typeof(AudioSource)).GetComponent<AudioSource>();
                m_bgm.transform.SetParent(_soundsRoot);
            }
            
            _soundsClipClone = GameObject.Find("Source");
            if (_soundsClipClone == null)
            {
                _soundsClipClone = new GameObject("Source", typeof(AudioSource));
                _soundsClipClone.transform.SetParent(_soundsRoot, false);
                _soundsClipClone.SetActive(false);
            }
            
            DontDestroyOnLoad(go);
        }

        public AudioSource CreateClipSource() {
            if (_soundsClipClone == null || _soundsRoot == null)
            {
                return null;
            }

            var obj = Instantiate(_soundsClipClone);
            obj.transform.SetParent(_soundsRoot, false);
            return obj.GetComponent<AudioSource>();
        }

        
    }
}