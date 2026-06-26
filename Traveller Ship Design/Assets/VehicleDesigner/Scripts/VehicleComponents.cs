using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


using UnityEngine;

namespace AtomosZ.MG2eTraveller.Vehicle
{
	public enum ChasisSize
	{
		Light,
		Heavy
	}
	public enum ChassisType
	{
		Light_Ground,
		Heavy_Ground,
		Light_Grav,
		Heavy_Grav,
		Unpowered_Ground,
		Unpowered_Boat,
		Powered_Boat,
		Ship,
		Light_Submersible,
		Heavy_Submersible,
		Airship,
		Light_Aeroplane,
		Heavy_Aeroplane,
		Light_Jet,
		Heavy_Jet,
		/// <summary>
		/// Helicopter, Aerodyne, Ornithopter
		/// </summary>
		Rotor_Flyer,
		Light_Walker,
		Heavy_Walker,
		Light_Hovercraft,
		Heavy_Hovercraft,
	}

	public enum Skill
	{
		Drive_Wheel,
		Drive_Track,
		Drive_Mole,
		Flyer_Grav,
	}

	[Serializable]
	public class Chassis
	{
		public ChassisType type;
		public string name;
		public Skill skill;
		public uint techLevel;
		public int agility;
		public uint minSpace;
		public uint maxSpace;
		public uint costPerSpace;
		public uint hullPerSpace;
		public float shippingTonsPerSpace;
		public List<Trait> traits;
		public TechTable techTable;
		public List<Option> options;
		public List<string> examples;
	}

	public enum Trait
	{
		Open_Vehicle,
		Tracked,
		Off_Roader,
		ATV,
		/// <summary>
		/// Armoured Fighting Vehicle
		/// </summary>
		AFV,

		Gas_Envelope,
	}

	public enum OptionModType
	{
		/// <summary>
		/// int.
		/// </summary>
		MinTechLevel,
		/// <summary>
		/// Skill class.
		/// </summary>
		Skill,
		/// <summary>
		/// int.
		/// </summary>
		Agility,
		/// <summary>
		/// int with low byte being the min figure and the high byte the max.
		/// A 0 for the high byte means there is no max.
		/// </summary>
		Spaces,
		/// <summary>
		/// int added to the chassis cost per space.
		/// </summary>
		CostPerSpace,
		/// <summary>
		/// Adjustment for the chassis shipping tons/space.
		/// </summary>
		Shipping,
		/// <summary>
		/// int, adjusts speed band of speed column on tech table
		/// </summary>
		SpeedBandAdjust,
		/// <summary>
		/// This would include Supercavitating drive.
		/// </summary>
		NewTechTable,
		/// <summary>
		/// For every 100% increase in cost per space, adjust armour, safe and crush depth byt 100%.
		/// </summary>
		IncreasedDive,
		/// <summary>
		/// List&lt;Trait>
		/// </summary>
		Traits,
		/// <summary>
		/// This allows sub choices in a mod.
		/// Example usage:<br/>
		/// <c>modValues.Add(OptionModType.MultiMod, new Dictionary&lt;OptionModType, object>());</c>
		/// </summary>
		MultiMod,
	}

	public class MultiMod
	{
		public string altOptionName;
		public Dictionary<OptionModType, object> altOptionModdedValues;
	}

	public class Option
	{
		public string name;
		public string description;


		/// <summary>
		/// public uint techLevel;
		///public int spaceConsumption;
		///public Skill skill;
		///public int agility;
		///public uint minSpaces;
		///public uint maxSpaces;
		///public uint costPerSpace;
		///public int speedBandAdjust; 
		/// </summary>
		public Dictionary<OptionModType, object> optionModValues;
		//public List<Trait> traits;
	}

	public enum Speed
	{
		Very_Slow,
		Slow,
		Medium,
		High,
		Fast,
		Very_Fast,
	}

	public class RangedDictionary<T>
	{
		public HashSet<uint> indices = new();
		public Dictionary<uint, T> dict = new();

		public T this[uint index]
		{
			get
			{
				while (index >= 0)
				{
					if (indices.Contains(index))
						return dict[index];
					--index;
				}

				return default;
			}
			set
			{
				indices.Add(index);
				dict.Add(index, value);
			}
		}

		public T Last()
		{
			if (indices.Count == 0)
				return default;

			uint highest = 0;
			foreach (uint i in indices)
			{
				highest = Math.Max(i, highest);
			}

			return dict[highest];
		}
	}

	public class TechTableRow
	{
		public uint techLevel;
		public Speed speed;
		public uint range;

		/// <summary>
		/// Submersible only.
		/// </summary>
		public int safeDepth = -1;
		/// <summary>
		/// Submersible only.
		/// </summary>
		public int crushDepth = -1;
		/// <summary>
		/// Submersible only.
		/// </summary>
		public int lifeSupport = -1;

		public TechTableRow(uint techLevel, Speed speed, uint range)
		{
			this.techLevel = techLevel;
			this.speed = speed;
			this.range = range;
		}
	}

	public class TechTable : RangedDictionary<TechTableRow>
	{
		public TechTable(params TechTableRow[] values)
		{
			foreach (var techRow in values)
			{
				this[techRow.techLevel] = techRow;
			}
		}
	}

	public static class VehicleComponents
	{
		public static Dictionary<ChassisType, Chassis> chassisList = new()
		{
			[ChassisType.Light_Ground] = new Chassis
			{
				name = "Light Ground Vehicle",
				type = ChassisType.Light_Ground,

				techLevel = 4,
				skill = Skill.Drive_Wheel,
				agility = 0,
				minSpace = 1,
				maxSpace = 20,
				costPerSpace = 750,
				hullPerSpace = 2,
				shippingTonsPerSpace = 0.5f,
				traits = null,
				techTable = new TechTable(
					new TechTableRow(4, Speed.Very_Slow, 100),
					new TechTableRow(5, Speed.Slow, 200),
					new TechTableRow(7, Speed.Medium, 400),
					new TechTableRow(9, Speed.High, 500),
					new TechTableRow(11, Speed.Fast, 600)),
				options = new List<Option>
				{
					new Option
					{
						name = "Open Frame",
						description = "An open frame vehicle is a Light Ground Vehicle that"
							+ " the rider mounts rather than climbs inside. They often"
							+ "have just two or three wheels to make a motorcycle or trike."
							+ " The following changes are made to the Light Ground Vehicle chassis.",
						optionModValues = new Dictionary<OptionModType, object>
						{
							[OptionModType.Agility] = 1,
							[OptionModType.Spaces] = 1 | (3 << 8),
							[OptionModType.CostPerSpace] = 750,
							[OptionModType.SpeedBandAdjust] = 1,
							[OptionModType.Traits] = new List<Trait>
							{
								Trait.Open_Vehicle,
							},
						},

					},
					new Option
					{
						name = "Monowheel",
						description = "A development of the motorcycle, this vehicle uses"
							+   "complex gyroscopic systems to balance itself on a single wheel."
							+   " The following changes are made to the Light Ground Vehicle chassis.",
						optionModValues = new Dictionary<OptionModType, object>
						{
							[OptionModType.MinTechLevel] = 9,
							[OptionModType.Agility] = 2,
							[OptionModType.Spaces] = 1 | (3 << 8),
							[OptionModType.CostPerSpace] = 2500,
							[OptionModType.SpeedBandAdjust] = 1,
							[OptionModType.Traits] = new List<Trait>
							{
								Trait.Open_Vehicle,
							},
						},

					},
					new Option
					{
						name = "Rail Rider",
						description = "A Light Ground Vehicle can be designed to run on a"
							+ " rail network, either in addition to its normal travel or instead of."
							+ "This consumes no Spaces unless the vehicle"
							+ " is designed to run off rails as well, in which case it consumes 1 Space.",
						optionModValues = new Dictionary<OptionModType, object>
						{
							[OptionModType.Agility] = -2,
							[OptionModType.CostPerSpace] = 400,
							[OptionModType.SpeedBandAdjust] = 1,
							[OptionModType.MultiMod] = new MultiMod
							{
								altOptionName = "Wheeled",
								altOptionModdedValues = new Dictionary<OptionModType, object>
								{
									[OptionModType.Spaces] = 1,
								},
							}
						},
					},
					new Option
					{
						name = "Rough Terrain",
						description = "A {chassisName} can have its suspension and"
							+" drive systems modified, or extra wheels added to"
							+ " enable it to handle rough terrain. This grants it either"
							+ " the Off-Roader trait and increases the Cost per Space"
							+ " by Cr{cost1}, or the ATV trait and increases the Cost per Space by Cr{cost2}.",
						optionModValues = new Dictionary<OptionModType, object>
						{
							[OptionModType.CostPerSpace] = 100,
							[OptionModType.Traits] = new List<Trait>
							{
								Trait.Off_Roader,
							},
							[OptionModType.MultiMod] = new MultiMod
							{
								altOptionName = "ATV",
								altOptionModdedValues = new Dictionary<OptionModType, object>
								{
									[OptionModType.CostPerSpace] = 250,
									[OptionModType.Traits]  = new List<Trait>
									{
										Trait.ATV,
									},
								},

							},
						},
					},
					new Option
					{
						name = "Tracks",
						description = "A {chassisName} can be built with tracks instead"
							+ " of wheels, specialising it to handle difficult terrain at"
							+ " the expense of performance on roads. The following"
							+ " changes are made to the {chassisName} chassis.",
						optionModValues = new Dictionary<OptionModType, object>
						{
							[OptionModType.MinTechLevel]= 5,
							[OptionModType.Skill] = Skill.Drive_Track,
							[OptionModType.CostPerSpace] = 750,
							[OptionModType.SpeedBandAdjust] = -1,
							[OptionModType.Traits] = new List<Trait>
							{
								Trait.Tracked,
							}
						},
					}
				},
			},
			/*
			[ChassisType.Heavy_Ground] = new Chassis
			{
				name = "Heavy Ground Vehicle",
				type = ChassisType.Heavy_Ground,

				techLevel = 4,
				skill = Skill.Drive_Wheel,
				agility = -2,
				minSpace = 20,
				maxSpace = uint.MaxValue,
				costPerSpace = 3000,
				hullPerSpace = 3,
				shippingTonsPerSpace = 0.5f,
				traits = null,
				techTable = new TechTable(
					new TechTableRow(4, Speed.Very_Slow, 200),
					new TechTableRow(5, Speed.Slow, 300),
					new TechTableRow(7, Speed.Medium, 400),
					new TechTableRow(9, Speed.Medium, 500),
					new TechTableRow(11, Speed.High, 600)),
				options = new List<Option>
				{
					new Option
					{
						name = "Armoured Fighting Vehicle",
						techLevel = 5,
						["costPerSpace"] = 3000,
						["speedBandAdjust"] = -1,
						traits = new List<Trait>
						{
							Trait.AFV,
							Trait.Off_Roader,
						},
					},
					new Option
					{
						name = "Rail Rider (No Wheels)",
						["agility"] = -2,
						["costPerSpace"] = 1000,
						["speedBandAdjust"] = 1,
					},
					new Option
					{
						name = "Rail Rider (Plus Wheels)*",
						// special: -2 agi on rails only
						//["agility"] = -2,
						["costPerSpace"] = 1000,
						// special: speed band +1 when on rails
						// ["speedBandAdjust"] = 1,
						 spaceConsumption = 1,
					},
					new Option
					{
						name = "Rough Terrain (Off Road)",
						["costPerSpace"] = 500,
						traits = new List<Trait>
						{
							Trait.Off_Roader,
						},
					},
					new Option
					{
						name = "Rough Terrain (ATV)",
						["costPerSpace"] = 1000,
						traits = new List<Trait>
						{
							Trait.ATV,
						},
					},
					new Option
					{
						name = "Tracks*",
						description = "A {chassisName} can be built with tracks instead"
							+ " of wheels, specialising it to handle difficult terrain at"
							+ " the expense of performance on roads. The following"
							+ " changes are made to the {chassisName} chassis.",
						techLevel = 5,
						skill = Skill.Drive_Track,
						["costPerSpace"] = 2000,
						// Special: speed band adjust only if NOT AFV
						// ["speedBandAdjust"] = -1,
						traits = new List<Trait>
						{
							Trait.Tracked,
						}
					},
					new Option
					{
						name = "Tunneller*",
						// special: while tunnelling, moves 10m/h * Tech level
						techLevel = 7,
						skill = Skill.Drive_Mole,
						["costPerSpace"] = 25000,
						["speedBandAdjust"] = -1,
					},
				}
			},
			[ChassisType.Light_Grav] = new Chassis
			{
				name = "Light Grav Vehicle",
				type = ChassisType.Light_Grav,

				techLevel = 8,
				skill = Skill.Flyer_Grav,
				agility = 1,
				minSpace = 1,
				maxSpace = 20,
				costPerSpace = 30000,
				hullPerSpace = 2,
				shippingTonsPerSpace = 0.5f,
				traits = null,
				techTable = new TechTable(
					new TechTableRow(8, Speed.High, 1000),
					new TechTableRow(9, Speed.Fast, 2000),
					new TechTableRow(11, Speed.Fast, 3000),
					new TechTableRow(13, Speed.Very_Fast, 4000),
					new TechTableRow(15, Speed.Very_Fast, 5000)),
				options = new List<Option>
				{
					new Option
					{
						name = "Open Frame",
						["agility"] = 1,
						["minSpaces"] = 1,
						["maxSpaces"] = 3,
						["costPerSpace"] = 10000,
						["speedBandAdjust"] = 1,
						traits = new List<Trait>
						{
							Trait.Open_Vehicle,
						}
					},
					new Option
					{
						name = "Streamlined",
						["agility"] = 1,
						["costPerSpace"] = 30000,
						["speedBandAdjust"] = 1,
					}
				}
			},
			[ChassisType.Heavy_Grav] = new Chassis
			{
				name = "Heavy Grav Vehicle",
				type = ChassisType.Heavy_Grav,

				techLevel = 8,
				skill = Skill.Flyer_Grav,
				agility = -1,
				minSpace = 20,
				maxSpace = int.MaxValue,
				costPerSpace = 80000,
				hullPerSpace = 2,
				shippingTonsPerSpace = 0.5f,
				traits = null,
				techTable = new TechTable(
					new TechTableRow(8, Speed.High, 1000),
					new TechTableRow(9, Speed.Fast, 2000),
					new TechTableRow(11, Speed.Fast, 3000),
					new TechTableRow(13, Speed.Fast, 4000),
					new TechTableRow(15, Speed.Very_Fast, 5000)),
				options = new List<Option>
				{
					new Option
					{
						name = "Armoured Fighting Vehicle",
						["costPerSpace"] = 100000,
						["speedBandAdjust"] = -1,
						traits = new List<Trait>
						{
							Trait.AFV,
						},
					},
					new Option
					{
						name = "Streamlined",
						["costPerSpace"] = 50000,
						["speedBandAdjust"] = 1,
					}
				}
			},*/
		};
	}
}