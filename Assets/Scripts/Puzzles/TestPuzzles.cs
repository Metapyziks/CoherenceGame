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
        public override bool ShouldAccept(bool[] input, bool[] output)
        {
            return input[0] != output[0];
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
        public override bool ShouldAccept(bool[] input, bool[] output)
        {
            return input.Any(x => x) == output[0];
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
        public override bool ShouldAccept(bool[] input, bool[] output)
        {
            return input.All(x => x) == output[0];
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
        public override bool ShouldAccept(bool[] input, bool[] output)
        {
            return (input[0] != input[1]) == output[0];
        }
    }
}
