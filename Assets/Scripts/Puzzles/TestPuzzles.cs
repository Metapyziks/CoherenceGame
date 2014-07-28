using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[PuzzleCategory("Test")]
public class TestPuzzles
{
    [Puzzle(
        index: 0,
        name: "Invert",
        desc: "Output RED for BLUE inputs, and output BLUE for RED inputs.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Output("Q", 0)]
    public class Invert : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return output[0] != Spin.None && input[0] != output[0];
        }
    }

    [Puzzle(
        index: 1,
        name: "OR Gate",
        desc: "Output RED if either input is RED, otherwise output BLUE.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Input("B", 2), Output("Q", 0)]
    public class OrGate : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return output[0] != Spin.None && input.Any(x => x == Spin.Up) == (output[0] == Spin.Up);
        }
    }

    [Puzzle(
        index: 2,
        name: "AND Gate",
        desc: "Output RED if both inputs are RED, otherwise output BLUE.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Input("B", 2), Output("Q", 0)]
    public class AndGate : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return output[0] != Spin.None && input.All(x => x == Spin.Up) == (output[0] == Spin.Up);
        }
    }

    [Puzzle(
        index: 3,
        name: "XOR Gate",
        desc: "Output RED if only one input is RED, otherwise output BLUE.",
        diff: Difficulty.Easy,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Input("B", 2), Output("Q", 0)]
    public class XorGate : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return output[0] != Spin.None && (input[0] != input[1]) == (output[0] == Spin.Up);
        }
    }

    [Puzzle(
        index: 4,
        name: "Only Red",
        desc: "Output one RED for each input of either color.",
        diff: Difficulty.Easy,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Output("Q", 0)]
    public class OnlyRed : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return output[0] == Spin.Up;
        }
    }

    [Puzzle(
        index: 5,
        name: "No blues",
        desc: "Output anything for a RED input, but nothing for a BLUE one.",
        diff: Difficulty.Easy,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Output("Q", 0)]
    public class NoBlues : Puzzle
    {
        public override IEnumerable<bool> ShouldAccept(IEnumerable<Spin[]> inputs, IEnumerable<Spin[]> outputs)
        {
            var inpIter = inputs.GetEnumerator();
            var outIter = outputs.GetEnumerator();

            while (inpIter.MoveNext()) {
                if (inpIter.Current[0] == Spin.Down) continue;

                if (!outIter.MoveNext()) {
                    yield return false;
                    yield break;
                }

                yield return outIter.Current[0] != Spin.None;
            }

            if (outIter.MoveNext()) yield return false;
        }
    }

    [Puzzle(
        index: 6,
        name: "Sandbox",
        desc: "Try to break the game.",
        diff: Difficulty.Extreme,
        width: 33, height: 33,
        inputPeriod: 2
    )]
    [Input("A1", 0), Input("A2", 2), Input("A3", 4), Input("A4", 6), Input("B1", 10), Input("B2", 12), Input("B3", 14), Input("B4", 16)]
    [Output("Q1", 0), Output("Q2", 2), Output("Q3", 4), Output("Q4", 6)]
    public class Sandbox : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return true;
        }
    }
}
