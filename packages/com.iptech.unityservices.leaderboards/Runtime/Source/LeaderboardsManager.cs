using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPTech.Platform;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;

namespace IPTech.UnityServices.Leaderboards {
    public class LeaderboardsManager : ILeaderboardsManager {
        private readonly IUnityServicesManager unityServicesManager;
        private ILeaderboardsService service;
        private EOnlineState onlineState;

        public LeaderboardsManager(IUnityServicesManager unityServicesManager) {
            onlineState = EOnlineState.Offline;
            this.unityServicesManager = unityServicesManager;
            Initialize();
        }

        public event Action<EServiceState> Initialized;
        public event Action<EOnlineState> OnlineStateChanged;

        public EServiceState State { get; private set; }
        public EOnlineState OnlineState {
            get => onlineState;
            set {
                if(value != onlineState) {
                    onlineState = value;
                    OnOnlineStateChanged();
                }
                return;

                void OnOnlineStateChanged() {
                    OnlineStateChanged?.Invoke(onlineState);
                }
            }
        }

        public async Task<ILeaderboardEntry> AddScore(string leaderboardId, double score) {
            Debug.Log($"[IPTech.UnityServices.Leaderboards] adding score started leaderboardId={leaderboardId}, score={score}");
            await WaitUntilInitialized();
            var res = await service.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log($"[IPTech.UnityServices.Leaderboards] adding score finished leaderboardId={leaderboardId}, score={score}");
            return new LeaderboardEntryAdapter(res);
        }

        public async Task<ILeaderboardScoresPage> GetScores(string leaderboardId) {
            Debug.Log($"[IPTech.UnityServices.Leaderboards] get scores started leaderboardId={leaderboardId}");
            await WaitUntilInitialized();
            var res = await service.GetScoresAsync(leaderboardId);
            Debug.Log($"[IPTech.UnityServices.Leaderboards] get scores finished leaderboardId={leaderboardId}");
            return new LeaderboardScoresPageAdapter(res);
        }

        public async Task<List<ILeaderboardEntry>> GetScoresInPlayerRange(string leaderboardId, int rangeLimit) {
            Debug.Log($"[IPTech.UnityServices.Leaderboards] get scores in player range started leaderboardId={leaderboardId}, rangeLimit={rangeLimit}");
            await WaitUntilInitialized();
            try {
                var res = await service.GetPlayerRangeAsync(leaderboardId, new GetPlayerRangeOptions { RangeLimit = rangeLimit });
                Debug.Log($"[IPTech.UnityServices.Leaderboards] get scores in player range finished leaderboardId={leaderboardId}, rangeLimit={rangeLimit}");
                return res.Results.Select(r => (ILeaderboardEntry)new LeaderboardEntryAdapter(r)).ToList();
            } catch(LeaderboardsException lex) {
                if(lex.Reason == LeaderboardsExceptionReason.EntryNotFound) {
                    var res = await GetScores(leaderboardId);
                    return res.Entries;
                }
                throw;
            }
        }

        void Initialize() {
            if(unityServicesManager.State == EServiceState.Initializing) {
                unityServicesManager.Initialized += HandleInitialized;
                return;
            }
            HandleInitialized(unityServicesManager.State);
        }

        void HandleInitialized(EServiceState state) {
            Debug.Log($"[IPTech.UnityServices.Leaderboards] initializing...");
            if(state == EServiceState.Initialized) {
                service = LeaderboardsService.Instance;
                if(unityServicesManager.Authentication.IsSignedIn) {
                    OnlineState = unityServicesManager.OnlineState;
                }
                unityServicesManager.Authentication.SignInChanged += HandleSignInChanged;
                unityServicesManager.OnlineStateChanged += HandleOnlineStateChanged;
            }
            State = state;
            Debug.Log($"[IPTech.UnityServices.Leaderboards] initialization complete: State={State}");
            Initialized?.Invoke(State);
        }

        private void HandleOnlineStateChanged(EOnlineState obj) {
            HandleSignInChanged();
        }

        private void HandleSignInChanged() {
            if(unityServicesManager.Authentication.IsSignedIn) {
                OnlineState = unityServicesManager.OnlineState;
            } else {
                OnlineState = EOnlineState.Offline;
            }
        }

        async Task WaitUntilInitialized() {
            while(State == EServiceState.Initializing) {
                await Task.Yield();
                if(!Application.isPlaying) throw new OperationCanceledException("unity editor exited play mode");
            }
            if(unityServicesManager.State == EServiceState.FailedToInitialize) {
                throw new ApiUnavailableException(nameof(LeaderboardsManager));
            }
        }
    }
}
