﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

public enum Difficulty : byte
{
    Tutorial = 0,
    Trivial = 1,
    Easy = 2,
    Medium = 3,
    Hard = 4,
    Extreme = 5
}

[AttributeUsage(AttributeTargets.Class)]
public class PuzzleCategoryAttribute : Attribute
{
    public String CategoryName { get; private set; }

    public PuzzleCategoryAttribute(String name)
    {
        CategoryName = name;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PuzzleAttribute : Attribute
{
    public int Index { get; private set; }

    public String Name { get; private set; }

    public String Description { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int InputPeriod { get; private set; }

    public String Solution { get; private set; }

    public PuzzleAttribute(int index, String name, String desc, Difficulty diff, int width, int height,
        int inputPeriod = 2, string solution = null)
    {
        Index = index;
        Name = name;
        Description = desc;
        Difficulty = diff;

        Width = width;
        Height = height;

        InputPeriod = inputPeriod;
        Solution = solution;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public abstract class InputOutputAttribute : Attribute
{
    public String Name { get; private set; }

    public int Placement { get; private set; }

    public InputOutputAttribute(String name, int placement)
    {
        Name = name;
        Placement = placement;
    }
}

public class InputAttribute : InputOutputAttribute
{
    public InputAttribute(String name, int placement) : base(name, placement) { }
}

public class OutputAttribute : InputOutputAttribute
{
    public OutputAttribute(String name, int placement) : base(name, placement) { }
}

public abstract class Puzzle : IComparable<Puzzle>
{
    private static Dictionary<String, List<Puzzle>> _sPuzzles
        = new Dictionary<string, List<Puzzle>>();

    static bool IsPuzzleCategoryType(Type type)
    {
        return type.GetCustomAttributes(typeof(PuzzleCategoryAttribute), false).Length > 0;
    }

    static PuzzleCategoryAttribute GetPuzzleCategoryAttribute(Type type)
    {
        return (PuzzleCategoryAttribute) type.GetCustomAttributes(typeof(PuzzleCategoryAttribute), false)[0];
    }

    static bool IsPuzzleType(Type type)
    {
        return typeof(Puzzle).IsAssignableFrom(type) && type.GetCustomAttributes(typeof(PuzzleAttribute), false).Length > 0;
    }

    static PuzzleAttribute GetPuzzleAttribute(Type type)
    {
        return (PuzzleAttribute) type.GetCustomAttributes(typeof(PuzzleAttribute), false)[0];
    }

    static Puzzle()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!IsPuzzleCategoryType(type)) continue;

            var cat = GetPuzzleCategoryAttribute(type);
                        
            foreach (var subType in type.GetNestedTypes()) {
                if (!IsPuzzleType(subType)) continue;

                var puz = GetPuzzleAttribute(subType);
                var ctor = subType.GetConstructor(new Type[0]);

                if (ctor == null) continue;

                var puzzle = (Puzzle) ctor.Invoke(new object[0]);

                puzzle.Index = puz.Index;
                puzzle.Name = puz.Name;
                puzzle.Category = cat.CategoryName;
                puzzle.Description = puz.Description;
                puzzle.Difficulty = puz.Difficulty;
                puzzle.Width = puz.Width;
                puzzle.Height = puz.Height;
                puzzle.InputPeriod = puz.InputPeriod;
                puzzle.Solution = puz.Solution;

                var inputs = subType.GetCustomAttributes(typeof(InputAttribute), true)
                    .Cast<InputAttribute>()
                    .OrderBy(x => x.Placement)
                    .ToArray();

                int from = (puz.Height - inputs.Max(x => x.Placement)) / 2;

                puzzle.InputLocations = inputs.Select(x => from + x.Placement).ToArray();
                puzzle.InputNames = inputs.Select(x => x.Name).ToArray();

                var outputs = subType.GetCustomAttributes(typeof(OutputAttribute), true)
                    .Cast<OutputAttribute>()
                    .OrderBy(x => x.Placement)
                    .ToArray();

                from = (puz.Height - outputs.Max(x => x.Placement)) / 2;

                puzzle.OutputLocations = outputs.Select(x => from + x.Placement).ToArray();
                puzzle.OutputNames = outputs.Select(x => x.Name).ToArray();

                Register(puzzle);
            }
        }

        foreach (var cat in _sPuzzles.Values) {
            if (cat.Count == 0) continue;

            if (cat.Any(x => cat.Any(y => x != y && x.Index == y.Index)
                || (x.Index > 0 && !cat.Any(y => y.Index + 1 == x.Index)))) {
                throw new Exception("Invalid puzzle indices in category " + cat.First().Category);
            }
        }
    }

    public static void Register(Puzzle puzzle)
    {
        if (!_sPuzzles.ContainsKey(puzzle.Category)) {
            _sPuzzles.Add(puzzle.Category, new List<Puzzle>());
        }

        _sPuzzles[puzzle.Category].Add(puzzle);
        _sPuzzles[puzzle.Category].Sort();
    }

    public static String[] GetCategories()
    {
        return _sPuzzles.Keys.ToArray();
    }

    public static Puzzle[] GetPuzzlesInCategory(String category)
    {
        if (!_sPuzzles.ContainsKey(category)) {
            return new Puzzle[0];
        }

        return _sPuzzles[category].ToArray();
    }

    public int Index { get; private set; }

    public String Name { get; private set; }

    public String Category { get; private set; }

    public String Description { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int InputPeriod { get; private set; }

    public String Solution { get; private set; }

    public int InputCount { get { return InputLocations.Length; } }

    public int OutputCount { get { return OutputLocations.Length; } }

    public int[] InputLocations { get; private set; }

    public int[] OutputLocations { get; private set; }

    public String[] InputNames { get; private set; }

    public String[] OutputNames { get; private set; }

    public bool Solved
    {
        get { return PlayerPrefs.HasKey(SolvedKeyName) && PlayerPrefs.GetInt(SolvedKeyName) > 0; }
        set { PlayerPrefs.SetInt(SolvedKeyName, value ? 1 : 0); }
    }

    public String SolvedKeyName
    {
        get { return SaveKeyName + "." + "Solved"; }
    }

    public String SaveKeyName
    {
        get { return Category.Replace(" ", "") + "." + Name.Replace(" ", ""); }
    }
    
    public virtual IEnumerable<Spin[]> GenerateInputs(System.Random rand, int count)
    {
        for (int i = 0; count == 0 || i < count; ++i) {
            int val = rand == null ? i : rand.Next(1 << InputCount);

            yield return Enumerable.Range(0, InputCount)
                .Select(x => ((val >> x) & 1) == 1 ? Spin.Up : Spin.Down)
                .ToArray();
        }
    }

    public virtual int GetExpectedOutputCount(IEnumerable<Spin[]> input)
    {
        return input.Count();
    }
    
    public virtual bool ShouldAccept(IEnumerable<Spin[]> inputs, IEnumerable<Spin[]> outputs)
    {
        var inIter = inputs.GetEnumerator();
        var outIter = outputs.GetEnumerator();

        while (inIter.MoveNext() && outIter.MoveNext()) {
            if (!ShouldAccept(inIter.Current, outIter.Current)) return false;
        }

        return !(inIter.MoveNext() || outIter.MoveNext());
    }

    public virtual bool ShouldAccept(Spin[] input, Spin[] output)
    {
        return false;
    }

    public int CompareTo(Puzzle other)
    {
        return Index - other.Index;
    }
}
