using AtomosZ.DFDQ.Battle.Units;
using AtomosZ.DFDQ.Tiles;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;

namespace AtomosZ.DFDQ.Battle
{
	public class MenuManager : MonoBehaviour
	{
		public static MenuManager instance;

		[SerializeField] private GameObject selectedHeroObject;
		[SerializeField] private GameObject tileInfoObject;
		[SerializeField] private GameObject tileUnitInfoObject;

		void Awake()
		{
			instance = this;
		}

		public void ShowTileInfo(GridTile tile)
		{
			if (tile == null)
			{
				tileInfoObject.SetActive(false);
				tileUnitInfoObject.SetActive(false);
				return;
			}

			tileInfoObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.tileName;
			tileInfoObject.SetActive(true);

			if (tile.occupiedUnit == null)
			{
				tileUnitInfoObject.SetActive(false);
				return;
			}

			tileUnitInfoObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.occupiedUnit.unitName;
			tileUnitInfoObject.SetActive(true);
		}

		public void ShowSelectedHero(BaseUnit hero)
		{
			if (hero == null)
			{
				selectedHeroObject.SetActive(false);
				return;
			}

			selectedHeroObject.GetComponentInChildren<TextMeshProUGUI>().text = hero.unitName;
			selectedHeroObject.SetActive(true);
		}
	}
}