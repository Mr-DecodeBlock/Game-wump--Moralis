using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoralisUnity.Samples.Shared;
using MoralisUnity.Samples.Shared.Components;
using MoralisUnity.Samples.Shared.Data.Types;
using MoralisUnity.Samples.TheGame.MVCS.Controller.Events;
using MoralisUnity.Samples.TheGame.MVCS.Model;
using MoralisUnity.Samples.TheGame.MVCS.Model.Data.Types;
using MoralisUnity.Samples.TheGame.MVCS.Service;
using MoralisUnity.Samples.TheGame.MVCS.View;
using UnityEngine;

namespace MoralisUnity.Samples.TheGame.MVCS.Controller
{
	/// <summary>
	/// Stores data for the game
	///		* See <see cref="TheGameSingleton"/> - Handles the core functionality of the game
	/// </summary>
	public class TheGameController
	{
		// Events -----------------------------------------
		public TheGameModelUnityEvent OnTheGameModelChanged = new TheGameModelUnityEvent();
		public void OnTheGameModelChangedRefresh() { OnTheGameModelChanged.Invoke(_theGameModel); }


		// Properties -------------------------------------
		public PendingMessage PendingMessageForDeletion { get { return _theGameService.PendingMessageActive; } }
		public PendingMessage PendingMessageForSave { get { return _theGameService.PendingMessagePassive; } }
		
		// Wait, So click sound is audible before scene changes
		private const int DelayLoadSceneMilliseconds = 100;

		// Fields -----------------------------------------
		private readonly TheGameModel _theGameModel = null;
		private readonly TheGameView _theGameView = null;
		private readonly ITheGameService _theGameService = null;
	

		// Initialization Methods -------------------------
		public TheGameController(
			TheGameModel theGameModel,
			TheGameView theGameView,
			ITheGameService theGameService)
		{
			_theGameModel = theGameModel;
			_theGameView = theGameView;
			_theGameService = theGameService;

			_theGameView.SceneManagerComponent.OnSceneLoadingEvent.AddListener(SceneManagerComponent_OnSceneLoadingEvent);
			_theGameView.SceneManagerComponent.OnSceneLoadedEvent.AddListener(SceneManagerComponent_OnSceneLoadedEvent);

			_theGameModel.Gold.OnValueChanged.AddListener((a) => OnTheGameModelChangedRefresh());
			_theGameModel.Prizes.OnValueChanged.AddListener((a) => OnTheGameModelChangedRefresh());
			_theGameModel.IsRegistered.OnValueChanged.AddListener((a) => OnTheGameModelChangedRefresh());
		}


		// General Methods --------------------------------
		
		///////////////////////////////////////////
		// Related To: MyMoralisWrapper
		///////////////////////////////////////////
		public async UniTask<bool> GetIsAuthenticatedAsync()
		{
			return await MyMoralisWrapper.Instance.IsAuthenticatedAsync();
		}
		
		public async UniTask<string> GetMoralisUserEthAddressAsync(bool useShortFormat)
		{
			string moralisUserEthAddress = await MyMoralisWrapper.Instance.GetMoralisUserEthAddressAsync();

			if (useShortFormat)
			{
				return MyMoralisWrapper.Instance.GetWeb3AddressShortFormat(moralisUserEthAddress);
			}
			else
			{
				return moralisUserEthAddress;
			}
		}

		///////////////////////////////////////////
		// Related To: Model
		///////////////////////////////////////////


		///////////////////////////////////////////
		// Related To: Service
		///////////////////////////////////////////

		// GETTER Methods -------------------------
		
		/// <summary>
		/// The checks "IsRegistered" *AND* if true, it loads
		/// some other data.
		///
		/// TODO: Does this method load too many things?
		/// </summary>
		/// <returns></returns>
		public async UniTask<bool> GetIsRegisteredAndUpdateModelAsync()
		{
			// Call Service. Sync Model
			bool isRegistered = await _theGameService.GetIsRegisteredAsync();
			_theGameModel.IsRegistered.Value = isRegistered;

			if (_theGameModel.IsRegistered.Value)
			{
				await GetGoldAndUpdateModelAsync();
				await GetPrizesAndUpdateModelAsync();
			}
			
			// Call Service
			//string lastRegisteredAddress = await _theGameService.GetLastRegisteredAddress();
			//Debug.Log("lastRegisteredAddress: " + lastRegisteredAddress);
			
			return _theGameModel.IsRegistered.Value;
		}

		public async UniTask<int> GetGoldAndUpdateModelAsync()
		{
			int gold = await _theGameService.GetGoldAsync();
			_theGameModel.Gold.Value = gold;
			return _theGameModel.Gold.Value;
		}
		
		public async UniTask<List<Prize>> GetPrizesAndUpdateModelAsync()
		{
			List<Prize> prizes = await _theGameService.GetPrizesAsync();
			_theGameModel.Prizes.Value = prizes;
			return _theGameModel.Prizes.Value;
		}
		
		public async UniTask<TransferLog> GetTransferLogHistoryAsync()
		{
			TransferLog result = await _theGameService.GetTransferLogHistoryAsync();
			return result;
		}

		// SETTER Methods -------------------------
		public async UniTask RegisterAsync()
		{
			await _theGameService.RegisterAsync();
			_theGameModel.IsRegistered.Value = await GetIsRegisteredAndUpdateModelAsync();
			
			// Wait for contract values to sync so the client will see the changes
			await DelayExtraAfterStateChangeAsync();
		}

		
		public async UniTask UnregisterAsync()
		{
			await _theGameService.UnregisterAsync();
			_theGameModel.IsRegistered.Value = await GetIsRegisteredAndUpdateModelAsync();

			// Wait for contract values to sync so the client will see the changes
			await DelayExtraAfterStateChangeAsync();
	
		}


		public async UniTask TransferGoldAsync()
		{
			await _theGameService.TransferGoldAsync();
			
			// Wait for contract values to sync so the client will see the changes
			await DelayExtraAfterStateChangeAsync();
		}

		
		public async UniTask TransferPrizeAsync(Prize prize)
		{
			if (prize == null)
			{
				throw new ArgumentException("TransferPrizeAsync() failed. prize = {prize}");
			}
			await _theGameService.TransferPrizeAsync(prize);
			
			// Wait for contract values to sync so the client will see the changes
			await DelayExtraAfterStateChangeAsync();
		}
		
		public async UniTask SafeReregisterDeleteAllPrizesAsync()
		{
			await _theGameService.SafeReregisterDeleteAllPrizesAsync();
			
			// Wait for contract values to sync so the client will see the changes
			await DelayExtraAfterStateChangeAsync();
		}

		///////////////////////////////////////////
		// Related To: View
		///////////////////////////////////////////
		public void PlayAudioClip(int audioClipIndex)
		{
			_theGameView.PlayAudioClip(audioClipIndex );
		}
		public void PlayAudioClipClick()
		{
			_theGameView.PlayAudioClipClick();
		}


		public async void LoadIntroSceneAsync()
		{
			// Wait, So click sound is audible before scene changes
			await UniTask.Delay(DelayLoadSceneMilliseconds);

			string sceneName = _theGameModel.TheGameConfiguration.IntroSceneData.SceneName;
			_theGameView.SceneManagerComponent.LoadScene(sceneName);
		}


		public async void LoadAuthenticationSceneAsync()
		{
			// Wait, So click sound is audible before scene changes
			await UniTask.Delay(DelayLoadSceneMilliseconds);

			string sceneName = _theGameModel.TheGameConfiguration.AuthenticationSceneData.SceneName;
			_theGameView.SceneManagerComponent.LoadScene(sceneName);
		}

		public async void LoadSettingsSceneAsync()
		{
			// Wait, So click sound is audible before scene changes
			await UniTask.Delay(DelayLoadSceneMilliseconds);

			string sceneName = _theGameModel.TheGameConfiguration.SettingsSceneData.SceneName;
			_theGameView.SceneManagerComponent.LoadScene(sceneName);
		}

		public async void LoadDeveloperConsoleSceneAsync()
		{
			// Wait, So click sound is audible before scene changes
			await UniTask.Delay(DelayLoadSceneMilliseconds);

			string sceneName = _theGameModel.TheGameConfiguration.DeveloperConsoleSceneData.SceneName;
			_theGameView.SceneManagerComponent.LoadScene(sceneName);
		}

		public async void LoadGameSceneAsync()
		{
			// Wait, So click sound is audible before scene changes
			await UniTask.Delay(DelayLoadSceneMilliseconds);

			string sceneName = _theGameModel.TheGameConfiguration.GameSceneData.SceneName;
			_theGameView.SceneManagerComponent.LoadScene(sceneName);
		}


		public async void LoadPreviousSceneAsync()
		{
			// Wait, So click sound is audible before scene changes
			await UniTask.Delay(DelayLoadSceneMilliseconds);

			_theGameView.SceneManagerComponent.LoadScenePrevious();
		}

		/// <summary>
		/// For short async messaging
		/// </summary>
		public async UniTask ShowMessagePassiveAsync(Func<UniTask> task)
		{
			await _theGameView.ShowMessageDuringMethodAsync(
				_theGameService.PendingMessagePassive.Message, task);
		}
		
		/// <summary>
		/// For long async messaging (e.g. "Waiting for wallet...", then "Loading..."
		/// </summary>
		public async UniTask ShowMessageActiveAsync(string title, Func<UniTask> task)
		{
			await _theGameView.ShowMessageDuringMethodAsync(
				$"{title}\n-\n{_theGameService.PendingMessageActive.Message}", task);
		}
		
		// Wait for contract values to sync so the client will see the changes
		private async UniTask DelayExtraAfterStateChangeAsync()
		{
			if (_theGameService.HasExtraDelay)
			{
				_theGameView.UpdateMessageDuringMethod(_theGameService.PendingMessageExtraDelay.Message);
				await _theGameService.DoExtraDelayAsync();
			}
		}
		
		public async UniTask ShowMessageCustomAsync(string message, int delayMilliseconds)
		{
			await _theGameView.ShowMessageWithDelayAsync(message, delayMilliseconds);
		}


		// Event Handlers ---------------------------------
		private void SceneManagerComponent_OnSceneLoadingEvent(SceneManagerComponent sceneManagerComponent)
		{
			if (_theGameView.BaseScreenCoverUI.IsVisible)
			{
				_theGameView.BaseScreenCoverUI.IsVisible = false;
			}
			
			// HACK: The WalletConnect prefab is not a robust Singleton pattern.
			// It does not work well if the prefab is in 2 or more scenes that are used at runtime. The 2 or more instances conflict.
			// So I manually delete the current one BEFORE the next scene loads. Works 100%
			if (MyMoralisWrapper.Instance.HasWalletConnectInstance)
			{
				MyMoralisWrapper.Instance.DestroyWalletConnectInstance();
			}

			if (DOTween.TotalPlayingTweens() > 0)
			{
				DOTween.KillAll();
			}
		}

		private void SceneManagerComponent_OnSceneLoadedEvent(SceneManagerComponent sceneManagerComponent)
		{
			// Do anything?
		}


		public void QuitGame()
		{
			if (Application.isEditor)
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif //UNITY_EDITOR
			}
			else
			{
				Application.Quit();
			}
		}



	}
}