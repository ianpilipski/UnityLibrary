using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.UnityServices.Internal {
    public class RewardAd : AdBase
    {
        bool rewardWasCalled;

        public RewardAd(string placementName) : base(placementName) {
        }

        override protected void LoadAd() {
            #if IPTECH_IRONSOURCE_INSTALLED
            IronSource.Agent.loadRewardedVideo();
            #endif
        }

        override protected async Task ShowAd() {
            #if IPTECH_IRONSOURCE_INSTALLED
            if (IronSource.Agent.isInterstitialReady()) {
                IronSource.Agent.showInterstitial(result.PlacementID);
                await WaitWhile(() => showState != ShowState.FinishedShowing );
                if(result.AdResult == AdResult.Cancelled) {
                    await WaitForRewardWithTimeout();
                }
            } else {
                result.AdResult = AdResult.FaildToLoad;
            }
            #endif
        }

        async Task WaitForRewardWithTimeout() {
            DateTime timeout = DateTime.Now.AddSeconds(5);
            while(!rewardWasCalled) {
                await Task.Yield();
                if(DateTime.Now > timeout) break;
            }

            if(rewardWasCalled) {
                result.AdResult = AdResult.Watched;
            }
        }

#if IPTECH_IRONSOURCE_INSTALLED
        protected override void ReardedVideoOnAdAvailable(IronSourceAdInfo info)
        {
            result.Info = info;
            loadState = LoadState.Loaded;
        }

        protected override void ReardedVideoOnAdUnavailable()
        {
            loadState = LoadState.FailedToLoad;
        }

        protected override void ReardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo info)
        {
            result.Info = info;
            result.UserClicked = true;
        }

        // This may come out of order with Reward Event!!
        protected override void ReardedVideoOnAdClosedEvent(IronSourceAdInfo info)
        {
            result.Info = info;
            result.AdResult = AdResult.Cancelled;
            showState = ShowState.FinishedShowing;
        }

        protected override void ReardedVideoOnAdOpenedEvent(IronSourceAdInfo info)
        {
            result.Info = info;
        }

        // This may come out of order with Closed Event!!
        protected override void ReardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo info)
        {
            result.Info = info;
            rewardWasCalled = true;
        }

        protected override void ReardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info)
        {
            result.Info = info;
            result.AdResult = AdResult.FailedToShow;
            showState = ShowState.FinishedShowing;
        }
#endif
        
    }
}
