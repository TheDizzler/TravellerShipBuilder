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
	}

	public class Option
	{
		public string name;
		public uint techLevel;
		public int spaceConsumption;
		public Skill skill;
		public int agility;
		public uint minSpaces;
		public uint maxSpaces;
		public uint costPerSpace;
		public int speedBandAdjust;
		public List<Trait> traits;
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
						agility = 1,
						minSpaces = 1,
						maxSpaces = 3,
						costPerSpace = 750,
						speedBandAdjust = 1,
						traits = new List<Trait>
						{
							Trait.Open_Vehicle,
						}
					},
					new Option
					{
						name = "Monowheel",
						techLevel = 9,
						agility =2,
						minSpaces = 1,
						maxSpaces = 3,
						costPerSpace = 2500,
						speedBandAdjust = 1,
						traits = new List<Trait>
						{
							Trait.Open_Vehicle,
						},
					},
					new Option
					{
						name = "Rail Rider (No Wheels)",
						agility = -2,
						costPerSpace = 400,
						speedBandAdjust = 1,
					},
					new Option
					{
						name = "Rail Rider (Plus Wheels)*",
						// special: -2 agi on rails only
						//agility = -2,
						costPerSpace = 400,
						// special: speed band +1 when on rails
						// speedBandAdjust = 1,
						 spaceConsumption = 1,
					},
					new Option
					{
						name = "Rough Terrain (Off Road)",
						costPerSpace = 100,
						traits = new List<Trait>
						{
							Trait.Off_Roader,
						},
					},
					new Option
					{
						name = "Rough Terrain (ATV)",
						costPerSpace = 250,
						traits = new List<Trait>
						{
							Trait.ATV,
						},
					},
					new Option
					{
						name = "Tracks",
						techLevel = 5,
						skill = Skill.Drive_Track,
						costPerSpace = 750,
						speedBandAdjust = -1,
						traits = new List<Trait>
						{
							Trait.Tracked,
						}
					}
				},
			},
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
						costPerSpace = 3000,
						speedBandAdjust = -1,
						traits = new List<Trait>
						{
							Trait.AFV,
							Trait.Off_Roader,
						},
					},
					new Option
					{
						name = "Rail Rider (No Wheels)",
						agility = -2,
						costPerSpace = 1000,
						speedBandAdjust = 1,
					},
					new Option
					{
						name = "Rail Rider (Plus Wheels)*",
						// special: -2 agi on rails only
						//agility = -2,
						costPerSpace = 1000,
						// special: speed band +1 when on rails
						// speedBandAdjust = 1,
						 spaceConsumption = 1,
					},
					new Option
					{
						name = "Rough Terrain (Off Road)",
						costPerSpace = 500,
						traits = new List<Trait>
						{
							Trait.Off_Roader,
						},
					},
					new Option
					{
						name = "Rough Terrain (ATV)",
						costPerSpace = 1000,
						traits = new List<Trait>
						{
							Trait.ATV,
						},
					},
					new Option
					{
						name = "Tracks*",
						techLevel = 5,
						skill = Skill.Drive_Track,
						costPerSpace = 2000,
						// Special: speed band adjust only if NOT AFV
						// speedBandAdjust = -1,
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
						costPerSpace = 25000,
						speedBandAdjust = -1,
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
						agility = 1,
						minSpaces = 1,
						maxSpaces = 3,
						costPerSpace = 10000,
						speedBandAdjust = 1,
						traits = new List<Trait>
						{
							Trait.Open_Vehicle,
						}
					},
					new Option
					{
						name = "Streamlined",
						agility = 1,
						costPerSpace = 30000,
						speedBandAdjust = 1,
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
						costPerSpace = 100000,
						speedBandAdjust = -1,
						traits = new List<Trait>
						{
							Trait.AFV,
						},
					},
					new Option
					{
						name = "Streamlined",
						costPerSpace = 50000,
						speedBandAdjust = 1,
					}
				}
			},
		};
	}
}