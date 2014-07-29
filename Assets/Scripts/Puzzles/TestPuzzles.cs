using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[PuzzleCategory("Test")]
public class TestPuzzles
{
    [Puzzle(
        index: 0,
        name: "Pass through",
        desc: "Output something for each input.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Output("Q", 0)]
    public class PassThrough : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return output[0] != Spin.None;
        }
    }

    [Puzzle(
        index: 1,
        name: "Split",
        desc: "Output something through both outputs simultaneously for each input.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Output("Q1", 0), Output("Q2", 2)]
    public class Split : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            return output[0] != Spin.None && output[1] != Spin.None;
        }
    }

    [Puzzle(
        index: 2,
        name: "Flip",
        desc: "Output RED for BLUE inputs, and output BLUE for RED inputs.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2,
        solution: "//////////8fPwh4////////////AQ=="
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
        index: 3,
        name: "At least one RED",
        desc: "Output RED if either input is RED, otherwise output BLUE.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2,
        solution: "//////////8B/w8A/P//////////AQ=="
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
        index: 4,
        name: "Crossover",
        desc: "Send each input to the output in the opposite corner of the screen, with both arriving simultaneously.",
        diff: Difficulty.Easy,
        width: 13, height: 13,
        inputPeriod: 2
    )]
    [Input("A", 0), Input("B", 6), Output("Q1", 0), Output("Q2", 6)]
    public class Crossover : Puzzle
    {
        public override bool ShouldAccept(Spin[] input, Spin[] output)
        {
            //Debug.Log("A: " + input[0] + ", " + output[0]);
            //Debug.Log("B: " + input[1] + ", " + output[1]);

            return input[0] == output[1] && input[1] == output[0];
        }
    }

    [Puzzle(
        index: 5,
        name: "Exactly two REDs",
        desc: "Output RED if both inputs are RED, otherwise output BLUE.",
        diff: Difficulty.Trivial,
        width: 13, height: 13,
        inputPeriod: 2,
        solution: "//////////2B+EcA+t//////////AQ=="
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
        index: 6,
        name: "Only RED",
        desc: "Output one RED for each input of either color.",
        diff: Difficulty.Easy,
        width: 13, height: 13,
        inputPeriod: 2,
        solution: "//////////8fPwh4/L//////////AQ=="
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
        index: 7,
        name: "Exactly one RED",
        desc: "Output RED if only one input is RED, otherwise output BLUE.",
        diff: Difficulty.Easy,
        width: 13, height: 13,
        inputPeriod: 2,
        solution: "/////////+hB/CMA8Y/+////////AQ=="
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
        index: 8,
        name: "No BLUE",
        desc: "Output anything for a RED input, but nothing for a BLUE one.",
        diff: Difficulty.Easy,
        width: 13, height: 13,
        inputPeriod: 2,
        solution: "//////////8HPy5Y8KP+////////AQ=="
    )]
    [Input("A", 0), Output("Q", 0)]
    public class NoBlues : Puzzle
    {
        public override int GetExpectedOutputCount(IEnumerable<Spin[]> input)
        {
            return input.Count(x => x[0] == Spin.Up);
        }

        public override bool ShouldAccept(IEnumerable<Spin[]> inputs, IEnumerable<Spin[]> outputs)
        {
            return outputs.Count() == inputs.Count(x => x[0] == Spin.Up);
        }
    }

    [Puzzle(
        index: 9,
        name: "Sandbox",
        desc: "Try to break the game.",
        diff: Difficulty.Extreme,
        width: 25, height: 25,
        inputPeriod: 2
    )]
    [Input("A1", 0), Input("A2", 2), Input("A3", 4), Input("A4", 6), Input("B1", 10), Input("B2", 12), Input("B3", 14), Input("B4", 16)]
    [Output("Q1", 0), Output("Q2", 2), Output("Q3", 4), Output("Q4", 6)]
    public class Sandbox : Puzzle
    {
        public override bool ShouldAccept(IEnumerable<Spin[]> inputs, IEnumerable<Spin[]> outputs)
        {
            return true;
        }
    }
}
