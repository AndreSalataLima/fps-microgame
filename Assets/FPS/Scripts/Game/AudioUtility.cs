using UnityEngine;
using UnityEngine.Audio;

namespace Unity.FPS.Game
{
    public class AudioUtility
    {
        static AudioManager s_AudioManager;

        public enum AudioGroups
        {
            DamageTick,
            Impact,
            EnemyDetection,
            Pickup,
            WeaponShoot,
            WeaponOverheat,
            WeaponChargeBuildup,
            WeaponChargeLoop,
            HUDVictory,
            HUDObjective,
            EnemyAttack
        }

        public static void CreateSFX(AudioClip clip, Vector3 position, AudioGroups audioGroup, float spatialBlend,
            float rolloffDistanceMin = 1f)
        {
            GameObject impactSfxInstance = new GameObject("SFX: " + clip.name);
            impactSfxInstance.transform.position = position;
            AudioSource source = impactSfxInstance.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spatialBlend;
            source.minDistance = rolloffDistanceMin;

            // Assign mixer group if available
            var mixerGroup = GetAudioGroup(audioGroup);
            if (mixerGroup != null)
            {
                source.outputAudioMixerGroup = mixerGroup;
            }
            else
            {
                Debug.LogWarning($"AudioUtility: no AudioMixerGroup for {audioGroup}, using default output");
            }

            source.Play();

            var timedSelfDestruct = impactSfxInstance.AddComponent<TimedSelfDestruct>();
            timedSelfDestruct.LifeTime = clip.length;
        }

        public static AudioMixerGroup GetAudioGroup(AudioGroups group)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindObjectOfType<AudioManager>();

            if (s_AudioManager == null)
            {
                Debug.LogWarning("AudioUtility: AudioManager not found, cannot find mixer groups");
                return null;
            }

            var groups = s_AudioManager.FindMatchingGroups(group.ToString());
            if (groups.Length > 0)
                return groups[0];

            Debug.LogWarning("AudioUtility: audio group not found for " + group);
            return null;
        }

        public static void SetMasterVolume(float value)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindObjectOfType<AudioManager>();

            if (s_AudioManager != null)
            {
                // Avoid log of zero or negative
                if (value <= 0f) value = 0.001f;
                float valueInDb = Mathf.Log10(value) * 20f;
                s_AudioManager.SetFloat("MasterVolume", valueInDb);
            }
            else
            {
                Debug.LogWarning("AudioUtility: AudioManager not found, falling back to AudioListener.volume");
                AudioListener.volume = Mathf.Clamp01(value);
            }
        }

        public static float GetMasterVolume()
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindObjectOfType<AudioManager>();

            if (s_AudioManager != null)
            {
                s_AudioManager.GetFloat("MasterVolume", out var valueInDb);
                return Mathf.Pow(10f, valueInDb / 20f);
            }
            else
            {
                Debug.LogWarning("AudioUtility: AudioManager not found, falling back to AudioListener.volume");
                return AudioListener.volume;
            }
        }
    }
}
