using System;
using System.Collections.Generic;
using MoralisUnity.Samples.Shared.Data.Types;
using MoralisUnity.Samples.TheGame.MVCS.Model.Data.Types;
using MoralisUnity.Samples.TheGame.MVCS.Model.Data.Types.Configuration;
using MoralisUnity.Samples.TheGame.MVCS.Networking;
using Unity.Collections;
using Unity.Netcode;

namespace MoralisUnity.Samples.TheGame.MVCS.Model
{
	//TODO: move this CustomPlayerInfo class. rename it?
	/// <summary>
	/// Observable<t> does not like 'string'. So I created a wrapper class.
	/// </summary>
	public struct CustomPlayerInfo : INetworkSerializable
	{
		public FixedString128Bytes Nickname;
		public FixedString128Bytes Web3Address;
		
		/// <summary>
		/// Required for use in the <see cref="NetworkVariable{T}"/> by <see cref="PlayerView_NetworkBehaviour"/>
		/// </summary>
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Nickname);
			serializer.SerializeValue(ref Web3Address);
		}

		public bool IsNullNickname()
		{
			return Nickname == "";
		}
		public bool IsNullWeb3Address()
		{
			return Web3Address == "";
		}
		public bool IsNull()
		{
			return IsNullNickname() && IsNullWeb3Address();
		}
		
		
	}
	
	/// <summary>
	/// Stores data for the game
	///		* See <see cref="TheGameSingleton"/>
	/// </summary>
	public class TheGameModel 
	{
		// Properties -------------------------------------
		public TheGameConfiguration TheGameConfiguration { get { return TheGameConfiguration.Instance; }  }
		public Observable<int> Gold { get { return _gold; } }
		public Observable<bool> IsRegistered { get { return _isRegistered; } }
		public Observable<CustomPlayerInfo> CustomPlayerInfo { get { return _customPlayerInfo; } }
		public Observable<List<Prize>> Prizes { get { return _prizes; } }

		// Fields -----------------------------------------
		private Observable<int> _gold = new Observable<int>();
		private ObservablePrizes _prizes = new ObservablePrizes();
		private Observable<CustomPlayerInfo> _customPlayerInfo = new Observable<CustomPlayerInfo>();
		private Observable<bool> _isRegistered = new Observable<bool>();

		// Initialization Methods -------------------------
		public TheGameModel()
		{
			ResetAllData();
		}

		
		// General Methods --------------------------------
		public bool HasAnyData()
		{
			// TODO: Put real check here
			return false;
		}
		
		
		public void ResetAllData()
		{
			_gold.Value = 0;
			_customPlayerInfo.Value = new CustomPlayerInfo();
			_prizes.Value = new List<Prize>();
			_isRegistered.Value = false;
		}
		
		// Event Handlers ---------------------------------
	}
}