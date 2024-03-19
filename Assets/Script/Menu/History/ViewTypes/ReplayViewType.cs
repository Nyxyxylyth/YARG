﻿using Cysharp.Threading.Tasks;
using UnityEngine;
using YARG.Core.Replays;
using YARG.Core.Replays.Analyzer;
using YARG.Core.Song;
using YARG.Helpers;
using YARG.Menu.Persistent;
using YARG.Replays;
using YARG.Settings;
using YARG.Song;

namespace YARG.Menu.History
{
    public class ReplayViewType : ViewType
    {
        public override BackgroundType Background => BackgroundType.Normal;

        public override bool UseFullContainer => true;

        private readonly ReplayEntry _replayEntry;
        private readonly SongEntry _songEntry;

        public ReplayViewType(ReplayEntry replayEntry)
        {
            _replayEntry = replayEntry;
            if (SongContainer.SongsByHash.TryGetValue(replayEntry.SongChecksum, out var songs))
            {
                _songEntry = songs[0];
            }
        }

        public override string GetPrimaryText(bool selected)
        {
            return FormatAs(_replayEntry.SongName, TextType.Primary, selected);
        }

        public override string GetSecondaryText(bool selected)
        {
            return FormatAs(_replayEntry.ArtistName, TextType.Secondary, selected);
        }

        public override async UniTask<Sprite> GetIcon()
        {
            // TODO: Show "song missing" icon instead
            if (_songEntry is null) return null;

            return await SongSources.SourceToIcon(_songEntry.Source);
        }

        public override void Confirm()
        {
            if (_songEntry is null) return;

            PlayReplay().Forget();
        }

        public override void Shortcut1()
        {
            if (_songEntry is null) return;

            AnalyzeReplay();
        }

        private async UniTaskVoid PlayReplay()
        {
            // Show warning
            if (SettingsManager.Settings.ShowEngineInconsistencyDialog)
            {
                var dialog = DialogManager.Instance.ShowOneTimeMessage(
                    LocaleHelper.LocalizeString("Dialogs.EngineInconsistency.Title"),
                    LocaleHelper.LocalizeString("Dialogs.EngineInconsistency"),
                    () =>
                    {
                        SettingsManager.Settings.ShowEngineInconsistencyDialog = false;
                        SettingsManager.SaveSettings();
                    });

                await dialog.WaitUntilClosed();
            }

            // We're good!
            GlobalVariables.State = PersistentState.Default;
            GlobalVariables.State.CurrentSong = _songEntry;
            GlobalVariables.State.CurrentReplay = _replayEntry;

            GlobalVariables.AudioManager.UnloadSong();
            GlobalVariables.Instance.LoadScene(SceneIndex.Gameplay);
        }

        private void AnalyzeReplay()
        {
            var chart = _songEntry.LoadChart();

            if (chart is null)
            {
                Debug.LogError("Chart did not load");
                return;
            }

            var replayReadResult = ReplayIO.ReadReplay(_replayEntry.ReplayPath, out var replayFile);
            if (replayReadResult != ReplayReadResult.Valid)
            {
                Debug.LogError("Replay did not load. " + replayReadResult);
                return;
            }

            var replay = replayFile!.Replay;

            var results = ReplayAnalyzer.AnalyzeReplay(chart, replay);

            for(int i = 0; i < results.Length; i++)
            {
                var analysisResult = results[i];

                var profile = replay.Frames[i].PlayerInfo.Profile;
                if (analysisResult.Passed)
                {
                    Debug.Log($"({profile.Name}, {profile.CurrentInstrument}/{profile.CurrentDifficulty}) PASSED verification!");
                }
                else
                {
                    Debug.LogWarning($"({profile.Name}, {profile.CurrentInstrument}/{profile.CurrentDifficulty}) FAILED verification");
                }
            }
        }

        public override GameInfo? GetGameInfo()
        {
            return new GameInfo
            {
                BandScore = _replayEntry.BandScore,
                BandStars = _replayEntry.BandStars
            };
        }
    }
}