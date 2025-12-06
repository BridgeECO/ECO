using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ECO
{
    [RequireComponent(typeof(PlayableDirector), typeof(Animator))]
    public class AnimationAssist : MonoBehaviour
    {
        private PlayableDirector _pd = null;
        private Animator _animator = null;

        [SerializeField] private List<TimelineAsset> _timelineAssetList = new List<TimelineAsset>();
        private Dictionary<string, TimelineAsset> _timelineAssetDict = new Dictionary<string, TimelineAsset>();

        private void OnDestroy()
        {
            Destroy();
        }

        public void Destroy()
        {
            _pd.Stop();

            _timelineAssetList.Clear();
            _timelineAssetDict.Clear();
        }

        public void Awake()
        {
            if (!UNITY.TryGetComp(out _pd, this.gameObject))
                return;
            if (!UNITY.TryGetComp(out _animator, this.gameObject))
                return;

            SetTimlineBindings();
        }

        private void SetTimlineBindings()
        {
            foreach (var asset in _timelineAssetList)
            {
                if (asset == null)
                    continue;

                foreach (var track in asset.GetOutputTracks())
                {
                    if (track is AnimationTrack)
                    {
                        _pd.SetGenericBinding(track, _animator);
                    }
                    else if (track is AkTimelineEventTrack)
                    {
                        _pd.SetGenericBinding(track, this.gameObject);
                    }
                    else if (track is MarkerTrack)
                    {

                    }
                    else
                    {
                        LOG.Error($"No Handling Type of Track. GameObject({this.gameObject.name}), TrackName({track.name})");
                    }

                }

                string name = this.name;
                string mainKey = asset.name;
                string subKey = mainKey.Remove(0, name.Length - 1);

                if (!_timelineAssetDict.TryAdd(mainKey, asset))
                    LOG.Error($"Already SameKey Exists. MainKey({mainKey})");

                if (!_timelineAssetDict.TryAdd(subKey, asset))
                    LOG.Error($"Already SameKey Exists. SubKey({subKey})");
            }
        }

        public void Play(string key, UnityAction<string> onCompleteAct = null)
        {
            _Play(key, DirectorWrapMode.None, onCompleteAct);
        }

        private float _Play(string key, DirectorWrapMode mode, UnityAction<string> onCompleteAct)
        {
            if (!_timelineAssetDict.TryGetValue(key, out var asset))
            {
                LOG.Error($"Not Found Key({key})");
                PrintAnimKeys();
                return 0;
            }

            if (_pd.state == PlayState.Playing)
                _pd.Stop();

            _pd.Play(asset, mode);
            return (float)_pd.duration;
        }

        private void PrintAnimKeys()
        {
            foreach (var pair in _timelineAssetDict)
            {
                LOG.Info($"Timeline Key({pair.Key}), Timeline Value({pair.Value.name})");
            }
        }
    }
}