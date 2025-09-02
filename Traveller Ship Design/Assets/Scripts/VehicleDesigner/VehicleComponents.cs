using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace AtomosZ.MG2eTraveller.Vehicle
{
	[Serializable]
	public class Chasis
	{
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
		private void Start()
		{
			var reader = new StreamReader(@"Chasis.tson");
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith("//") || string.IsNullOrEmpty(line))
					continue;
				Console.WriteLine(line);

				var values = line.Split(',');
				var chasis = new Chasis
				{
					name = values[0].Trim(),
					skill = values[1].Trim(),
					techLevel = uint.Parse(values[2]),
					agility = int.Parse(values[3]),
					costPerSpace = uint.Parse(values[5]),
					hullPerSpace = uint.Parse(values[6]),
					shippingTonsPerSpace = float.Parse(values[7]),
				};

				var spaces = values[4].Split('-');
				chasis.minSpace = uint.Parse(spaces[0]);
				chasis.maxSpace = uint.Parse(spaces[1]);

				var examples = values[9].Split('"');
				chasis.examples = new List<string>();
				foreach (var ex in examples)
					if (!string.IsNullOrEmpty(ex))
						chasis.examples.Add(ex.Trim());


				line = reader.ReadLine();

				PrintChasis(chasis);
			}
		}

		//[DebuggerHidden] // debugger will not step into this method
		[Conditional("DEBUG")] // turn this function into a NOP outside of DEBUG builds
		private static void PrintChasis(Chasis chasis)
		{
			Console.WriteLine("Name:\t\t" + chasis.name);

			Console.WriteLine("Tech Level:\t" + chasis.techLevel);
			Console.WriteLine("Skill:\t\t" + chasis.skill);

			Console.WriteLine("agility:\t" + (chasis.agility >= 0 ? "+" : "-") + chasis.agility);
			Console.WriteLine("Spaces:\t\t" + chasis.minSpace + "-" + chasis.maxSpace);
			Console.WriteLine("Cost per Space:\tCr" + chasis.costPerSpace);
			Console.WriteLine("Hull:\t\t" + chasis.hullPerSpace + " per Space");
			Console.WriteLine("Shipping:\t" + chasis.shippingTonsPerSpace.ToString("f1") + " tons per Space");
			Console.WriteLine("Traits:\t" + (chasis.traits == null ? "None" : "display traits..."));
			Console.WriteLine("Examples:\t" + chasis.examples);
		}
	}
}