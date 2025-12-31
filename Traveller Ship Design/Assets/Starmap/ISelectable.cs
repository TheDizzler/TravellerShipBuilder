using UnityEngine;
using static AtomosZ.MG2eTraveller.Starmap.Starmap;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public interface ISelectable
	{
		public void SetInteractionState(InteractionState state, bool forcedStateChange = false);
	}

	public static class ISelectableExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="selectable"></param>
		/// <param name="newState"></param>
		/// <returns>False if no changes to state. True if state has changed.</returns>
		internal static bool CheckState(this ISelectable selectable, InteractionState newState, ref InteractionState currentState)
		{
			if (newState == currentState)
				return false;
			if (newState == InteractionState.None && currentState == InteractionState.SelectedMouseOver)
				currentState = InteractionState.Selected;
			else if ((newState == InteractionState.Selected && currentState == InteractionState.MouseOver)
				|| (newState == InteractionState.MouseOver && currentState == InteractionState.Selected))
				currentState = InteractionState.SelectedMouseOver;
			else
				currentState = newState;
			return true;
		}
	}
}