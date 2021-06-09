using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomCritters
{
    public class CritterEntry
    {
        public string Id { get; set; }
        public class SpriteData_
        {
            public int Variations { get; set; }
            public int FrameWidth { get; set; }
            public int FrameHeight { get; set; }
            public float Scale { get; set; } = 4;
            public Boolean Flying { get; set; } = true;
        }
        public SpriteData_ SpriteData { get; set; } = new SpriteData_();

        public class Animation_
        {
            public class AnimationFrame_
            {
                public int Frame;
                public int Duration;
            }

            public List<AnimationFrame_> Frames = new List<AnimationFrame_>();
        }
        public Dictionary<string, Animation_> Animations { get; set; } = new Dictionary<string, Animation_>();

        public class SpawnCondition_
        {
            public Boolean Not { get; set; } = false;
            public string[] Seasons { get; set; } = new string[0];
            public string[] Locations { get; set; } = new string[0];
            public int MinTimeOfDay { get; set; } = -1;
            public int MaxTimeOfDay { get; set; } = -1;
            public double ChancePerTile { get; set; } = 1.0 / 15000;
            public bool RequireDarkOut { get; set; } = false;
            public bool AllowRain { get; set; } = false;
            public string ChildrenCombine { get; set; } = "and";
            public List<SpawnCondition_> Children { get; set; } = new List<SpawnCondition_>();

            public bool check(GameLocation loc)
            {
                bool ret = true;

                if (this.Children.Count > 0)
                {
                    if (this.ChildrenCombine != "and")
                        ret = false;

                    int totalMet = 0;
                    foreach (var child in this.Children)
                    {
                        bool childCheck = child.check(loc);
                        if (childCheck)
                            ++totalMet;

                        switch (this.ChildrenCombine)
                        {
                            case "and": ret = ret && childCheck; break;
                            case "or": ret = ret || childCheck; break;
                            case "xor": ret = ret ^ childCheck; break;
                        }
                    }

                    if (this.ChildrenCombine.StartsWith("atleast"))
                    {
                        ret = totalMet >= int.Parse(this.ChildrenCombine.Substring(7));
                    }
                    else if (this.ChildrenCombine.StartsWith("exactly"))
                    {
                        ret = totalMet == int.Parse(this.ChildrenCombine.Substring(7));
                    }
                    else if (this.ChildrenCombine != "and" && this.ChildrenCombine != "or" && this.ChildrenCombine != "xor")
                    {
                        throw new ArgumentException("Bad ChildrenCombine: " + this.ChildrenCombine);
                    }
                }
                else if (this.MinTimeOfDay != -1 && Game1.timeOfDay < this.MinTimeOfDay)
                    ret = false;
                else if (this.MaxTimeOfDay != -1 && Game1.timeOfDay > this.MaxTimeOfDay)
                    ret = false;
                else if (this.Seasons != null && this.Seasons.Count() > 0 && !this.Seasons.Contains(Game1.currentSeason))
                    ret = false;
                else if (this.Locations != null && this.Locations.Count() > 0 && !this.Locations.Contains(loc.Name))
                    ret = false;
                else if (Game1.random.NextDouble() >= Math.Max(0.15, (Math.Min(0.5, loc.map.Layers[0].LayerWidth * loc.map.Layers[0].LayerHeight / this.ChancePerTile))))
                    ret = false;
                else if (this.RequireDarkOut && !Game1.isDarkOut())
                    ret = false;
                else if (!this.AllowRain && Game1.isRaining)
                    ret = false;

                if (this.Not)
                    ret = !ret;
                return ret;
            }
        }
        public List<SpawnCondition_> SpawnConditions { get; set; } = new List<SpawnCondition_>();

        public class Behavior_
        {
            public string Type { get; set; }
            public float Speed { get; set; }

            public class PatrolPoint_
            {
                public string Type { get; set; } = "start";
                public float X { get; set; }
                public float Y { get; set; }
            }
            public List<PatrolPoint_> PatrolPoints { get; set; } = new List<PatrolPoint_>();
            public int PatrolPointDelay { get; set; }
            public int PatrolPointDelayAddRandom { get; set; }
        }
        public Behavior_ Behavior { get; set; }

        public class SpawnLocation_
        {
            public string LocationType { get; set; } = "random";
            //public Vector2 Offset { get; set; } = new Vector2();

            public class ConditionEntry_
            {
                public bool Not { get; set; } = false;

                public double Chance { get; set; } = 1.0;
                public string Variable { get; set; }
                public bool RequireNotNull { get; set; }
                public string Is { get; set; }
                public string ValueEquals { get; set; }

                public string ChildrenCombine { get; set; } = "and";
                public List<ConditionEntry_> Children { get; set; } = new List<ConditionEntry_>();

                public bool check(object obj)
                {
                    bool ret = true;

                    if (this.Children.Count > 0)
                    {
                        if (this.ChildrenCombine != "and")
                            ret = false;

                        int totalMet = 0;
                        foreach (var child in this.Children)
                        {
                            bool childCheck = child.check(obj);
                            if (childCheck)
                                ++totalMet;

                            switch (this.ChildrenCombine)
                            {
                                case "and": ret = ret && childCheck; break;
                                case "or": ret = ret || childCheck; break;
                                case "xor": ret = ret ^ childCheck; break;
                            }
                        }

                        if (this.ChildrenCombine.StartsWith("atleast"))
                        {
                            ret = totalMet >= int.Parse(this.ChildrenCombine.Substring(7));
                        }
                        else if (this.ChildrenCombine.StartsWith("exactly"))
                        {
                            ret = totalMet == int.Parse(this.ChildrenCombine.Substring(7));
                        }
                        else if (this.ChildrenCombine != "and" && this.ChildrenCombine != "or" && this.ChildrenCombine != "xor")
                        {
                            throw new ArgumentException("Bad ChildrenCombine: " + this.ChildrenCombine);
                        }
                    }
                    else
                    {
                        if (this.Chance != 1.0 && Game1.random.NextDouble() > this.Chance)
                            ret = false;
                        if (this.Variable != null && this.Variable != "")
                        {
                            string[] toks = this.Variable.Split('.');

                            var o = obj;
                            for (int i = 0; i < toks.Length; ++i)
                            {
                                if (o == null)
                                    break;
                                var f = o.GetType().GetField(toks[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                if (f == null)
                                {
                                    o = null;
                                    break;
                                }

                                o = f.GetValue(o);
                            }

                            if (o != null)
                            {
                                if (this.Is != null && this.Is != "" && !o.GetType().IsInstanceOfType(Type.GetType(this.Is)))
                                    ret = false;
                                else if (this.ValueEquals != null && this.ValueEquals != "" && !o.ToString().Equals(this.ValueEquals))
                                    ret = false;
                            }
                            else if (this.RequireNotNull)
                                ret = false;
                        }
                    }

                    if (this.Not)
                        ret = !ret;

                    return ret;
                }
            }
            public List<ConditionEntry_> Conditions { get; set; } = new List<ConditionEntry_>();

            public bool check(object obj)
            {
                foreach (var cond in this.Conditions)
                {
                    if (!cond.check(obj))
                        return false;
                }

                return true;
            }

            public Vector2? pickSpot(GameLocation loc)
            {
                if (this.LocationType == "random")
                {
                    if (this.check(null))
                        return loc.getRandomTile() * Game1.tileSize;
                    return null;
                }
                else if (this.LocationType == "terrainfeature")
                {
                    var keys = loc.terrainFeatures.Keys.ToList();
                    keys.Shuffle();
                    foreach (var key in keys)
                    {
                        if (this.check(loc.terrainFeatures[key]))
                            return key * Game1.tileSize;
                    }

                    return null;
                }
                else if (this.LocationType == "object")
                {
                    var keys = loc.objects.Keys.ToList();
                    keys.Shuffle();
                    foreach (var key in keys)
                    {
                        if (this.check(loc.objects[key]))
                            return key * Game1.tileSize;
                    }

                    return null;
                }
                else throw new ArgumentException("Bad location type");
            }
        }
        public List<SpawnLocation_> SpawnLocations { get; set; } = new List<SpawnLocation_>();

        public int SpawnAttempts { get; set; } = 3;

        public class Light_
        {
            public int VanillaLightId = 3;
            public float Radius { get; set; } = 0.5f;
            public class Color_
            {
                public int R { get; set; } = 255;
                public int G { get; set; } = 255;
                public int B { get; set; } = 255;
            }
            public Color_ Color { get; set; } = new Color_();
        }
        public Light_ Light { get; set; } = null;

        public virtual bool check(GameLocation loc)
        {
            foreach (var cond in this.SpawnConditions)
            {
                if (!cond.check(loc))
                    return false;
            }

            return true;
        }

        public virtual Vector2? pickSpot(GameLocation loc)
        {
            foreach (var sl in this.SpawnLocations)
            {
                var ret = sl.pickSpot(loc);
                if (ret.HasValue)
                    return ret.Value;
            }
            return null;
        }

        public virtual Critter makeCritter(Vector2 pos)
        {
            return new CustomCritter(pos + new Vector2(1, 1) * (Game1.tileSize / 2), this);
        }

        internal static Dictionary<string, CritterEntry> critters = new Dictionary<string, CritterEntry>();
        public static void Register(CritterEntry entry)
        {
            CritterEntry.critters.Add(entry.Id, entry);
        }
    }
}
