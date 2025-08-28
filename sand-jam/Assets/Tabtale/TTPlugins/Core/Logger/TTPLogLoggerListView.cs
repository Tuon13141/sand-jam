using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Tabtale.TTPlugins
{
	public class TTPLogLoggerListView : TTPLogMessageList<TTPLoggerViewHolder>
	{
		private TTPLoggerFiltersBaseFilter gameProgressionFilter = new TTPLoggerFiltersGameProgression();
		
		public RectTransform itemPrefab;

		public List<string> Data { get; private set; }

		bool _eventFilter;
		LayoutElement _PrefabLayoutElement;

		Dictionary<RectTransform, TTPLoggerViewHolder> _MapRootToViewsHolder =
			new Dictionary<RectTransform, TTPLoggerViewHolder>();

	    public void ToggleEventFilter()
		{
			_eventFilter = !_eventFilter;
			RefreshList();
		}

		protected override void Awake()
		{
			base.Awake();

			Data = new List<string>();
			_PrefabLayoutElement = itemPrefab.GetComponent<LayoutElement>();
		}

		protected override void Start()
		{
			base.Start();
			Data.AddRange(TTPLogger.GetLogs());
			TTPLogger.onAddMessage = msg =>
			{
				if (IsFilteredIn(msg))
				{
					Data.Add(msg);
					InsertItems(Data.Count - 1, 1);
				}
			};
			ResetItems(Data.Count);
		}

		protected override TTPLoggerViewHolder CreateViewsHolder(int itemIndex)
		{
			var instance = new TTPLoggerViewHolder();
			instance.Init(itemPrefab, itemIndex);
			_MapRootToViewsHolder[instance.root] = instance;

			return instance;
		}

		protected override void UpdateViewsHolder(TTPLoggerViewHolder vh)
		{
			vh.UpdateViews(Data[vh.ItemIndex]);
		}

		void RefreshList()
		{
			Data.Clear();
			var logs = TTPLogger.GetLogs().Where(item => IsFilteredIn(item)).ToList();
			Data.AddRange(logs);
			ResetItems(Data.Count);
		}

		bool IsFilteredIn(string message)
		{
			return !_eventFilter || message.Contains(EVENT_PREFIX) || gameProgressionFilter.Matches(message);
		}

		const string EVENT_PREFIX = "TTPAnalytics::LogEvent:eventName=";
		
	}
}
