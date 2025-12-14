using UnityEngine;
using UnityEngine.U2D.Animation;

namespace AtomosZ.DFDQ.Tiles.Widgets
{
	public class DestructableObject : MonoBehaviour
	{
		[SerializeField] private SpriteResolver damageResolver;
		[SerializeField] private string category;
		[SerializeField] private string baseLabel;
		[SerializeField] private string damageCategory;
		[SerializeField] private string baseDamageLabel;
		[SerializeField] private int currentDamageLevel = 0;

		public void RepairDamage()
		{
			currentDamageLevel = Mathf.Max(currentDamageLevel - 1, 0);
			UpdateDamageGFX();
		}

		public void TakeDamage()
		{
			currentDamageLevel += 1;
			UpdateDamageGFX();
		}

		private void UpdateDamageGFX()
		{
			var baseResolver = GetComponent<SpriteResolver>();
			damageResolver.gameObject.SetActive(true);
			string decalLabel = baseDamageLabel;
			string baseResolverLabel = baseLabel;

			switch (currentDamageLevel)
			{
				case > 5:
					damageResolver.gameObject.SetActive(false);
					var rand = Random.Range(0, 2);
					baseResolverLabel = "rubble_" + rand.ToString("00");
					break;
				case 5:
				{
					damageResolver.gameObject.SetActive(false);

					baseResolverLabel += "_broken";
					if (baseLabel.Contains("_right"))
						GetComponent<SpriteRenderer>().flipX = true;
					else
						GetComponent<SpriteRenderer>().flipX = false;
				}
				break;

				case < 1:
					decalLabel = decalLabel.Replace("_dark", "");
					decalLabel += "_none";
					break;
				case 1:
					decalLabel += "_small";
					break;
				case 2:
					decalLabel += "_mid";
					break;
				case 3:
					decalLabel += "_large";
					break;
				case 4:
					decalLabel += "_hole";
					break;

			}

			damageResolver.SetCategoryAndLabel(damageCategory, decalLabel);
			baseResolver.SetCategoryAndLabel(category, baseResolverLabel);
		}
	}
}