using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

public delegate bool AcceptorDelegate(bool[] input, bool[] output);

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

[AttributeUsage(AttributeTargets.Method)]
public class PuzzleAttribute : Attribute
{
    public String Name { get; private set; }

    public String Description { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int InputCount { get; private set; }

    public int OutputCount { get; private set; }

    public PuzzleAttribute(String name, String desc, Difficulty diff, int width, int height, int inputs, int outputs)
    {
        Name = name;
        Description = desc;
        Difficulty = diff;

        Width = width;
        Height = height;

        InputCount = inputs;
        OutputCount = outputs;
    }
}

public class Puzzle
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

    static bool IsPuzzleMethod(MethodInfo method)
    {
        return method.GetCustomAttributes(typeof(PuzzleAttribute), false).Length > 0;
    }

    static PuzzleAttribute GetPuzzleAttribute(MethodInfo method)
    {
        return (PuzzleAttribute) method.GetCustomAttributes(typeof(PuzzleAttribute), false)[0];
    }

    static Puzzle()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!IsPuzzleCategoryType(type)) continue;

            var cat = GetPuzzleCategoryAttribute(type);

            foreach (var method in type.GetMethods()) {
                if (!IsPuzzleMethod(method)) continue;

                var puz = GetPuzzleAttribute(method);

                Register(new Puzzle(
                    puz.Name, cat.CategoryName, puz.Description, puz.Difficulty,
                    puz.Width, puz.Height, puz.InputCount, puz.OutputCount,
                    (input, output) => (bool) method.Invoke(null, new object[] { input, output })
                ));
            }
        }
    }

    public static void Register(Puzzle puzzle)
    {
        if (!_sPuzzles.ContainsKey(puzzle.Category)) {
            _sPuzzles.Add(puzzle.Category, new List<Puzzle>());
        }

        _sPuzzles[puzzle.Category].Add(puzzle);
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

    private AcceptorDelegate _acceptor;

    public String Name { get; private set; }

    public String Category { get; private set; }

    public String Description { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int InputCount { get; private set; }

    public int OutputCount { get; private set; }

    public Puzzle(String name, String category, String desc, Difficulty diff, int width, int height,
        int inputs, int outputs, AcceptorDelegate acceptor)
    {
        Name = name;
        Category = category;
        Description = desc;
        Difficulty = diff;

        Width = width;
        Height = height;

        InputCount = inputs;
        OutputCount = outputs;

        _acceptor = acceptor;
    }

    public bool ShouldAccept(Spin[] input, Spin[] output)
    {
        return _acceptor(
            input.Select(x => x == Spin.Up).ToArray(),
            output.Select(x => x == Spin.Up).ToArray());
    }
}
