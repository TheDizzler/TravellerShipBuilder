using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using UnityEngine;

namespace AtomosZ.MG2eTraveller.Vehicle
{
	public enum ChasisSize
	{
		Light,
		Heavy
	}
	public enum ChasisType
	{
		LightGround,
		HeavyGround,
		LightGrav,
		HeavyGrav,
		UnpoweredGround,
		UnpoweredBoat,
		PoweredBoat,
		Ship,
		LightSubmersible,
		HeavySubmersible,
		Airship,
		LightAeroplane,
		HeavyAeroplane,
		LightJet,
		HeavyJet,
		/// <summary>
		/// Helicopter, Aerodyne, Ornithopter
		/// </summary>
		RotorFlyer,
		LightWalker,
		HeavyWalker,
		LightHovercraft,
		HeavyHovercraft,
	}

	[Serializable]
	public class Chassis
	{
		public ChasisType type;
		public string name;
		public string skill;
		public uint techLevel;
		public int agility;
		public uint minSpace;
		public uint maxSpace;
		public uint costPerSpace;
		public uint hullPerSpace;
		public float shippingTonsPerSpace;
		public List<Trait> traits;
		public List<string> examples;
	}

	public class Trait
	{

	}

	public class VehicleComponents : MonoBehaviour
	{
		public List<Chassis> chasisList = new()
		{
			//name = "
		};
	}
}